using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.ReportService.Interface.DataMatch;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;
using ESFA.DC.ILR.ReportService.Service.Extensions.DataMatch;
using ESFA.DC.ILR.ReportService.Service.ReferenceData;

namespace ESFA.DC.ILR.ReportService.Service.Service.DataMatch
{
    public sealed class DatalockValidationResultBuilder : IDatalockValidationResultBuilder
    {
        public List<DatalockValidationError> ValidationErrors { get; } = new List<DatalockValidationError>();

        public List<DatalockValidationErrorByPeriod> ValidationErrorsByPeriod { get; } = new List<DatalockValidationErrorByPeriod>();

        public List<PriceEpisodePeriodMatchEntity> PriceEpisodePeriodMatches { get; } = new List<PriceEpisodePeriodMatchEntity>();

        public List<PriceEpisodeMatchEntity> PriceEpisodeMatches { get; } = new List<PriceEpisodeMatchEntity>();

        public List<DatalockOutputEntity> DatalockOutputEntities { get; } = new List<DatalockOutputEntity>();

        public void Add(RawEarning earning, List<string> errors, TransactionTypesFlag paymentType, DasCommitment commitment)
        {
            var payable = false;
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    if (ValidationErrors.DoesNotAlreadyContainEarningForThisError(earning, error) &&
                        PriceEpisodePeriodMatches.DoesNotContainEarningForCommitmentAndPaymentType(earning, commitment, paymentType))
                    {
                        ValidationErrors.Add(new DatalockValidationError
                        {
                            LearnRefNumber = earning.LearnRefNumber,
                            AimSeqNumber = earning.AimSeqNumber,
                            PriceEpisodeIdentifier = earning.PriceEpisodeIdentifier,
                            RuleId = error,
                            Ukprn = earning.Ukprn,
                        });
                    }

                    if (ValidationErrorsByPeriod.DoesNotAlreadyContainEarningForThisError(earning, error))
                    {
                        ValidationErrorsByPeriod.Add(new DatalockValidationErrorByPeriod()
                        {
                            LearnRefNumber = earning.LearnRefNumber,
                            AimSeqNumber = earning.AimSeqNumber,
                            PriceEpisodeIdentifier = earning.PriceEpisodeIdentifier,
                            RuleId = error,
                            Ukprn = earning.Ukprn,
                            Period = earning.Period,
                        });
                    }
                }
            }
            else
            {
                payable = true;
            }

            if (commitment == null)
            {
                return;
            }

            PriceEpisodePeriodMatches.Add(new PriceEpisodePeriodMatchEntity
            {
                AimSeqNumber = earning.AimSeqNumber,
                CommitmentId = commitment.CommitmentId,
                LearnRefNumber = earning.LearnRefNumber,
                PriceEpisodeIdentifier = earning.PriceEpisodeIdentifier,
                Period = earning.Period,
                TransactionTypesFlag = paymentType,
                Payable = payable,
                Ukprn = earning.Ukprn,
                VersionId = commitment.VersionId,
            });

            if (payable)
            {
                var validationErrorsToRemove = ValidationErrors.Where(x =>
                        x.Ukprn == earning.Ukprn &&
                        x.LearnRefNumber == earning.LearnRefNumber &&
                        x.PriceEpisodeIdentifier == earning.PriceEpisodeIdentifier)
                    .ToList();

                foreach (var validationError in validationErrorsToRemove)
                {
                    ValidationErrors.Remove(validationError);
                }
            }

            if (PriceEpisodeMatches.DoesNotAlreadyContainEarningForCommitment(earning, commitment, payable))
            {
                PriceEpisodeMatches.Add(new PriceEpisodeMatchEntity
                {
                    CommitmentId = commitment.CommitmentId,
                    IsSuccess = payable,
                    LearnRefNumber = earning.LearnRefNumber,
                    PriceEpisodeIdentifier = earning.PriceEpisodeIdentifier,
                    Ukprn = earning.Ukprn,
                    AimSeqNumber = earning.AimSeqNumber,
                });
            }
        }

        public DatalockValidationResult Build()
        {
            RemoveErrorsInAdditionToDLOCK_09();

            RemoveDuplicateMatches();

            return new DatalockValidationResult(
                ValidationErrors,
                ValidationErrorsByPeriod,
                PriceEpisodePeriodMatches,
                PriceEpisodeMatches,
                DatalockOutputEntities);
        }

        private void RemoveDuplicateMatches()
        {
            var groupedMatches = PriceEpisodeMatches.GroupBy(x => x.PriceEpisodeIdentifier);
            foreach (var groupedMatch in groupedMatches)
            {
                if (groupedMatch.Any(x => x.IsSuccess))
                {
                    PriceEpisodeMatches.RemoveAll(x => x.PriceEpisodeIdentifier == groupedMatch.Key &&
                                                       !x.IsSuccess);
                }
            }
        }

        private void RemoveErrorsInAdditionToDLOCK_09()
        {
            var groupedErrors = ValidationErrors.GroupBy(x => x.PriceEpisodeIdentifier);
            foreach (var groupedError in groupedErrors)
            {
                if (groupedError.Any(x => x.RuleId == DataLockValidationMessages.DLOCK_09))
                {
                    ValidationErrors.RemoveAll(x => x.PriceEpisodeIdentifier == groupedError.Key &&
                                                    x.RuleId != DataLockValidationMessages.DLOCK_09);
                }
            }
        }
    }
}
