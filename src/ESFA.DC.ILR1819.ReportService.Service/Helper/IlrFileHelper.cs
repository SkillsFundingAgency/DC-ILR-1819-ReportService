using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Helper
{
    public class IlrFileHelper : IIlrFileHelper
    {
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly ILogger _logger;

        private string _ilrFileName;
        private string _ilrFileDateTime;

        public IlrFileHelper(IStringUtilitiesService stringUtilitiesService, ILogger logger)
        {
            _stringUtilitiesService = stringUtilitiesService;
            _logger = logger;
        }

        public bool CheckIlrFileNameIsValid(IJobContextMessage jobContextMessage)
        {
            _ilrFileName = jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename].ToString();
            _ilrFileDateTime = _stringUtilitiesService.GetIlrFileDate(_ilrFileName)?.ToString("yyyy-MM-dd HH:mm:ssy");

            if (_ilrFileDateTime != null)
            {
                return true;
            }

            _logger.LogError("ILR file name contains invalid date time format");
            return false;
        }
    }
}
