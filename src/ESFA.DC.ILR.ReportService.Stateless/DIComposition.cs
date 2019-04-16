using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.DASPayments.EF;
using ESFA.DC.DASPayments.EF.Interfaces;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.DataMatch;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Service;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.Builders.PeriodEnd;
using ESFA.DC.ILR.ReportService.Service.BusinessRules;
using ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Reports.PeriodEnd;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Service.Service.DataMatch;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.ILR1819.ReportService.Stateless.Configuration;
using ESFA.DC.ILR1819.ReportService.Stateless.Handlers;
using ESFA.DC.ILR1819.ReportService.Stateless.Interfaces;
using ESFA.DC.ILR1819.ReportService.Stateless.Modules;
using ESFA.DC.IO.AzureStorage;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
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
using VersionInfo = ESFA.DC.ILR1819.ReportService.Stateless.Configuration.VersionInfo;

namespace ESFA.DC.ILR1819.ReportService.Stateless
{
    public static class DIComposition
    {
        public static ContainerBuilder BuildContainer(IConfigurationHelper configHelper)
        {
            var containerBuilder = new ContainerBuilder();

            var topicAndTaskOptions = configHelper.GetSectionValues<TopicAndTaskSectionOptions>("TopicAndTaskSection");
            containerBuilder.RegisterInstance(topicAndTaskOptions).As<ITopicAndTaskSectionOptions>().SingleInstance();

            var larsConfiguration = configHelper.GetSectionValues<LarsConfiguration>("LarsSection");
            containerBuilder.RegisterInstance(larsConfiguration).As<LarsConfiguration>().SingleInstance();

            var dasCommitmentsConfiguration = configHelper.GetSectionValues<DasCommitmentsConfiguration>("DasCommitmentsSection");
            containerBuilder.RegisterInstance(dasCommitmentsConfiguration).As<DasCommitmentsConfiguration>().SingleInstance();

            var orgConfiguration = configHelper.GetSectionValues<OrgConfiguration>("OrgSection");
            containerBuilder.RegisterInstance(orgConfiguration).As<OrgConfiguration>().SingleInstance();

            var easConfiguration = configHelper.GetSectionValues<EasConfiguration>("EasSection");
            containerBuilder.RegisterInstance(easConfiguration).As<EasConfiguration>().SingleInstance();

            var ilrValidationErrorsConfiguration = configHelper.GetSectionValues<IlrValidationErrorsConfiguration>("IlrValidationErrorsSection");
            containerBuilder.RegisterInstance(ilrValidationErrorsConfiguration).As<IlrValidationErrorsConfiguration>().SingleInstance();

            var dataStoreConfiguration = configHelper.GetSectionValues<DataStoreConfiguration>("DataStoreSection");
            containerBuilder.RegisterInstance(dataStoreConfiguration).As<DataStoreConfiguration>().SingleInstance();

            var largeEmployeeConfiguration = configHelper.GetSectionValues<LargeEmployerConfiguration>("LargeEmployerSection");
            containerBuilder.RegisterInstance(largeEmployeeConfiguration).As<LargeEmployerConfiguration>().SingleInstance();

            var dasPaymentsConfiguration = configHelper.GetSectionValues<DASPaymentsConfiguration>("DASPaymentsSection");
            containerBuilder.RegisterInstance(dasPaymentsConfiguration).As<DASPaymentsConfiguration>().SingleInstance();

            var postcodeConfiguration = configHelper.GetSectionValues<PostcodeConfiguration>("PostcodeSection");
            containerBuilder.RegisterInstance(postcodeConfiguration).As<PostcodeConfiguration>().SingleInstance();

            var collectionsManagementConfiguration =
                configHelper.GetSectionValues<CollectionsManagementConfiguration>("CollectionsManagementSection");
            containerBuilder.RegisterInstance(collectionsManagementConfiguration)
                .As<CollectionsManagementConfiguration>().SingleInstance();

            // register azure blob storage service
            var azureBlobStorageOptions = configHelper.GetSectionValues<AzureStorageOptions>("AzureStorageSection");
            containerBuilder.RegisterInstance(azureBlobStorageOptions).As<IAzureStorageOptions>();
            containerBuilder.Register(c =>
                    new AzureStorageKeyValuePersistenceConfig(
                        azureBlobStorageOptions.AzureBlobConnectionString,
                        azureBlobStorageOptions.AzureBlobContainerName))
                .As<IAzureStorageKeyValuePersistenceServiceConfig>().SingleInstance();

            containerBuilder.RegisterType<AzureStorageKeyValuePersistenceService>()
                .As<IKeyValuePersistenceService>()
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

            // register Job Context services
            var topicConfig = new ServiceBusTopicConfig(
                serviceBusOptions.ServiceBusConnectionString,
                serviceBusOptions.TopicName,
                serviceBusOptions.ReportingSubscriptionName,
                Environment.ProcessorCount);
            containerBuilder.Register(c =>
            {
                var topicSubscriptionService =
                    new TopicSubscriptionSevice<JobContextDto>(
                        topicConfig,
                        c.Resolve<IJsonSerializationService>(),
                        c.Resolve<ILogger>());
                return topicSubscriptionService;
            }).As<ITopicSubscriptionService<JobContextDto>>();

            containerBuilder.Register(c =>
            {
                var topicPublishService =
                    new TopicPublishService<JobContextDto>(
                        topicConfig,
                        c.Resolve<IJsonSerializationService>());
                return topicPublishService;
            }).As<ITopicPublishService<JobContextDto>>();

            // register message mapper
            containerBuilder.RegisterType<DefaultJobContextMessageMapper<JobContextMessage>>().As<IMapper<JobContextMessage, JobContextMessage>>();

            // register MessageHandler
            containerBuilder.RegisterType<MessageHandler>().As<IMessageHandler<JobContextMessage>>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<EntryPoint>().WithAttributeFiltering().InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextManager<JobContextMessage>>().As<IJobContextManager<JobContextMessage>>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<JobContextMessage>().As<IJobContextMessage>()
                .InstancePerLifetimeScope();

            containerBuilder.Register(context =>
            {
                CollectionsManagementConfiguration settings = context.Resolve<CollectionsManagementConfiguration>();
                DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(
                    settings.CollectionsManagementConnectionString,
                    options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));
                return optionsBuilder.Options;
            })
            .As<DbContextOptions>()
            .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ILR1819_DataStoreEntitiesValid>().As<IIlr1819ValidContext>();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>();
                    optionsBuilder.UseSqlServer(
                        dataStoreConfiguration.ILRDataStoreValidConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<ILR1819_DataStoreEntitiesValid>>()
                .SingleInstance();

            containerBuilder.RegisterType<ILR1819_DataStoreEntities>().As<IIlr1819RulebaseContext>();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ILR1819_DataStoreEntities>();
                    optionsBuilder.UseSqlServer(
                        dataStoreConfiguration.ILRDataStoreConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<ILR1819_DataStoreEntities>>()
                .SingleInstance();

            containerBuilder.RegisterType<DASPaymentsContext>().As<IDASPaymentsContext>();
            containerBuilder.Register(context =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<DASPaymentsContext>();
                    optionsBuilder.UseSqlServer(
                        dasPaymentsConfiguration.DASPaymentsConnectionString,
                        options => options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), new List<int>()));

                    return optionsBuilder.Options;
                })
                .As<DbContextOptions<DASPaymentsContext>>()
                .SingleInstance();

            containerBuilder.RegisterType<DateTimeProvider.DateTimeProvider>().As<IDateTimeProvider>().InstancePerLifetimeScope();

            RegisterReports(containerBuilder);
            RegisterServices(containerBuilder);
            RegisterBuilders(containerBuilder);
            RegisterRules(containerBuilder);
            RegisterCommands(containerBuilder);

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

            containerBuilder.RegisterType<AdultFundingClaimReport>().As<IReport>()
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

            containerBuilder.RegisterType<SummaryOfFm35FundingReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsAdditionalPaymentsReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsIndicativeEarningsReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DataMatchReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<TrailblazerEmployerIncentivesReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FundingClaim1619Report>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsCoInvestmentContributionsReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsMonthlyPaymentReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AppsDataMatchMonthEndReport>().As<IReport>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FundingSummaryPeriodEndReport>().As<IReport>()
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

            containerBuilder.RegisterType<DASPaymentsProviderService>().As<IDASPaymentsProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LarsProviderService>().As<ILarsProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<AllbProviderService>().As<IAllbProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FM25ProviderService>().As<IFM25ProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FM35ProviderService>().As<IFM35ProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FM36ProviderService>().As<IFM36ProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FM81TrailBlazerProviderService>().As<IFM81TrailBlazerProviderService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ValidLearnersService>().As<IValidLearnersService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<InvalidLearnersService>().As<IInvalidLearnersService>()
                .WithAttributeFiltering()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<StringUtilitiesService>().As<IStringUtilitiesService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<IntUtilitiesService>().As<IIntUtilitiesService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<OrgProviderService>().As<IOrgProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PeriodProviderService>().As<IPeriodProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<DasCommitmentsService>().As<IDasCommitmentsService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<EasProviderService>().As<IEasProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<PostcodeProviderService>().As<IPostcodeProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LargeEmployerProviderService>().As<ILargeEmployerProviderService>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ExcelStyleProvider>().As<IExcelStyleProvider>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<CacheProviderService<ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery[]>>().As<ICacheProviderService<ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery[]>>()
                .InstancePerDependency();
            containerBuilder.RegisterType<CacheProviderService<ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery[]>>().As<ICacheProviderService<ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery[]>>()
                .InstancePerDependency();
            containerBuilder.RegisterType<CacheProviderService<ILR.FundingService.FM81.FundingOutput.Model.Output.LearningDelivery[]>>().As<ICacheProviderService<ILR.FundingService.FM81.FundingOutput.Model.Output.LearningDelivery[]>>()
                .InstancePerDependency();

            containerBuilder.RegisterType<ValueProvider>().As<IValueProvider>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ValidationStageOutputCache>().As<IValidationStageOutputCache>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ValidationErrorsService>().As<IValidationErrorsService>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterBuilders(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MathsAndEnglishModelBuilder>().As<IMathsAndEnglishModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<MainOccupancyReportModelBuilder>().As<IMainOccupancyReportModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsAdditionalPaymentsModelBuilder>().As<IAppsAdditionalPaymentsModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsMonthlyPaymentModelBuilder>().As<IAppsMonthlyPaymentModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsDataMatchMonthEndModelBuilder>().As<IAppsDataMatchMonthEndModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<FundingSummaryPeriodEndModelBuilder>().As<IFundingSummaryPeriodEndModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeEarningsModelBuilder>().As<IAppsIndicativeEarningsModelBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<DasCommitmentBuilder>().As<IDasCommitmentBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fm25Builder>().As<IFm25Builder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fm35Builder>().As<IFm35Builder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fm36Builder>().As<IFm36Builder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fm81Builder>().As<IFm81Builder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AllbBuilder>().As<IAllbBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<TotalBuilder>().As<ITotalBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<EasBuilder>().As<IEasBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<DatalockValidationResultBuilder>().As<IDatalockValidationResultBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AdultFundingClaimBuilder>().As<IAdultFundingClaimBuilder>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<TrailblazerEmployerIncentivesModelBuilder>().As<ITrailblazerEmployerIncentivesModelBuilder>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterRules(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MathsAndEnglishFm25Rules>().As<IMathsAndEnglishFm25Rules>()
                .InstancePerLifetimeScope();
        }

        private static void RegisterCommands(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<AppsIndicativeAugustCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeSeptemberCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeOctoberCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeNovemberCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeDecemberCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeJanuaryCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeFebruaryCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeMarchCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeAprilCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeMayCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeJuneCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<AppsIndicativeJulyCommand>().As<IAppsIndicativeCommand>()
                .InstancePerLifetimeScope();

            containerBuilder.Register(c => new List<IAppsIndicativeCommand>(c.Resolve<IEnumerable<IAppsIndicativeCommand>>()))
                .As<IList<IAppsIndicativeCommand>>();
        }
    }
}
