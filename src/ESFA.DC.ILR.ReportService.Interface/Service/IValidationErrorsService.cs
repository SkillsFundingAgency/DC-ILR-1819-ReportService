using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Model.Poco;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IValidationErrorsService
    {
        Task PopulateValidationErrors(string[] ruleNames, List<ValidationErrorDetails> validationErrors, CancellationToken cancellationToken);
    }
}
