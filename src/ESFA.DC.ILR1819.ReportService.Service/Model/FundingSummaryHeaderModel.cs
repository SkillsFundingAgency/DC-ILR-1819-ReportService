﻿namespace ESFA.DC.ILR1819.ReportService.Service.Model
{
    public sealed class FundingSummaryHeaderModel
    {
        public string ProviderName { get; set; }

        public int Ukprn { get; set; }

        public string IlrFile { get; set; }

        public string LastIlrFileUpdate { get; set; }

        public string LastEasUpdate { get; set; }

        public string SecurityClassification { get; set; }
    }
}
