using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Data.DAS.Model;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;
using ESFA.DC.ILR.ReportService.Service.Extensions;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public sealed class DasCommitmentBuilder : IDasCommitmentBuilder
    {
        // Single instance
        public DasCommitmentBuilder()
        {
        }

        public List<DasCommitment> Build(IEnumerable<DasCommitments> commitments)
        {
            List<DasCommitment> dasCommitments = new List<DasCommitment>(commitments.Select(x => new DasCommitment(x)));
            ILookup<long, DasCommitment> commitmentsById = dasCommitments.ToLookup(x => x.CommitmentId);

            foreach (var commitmentsForId in commitmentsById)
            {
                var commitmentList = commitmentsForId.ToList();

                if (commitmentList.Count == 1)
                {
                    // No 'versions'
                    commitmentList[0].EffectiveStartDate = commitmentList[0].StartDate.LastDayOfMonth();
                    commitmentList[0].EffectiveEndDate = commitmentList[0].EndDate.LastDayOfMonth().AddDays(-1);
                }
                else
                {
                    foreach (var commitment in commitmentList)
                    {
                        commitment.EffectiveStartDate = commitment.EffectiveFromDate;
                        commitment.EffectiveEndDate = commitment.EffectiveToDate;
                        commitment.IsVersioned = true;
                    }
                }

                foreach (var commitment in commitmentList)
                {
                    if (commitment.WithdrawnOnDate.HasValue)
                    {
                        commitment.EffectiveEndDate = commitment.WithdrawnOnDate;
                    }
                }
            }

            DasCommitment lastCommitment = dasCommitments
                .OrderByDescending(x => x.EffectiveStartDate)
                .ThenByDescending(x => x.CommitmentId)
                .FirstOrDefault();
            if (lastCommitment != null)
            {
                lastCommitment.EffectiveEndDate = null;
            }

            return dasCommitments;
        }

        public IReadOnlyList<DasCommitment> ActiveCommitmentsForDate(IEnumerable<DasCommitment> commitments, DateTime date)
        {
            return commitments.Where(x => x.EffectiveStartDate <= date &&

                                          (x.EffectiveEndDate == null ||
                                          x.EffectiveEndDate >= date) &&

                                          (x.PaymentStatus == (int)PaymentStatus.Active ||
                                              (x.PaymentStatus == (int)PaymentStatus.Withdrawn &&
                                               x.WithdrawnOnDate >= date)))
                .ToList();
        }

        public IReadOnlyList<DasCommitment> NonActiveCommitmentsForDate(IEnumerable<DasCommitment> commitments, DateTime date)
        {
            return commitments.Where(x => x.EffectiveStartDate < date &&
                                          (x.WithdrawnOnDate.HasValue ||
                                          x.PausedOnDate.HasValue))
                .ToList();
        }
    }
}
