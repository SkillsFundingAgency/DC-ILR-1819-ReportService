﻿using System.Collections.Generic;
using ESFA.DC.ILR1819.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR1819.ReportService.Interface.DataMatch
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
