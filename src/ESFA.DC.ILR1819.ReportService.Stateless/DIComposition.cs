using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.Auditing;
using ESFA.DC.Auditing.Dto;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.CollectionsManagement.Services;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Reports;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Configuration;
using ESFA.DC.ILR1819.ReportService.Service;
using ESFA.DC.ILR1819.ReportService.Service.Builders;
using ESFA.DC.ILR1819.ReportService.Service.BusinessRules;
using ESFA.DC.ILR1819.ReportService.Service.Helper;
using ESFA.DC.ILR1819.ReportService.Service.Reports;
using ESFA.DC.ILR1819.ReportService.Service.Service;
using ESFA.DC.ILR1819.ReportService.Stateless.Configuration;
using ESFA.DC.ILR1819.ReportService.Stateless.Handlers;
using ESFA.DC.ILR1819.ReportService.Stateless.Mappers;
using ESFA.DC.ILR1819.ReportService.Stateless.Modules;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis;
using ESFA.DC.IO.Redis.Config.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using ESFA.DC.ServiceFabric.Helpers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.ILR1819.ReportService.Stateless
{
    public class DIComposition
    {
        public static ContainerBuilder BuildContainer(IConfigurationHelper configHelper)
        {
            var containerBuilder = new ContainerBuilder();

            var topicAndTaskOptions = configHelper.GetSectionValues<TopicAndTaskSectionOptions>("TopicAndTaskSection");
            containerBuilder.RegisterInstance(topicAndTaskOptions).As<ITopicAndTaskSectionOptions>().SingleInstance();

            var larsConfiguration = configHelper.GetSectionValues<LarsConfiguration>("LarsSection");
            containerBuilder.RegisterInstance(larsConfiguration).As<ILarsConfiguration>().SingleInstance();

            var orgConfiguration = configHelper.GetSectionValues<OrgConfiguration>("OrgSection");
            containerBuilder.RegisterInstance(orgConfiguration).As<IOrgConfiguration>().SingleInstance();

            var collectionsManagementConfiguration =
                configHelper.GetSectionValues<CollectionsManagementConfiguration>("CollectionsManagementSection");
            containerBuilder.RegisterInstance(collectionsManagementConfiguration)
                .As<ICollectionsManagementConfiguration>().SingleInstance();

            // register Cosmos config
            var azureRedisOptions = configHelper.GetSectionValues<RedisOptions>("RedisSection");
            containerBuilder.Register(c => new RedisKeyValuePersistenceConfig(
                    azureRedisOptions.RedisConnectionString))
                .As<IRedisKeyValuePersistenceServiceConfig>().SingleInstance();

            containerBuilder.RegisterType<RedisKeyValuePersistenceService>()
                .Keyed<IKeyValuePersistenceService>(PersistenceStorageKeys.Redis)
                .InstancePerLifetimeScope();

            // register azure blob storage service
            var azureBlobStorageOptions = configHelper.GetSectionValues<AzureStorageOptions>("AzureStorageSection");
            containerBuilder.Register(c =>
                    new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        azureBlobStorageOptions.AzureBlobContainerName))
                .As<IAzureStorageKeyValuePersistenceServiceConfig>().SingleInstance();

            containerBuilder.RegisterType<AzureStorageKeyValuePersistenceService>()
                .Keyed<IKeyValuePersistenceService>(PersistenceStorageKeys.Blob)
                .As<IStreamableKeyValuePersistenceService>()
                .InstancePerLifetimeScope();

            // register serialization
            containerBuilder.RegisterType<JsonSerializationService>()
                .As<IJsonSerializationService>();
            containerBuilder.RegisterType<XmlSerializationService>()
                .As<IXmlSerializationService>();

            // get ServiceBus, Azurestorage config values and register container
            var serviceBusOptions =
                configHelper.GetSectionValues<ServiceBusOptions>("ServiceBusSettings");
            containerBuilder.RegisterInstance(serviceBusOptions).As<ServiceBusOptions>().SingleInstance();

            // Version info
            var versionInfo = configHelper.GetSectionValues<VersionInfo>("VersionSection");
            containerBuilder.RegisterInstance(versionInfo).As<IVersionInfo>().SingleInstance();

            // register logger
            var loggerOptions =
                configHelper.GetSectionValues<LoggerOptions>("LoggerSection");
            containerBuilder.RegisterInstance(loggerOptions).As<LoggerOptions>().SingleInstance();
            containerBuilder.RegisterModule<LoggerModule>();

            // auditing
            var auditPublishConfig = new ServiceBusQueueConfig(
                serviceBusOptions.ServiceBusConnectionString,
                serviceBusOptions.AuditQueueName,
                Environment.ProcessorCount);
            containerBuilder.Register(c => new QueuePublishService<AuditingDto>(
                    auditPublishConfig,
                    c.Resolve<IJsonSerializationService>()))
                .As<IQueuePublishService<AuditingDto>>();
            containerBuilder.RegisterType<Auditor>().As<IAuditor>();

            // get job status queue config values and register container
            var jobStatusQueueOptions =
                configHelper.GetSectionValues<JobStatusQueueOptions>("JobStatusSection");
            containerBuilder.RegisterInstance(jobStatusQueueOptions).As<JobStatusQueueOptions>().SingleInstance();

            // Job Status Update Service
            var jobStatusPublishConfig = new JobStatusQueueConfig(
                jobStatusQueueOptions.JobStatusConnectionString,
                jobStatusQueueOptions.JobStatusQueueName,
                Environment.ProcessorCount);

            containerBuilder.Register(c => new QueuePublishService<JobStatusDto>(
                    jobStatusPublishConfig,
                    c.Resolve<IJsonSerializationService>()))
                .As<IQueuePublishService<JobStatusDto>>();
            containerBuilder.RegisterType<JobStatus.JobStatus>().As<IJobStatus>();

            // register Jobcontext services
            var topicConfig = new ServiceBusTopicConfig(
                serviceBusOptions.ServiceBusConnectionString,
                serviceBusOptions.TopicName,
                serviceBusOptions.ReportingSubscriptionName,
                Environment.ProcessorCount);
            containerBuilder.Register(c =>
            {
                var topicSubscriptionSevice =
                    new TopicSubscriptionSevice<JobContextDto>(
                        topicConfig,
                        c.Resolve<IJsonSerializationService>(),
                        c.Resolve<ILogger>(),
                        c.Resolve<IDateTimeProvider>());
                return topicSubscriptionSevice;
            }).As<ITopicSubscriptionService<JobContextDto>>();

            containerBuilder.Register(c =>
            {
                var topicPublishSevice =
                    new TopicPublishService<JobContextDto>(
                        topicConfig,
                        c.Resolve<IJsonSerializationService>());
                return topicPublishSevice;
            }).As<ITopicPublishService<JobContextDto>>();

            // register message mapper
            containerBuilder.RegisterType<JobContextMessageMapper>()
                .As<IMapper<JobContextMessage, JobContextMessage>>();

            // register MessageHandler
            containerBuilder.RegisterType<MessageHandler>().As<IMessageHandler>().InstancePerLifetimeScope();

            // register the  callback handle when a new message is received from ServiceBus
            containerBuilder.Register<Func<JobContextMessage, CancellationToken, Task<bool>>>(c =>
                c.Resolve<IMessageHandler>().Handle).InstancePerLifetimeScope();

            containerBuilder.RegisterType<EntryPoint>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextManagerForTopics<JobContextMessage>>().As<IJobContextManager<JobContextMessage>>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextMessage>().As<IJobContextMessage>()
                .InstancePerLifetimeScope();

            containerBuilder.Register(context =>
            {
                var settings = context.Resolve<ICollectionsManagementConfiguration>();
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(
                    settings.CollectionsManagementConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                return optionsBuilder.Options;
            })
                .As<DbContextOptions>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().InstancePerLifetimeScope();

            RegisterReports(containerBuilder);
            RegisterServices(containerBuilder);
            RegisterBuilders(containerBuilder);
            RegisterRules(containerBuilder);
            RegisterHelpers(containerBuilder);

            return containerBuilder;
        }

        private static void RegisterReports(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<AllbOccupancyReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<MainOccupancyReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FundingSummaryReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<SummaryOfFunding1619Report>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<MathsAndEnglishReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ValidationErrorsReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.Register(c => new List<IReport>(c.Resolve<IEnumerable<IReport>>()))
                .As<IList<IReport>>();
        }

        private static void RegisterServices(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<IlrProviderService>().As<IIlrProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LarsProviderService>().As<ILarsProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AllbProviderService>().As<IAllbProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FM25ProviderService>().As<IFM25ProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ReturnCalendarService>().As<IReturnCalendarService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ValidLearnersService>().As<IValidLearnersService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<InvalidLearnersService>().As<IInvalidLearnersService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<StringUtilitiesService>().As<IStringUtilitiesService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<OrgProviderService>().As<IOrgProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PeriodProviderService>().As<IPeriodProviderService>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterBuilders(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MathsAndEnglishModelBuilder>().As<IMathsAndEnglishModelBuilder>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterRules(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MathsAndEnglishFm25Rules>().As<IMathsAndEnglishFm25Rules>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterHelpers(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<IlrFileHelper>().As<IIlrFileHelper>()
                .InstancePerLifetimeScope();
        }
    }
}
