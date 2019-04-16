using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR.ReportService.Interface.DataMatch
{
    public interface IDatalockValidationResultBuilder
    {
        void Add(
            RawEarning earning,
            List<string> errors,
            TransactionTypesFlag paymentType,
            DasCommitment commitment);

        DatalockValidationResult Build();
    }
}
