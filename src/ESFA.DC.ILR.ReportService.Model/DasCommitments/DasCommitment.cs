using System;

namespace ESFA.DC.ILR.ReportService.Model.DasCommitments
{
    public sealed class DasCommitment : Data.DAS.Model.DasCommitments
    {
        public DasCommitment(Data.DAS.Model.DasCommitments entity)
        {
            CommitmentId = entity.CommitmentId;
            VersionId = entity.VersionId;
            Uln = entity.Uln;
            Ukprn = entity.Ukprn;
            AccountId = entity.AccountId;
            StartDate = entity.StartDate;
            EndDate = entity.EndDate;
            AgreedCost = entity.AgreedCost;
            StandardCode = entity.StandardCode;
            ProgrammeType = entity.ProgrammeType;
            FrameworkCode = entity.FrameworkCode;
            PathwayCode = entity.PathwayCode;
            PaymentStatus = entity.PaymentStatus;
            PaymentStatusDescription = entity.PaymentStatusDescription;
            Priority = entity.Priority;
            EffectiveFromDate = entity.EffectiveFromDate;
            EffectiveToDate = entity.EffectiveToDate;
            TransferSendingEmployerAccountId = entity.TransferSendingEmployerAccountId;
            TransferApprovalDate = entity.TransferApprovalDate;
            WithdrawnOnDate = entity.WithdrawnOnDate;
            PausedOnDate = entity.PausedOnDate;
            Ukprn = entity.Ukprn;
        }

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsVersioned { get; set; }
    }
}
