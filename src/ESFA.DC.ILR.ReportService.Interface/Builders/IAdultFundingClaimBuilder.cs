using System.Collections.Generic;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Output;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Eas;
using ESFA.DC.ILR.ReportService.Model.ILR;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface IAdultFundingClaimBuilder
    {
       AdultFundingClaimModel BuildAdultFundingClaimModel(
            ILogger logger,
            IReportServiceContext reportServiceContext,
            FM35Global fm35Global,
            List<EasSubmissionValues> easSubmissionValues,
            ALBGlobal albGlobal,
            string providerName,
            ILRSourceFileInfo ilrSourceFileInfo,
            IDateTimeProvider dateTimeProvider,
            IIntUtilitiesService intUtilitiesService,
            IMessage message,
            IVersionInfo versionInfo,
            string orgData,
            string largeEmployersData,
            string postcodeData,
            string larsData);
    }
}