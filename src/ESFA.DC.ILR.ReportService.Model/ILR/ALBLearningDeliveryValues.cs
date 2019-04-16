using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.ILR.ReportService.Model.ILR
{
    public class ALBLearningDeliveryValues
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public string AttributeName { get; set; }

        public string FundLine {get; set;}

        public Decimal? Period1 { get; set; }

        public Decimal? Period2 { get; set; }

        public Decimal? Period3 { get; set; }

        public Decimal? Period4 { get; set; }

        public Decimal? Period5 { get; set; }

        public Decimal? Period6 { get; set; }

        public Decimal? Period7 { get; set; }

        public Decimal? Period8 { get; set; }

        public Decimal? Period9 { get; set; }

        public Decimal? Period10 { get; set; }

        public Decimal? Period11 { get; set; }

        public Decimal? Period12 { get; set; }
    }
}
