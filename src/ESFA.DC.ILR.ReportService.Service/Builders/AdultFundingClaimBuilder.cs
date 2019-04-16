using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Output;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Eas;
using ESFA.DC.ILR.ReportService.Model.ILR;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public class AdultFundingClaimBuilder : IAdultFundingClaimBuilder
    {
        private int[] First6MonthsArray => new[] { 1, 2, 3, 4, 5, 6 };

        private int[] First12MonthsArray => new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

      public AdultFundingClaimModel BuildAdultFundingClaimModel(
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
            string larsData)
        {
            var adultFundingClaimModel = new AdultFundingClaimModel();

            try
            {
                var ilrfileName = reportServiceContext.OriginalFilename ?? reportServiceContext.Filename;

                // FM35
                decimal otherLearningProgramFunding6MonthsFm35 = 0;
                decimal otherLearningProgramFunding12MonthsFm35 = 0;
                decimal otherLearningSupport6MonthsFm35 = 0;
                decimal otherLearningSupport12MonthsFm35 = 0;
                decimal traineeShips1924ProgrammeFunding6MonthsFm35 = 0;
                decimal traineeShips1924ProgrammeFunding12MonthsFm35 = 0;
                decimal traineeShips1924LearningSupport6MonthsFm35 = 0;
                decimal traineeShips1924LearningSupport12MonthsFm35 = 0;

                // EAS
                decimal otherLearningProgramFunding6MonthsEas = 0;
                decimal otherLearningProgramFunding12MonthsEas = 0;
                decimal otherLearningSupport6MonthsEas = 0;
                decimal otherLearningSupport12MonthsEas = 0;
                decimal traineeShips1924ProgrammeFunding6MonthsEas = 0;
                decimal traineeShips1924ProgrammeFunding12MonthsEas = 0;
                decimal traineeShips1924LearningSupport6MonthsEas = 0;
                decimal traineeShips1924LearningSupport12MonthsEas = 0;
                decimal traineeShips1924LearnerSupport6MonthsEas = 0;
                decimal traineeShips1924LearnerSupport12MonthsEas = 0;
                decimal loansAreasCosts6MonthsEas = 0;
                decimal loansAreasCosts12MonthsEas = 0;
                decimal loansExcessSupport6MonthsEas = 0;
                decimal loansExcessSupport12MonthsEas = 0;

                // ALB
                decimal loansBursaryFunding6Months = 0;
                decimal loansBursaryFunding12Months = 0;
                decimal loansAreaCosts6Months = 0;
                decimal loansAreaCosts12Months = 0;

                var ukPrn = reportServiceContext.Ukprn;

                // FM35
                var fm35LearningDeliveryPeriodisedValues = GetFM35LearningDeliveryPeriodisedValues(fm35Global, ukPrn);
                otherLearningProgramFunding6MonthsFm35 = Fm35DeliveryValues6Months(fm35LearningDeliveryPeriodisedValues, new[] { "OnProgPayment", "BalancePayment", "AchievePayment", "EmpOutcomePay" }, new[] { "AEB - Other Learning", "AEB - Other Learning (non-procured)" });
                otherLearningProgramFunding12MonthsFm35 = Fm35DeliveryValues12Months(fm35LearningDeliveryPeriodisedValues, new[] { "OnProgPayment", "BalancePayment", "AchievePayment", "EmpOutcomePay" }, new[] { "AEB - Other Learning", "AEB - Other Learning (non-procured)" });
                otherLearningSupport6MonthsFm35 = Fm35DeliveryValues6Months(fm35LearningDeliveryPeriodisedValues, new[] { "LearnSuppFundCash" }, new[] { "AEB - Other Learning", "AEB - Other Learning (non-procured)" });
                otherLearningSupport12MonthsFm35 = Fm35DeliveryValues12Months(fm35LearningDeliveryPeriodisedValues, new[] { "LearnSuppFundCash" }, new[] { "AEB - Other Learning", "AEB - Other Learning (non-procured)" });
                traineeShips1924ProgrammeFunding6MonthsFm35 = Fm35DeliveryValues6Months(fm35LearningDeliveryPeriodisedValues, new[] { "OnProgPayment", "BalancePayment", "AchievePayment", "EmpOutcomePay" }, new[] { "19-24 Traineeship’", "19-24 Traineeship (non-procured)" });
                traineeShips1924ProgrammeFunding12MonthsFm35 = Fm35DeliveryValues12Months(fm35LearningDeliveryPeriodisedValues, new[] { "OnProgPayment", "BalancePayment", "AchievePayment", "EmpOutcomePay" }, new[] { "19-24 Traineeship’", "19-24 Traineeship (non-procured)" });
                traineeShips1924LearningSupport6MonthsFm35 = Fm35DeliveryValues6Months(fm35LearningDeliveryPeriodisedValues, new[] { "LearnSuppFundCash" }, new[] { "19-24 Traineeship’", "19-24 Traineeship (non-procured)" });
                traineeShips1924LearningSupport12MonthsFm35 = Fm35DeliveryValues12Months(fm35LearningDeliveryPeriodisedValues, new[] { "LearnSuppFundCash" }, new[] { "19-24 Traineeship’", "19-24 Traineeship (non-procured)" });

                // ALB
                var albLearningDeliveryPeriodisedValues = GetAlbLearningDeliveryPeriodisedValues(albGlobal, ukPrn);
                loansBursaryFunding6Months = AlbDeliveryValues6Months(albLearningDeliveryPeriodisedValues, new[] { "ALBSupportPayment" }, new[] { "Advanced Learner Loans Bursary" });
                loansBursaryFunding12Months = AlbDeliveryValues12Months(albLearningDeliveryPeriodisedValues, new[] { "ALBSupportPayment" }, new[] { "Advanced Learner Loans Bursary" });
                loansAreaCosts6Months = AlbDeliveryValues6Months(albLearningDeliveryPeriodisedValues, new[] { "AreaUpliftBalPayment", "AreaUpliftOnProgPayment" }, new[] { "Advanced Learner Loans Bursary" });
                loansAreaCosts12Months = AlbDeliveryValues12Months(albLearningDeliveryPeriodisedValues, new[] { "AreaUpliftBalPayment", "AreaUpliftOnProgPayment" }, new[] { "Advanced Learner Loans Bursary" });

                // EAS

                otherLearningProgramFunding6MonthsEas = easSubmissionValues.Where(x => new[] { "Authorised Claims: AEB-Other Learning", "Princes Trust: AEB-Other Learning" }.Contains(x.PaymentTypeName) &&
                                                          First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                otherLearningProgramFunding12MonthsEas = easSubmissionValues.Where(x => new[] { "Authorised Claims: AEB-Other Learning", "Princes Trust: AEB-Other Learning" }.Contains(x.PaymentTypeName) &&
                                                                                       First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);

                otherLearningSupport6MonthsEas = easSubmissionValues.Where(x => new[] { "Excess Learning Support: AEB-Other Learning" }.Contains(x.PaymentTypeName)
                                                                                && First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                otherLearningSupport12MonthsEas = easSubmissionValues.Where(x => new[] { "Excess Learning Support: AEB-Other Learning" }.Contains(x.PaymentTypeName)
                                                                                 && First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);

                traineeShips1924ProgrammeFunding6MonthsEas = easSubmissionValues.Where(x => new[] { "Authorised Claims: 19-24 Traineeships" }.Contains(x.PaymentTypeName)
                                                                                && First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                traineeShips1924ProgrammeFunding12MonthsEas = easSubmissionValues.Where(x => new[] { "Authorised Claims: 19-24 Traineeships" }.Contains(x.PaymentTypeName)
                                                                                            && First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);

                traineeShips1924LearningSupport6MonthsEas = easSubmissionValues.Where(x => new[] { "Excess Learning Support: 19-24 Traineeships" }.Contains(x.PaymentTypeName)
                                                                                            && First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                traineeShips1924LearningSupport12MonthsEas = easSubmissionValues.Where(x => new[] { "Excess Learning Support: 19-24 Traineeships" }.Contains(x.PaymentTypeName)
                                                                                           && First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);

                traineeShips1924LearnerSupport6MonthsEas = easSubmissionValues.Where(x => new[] { "Learner Support: 19-24 Traineeships" }.Contains(x.PaymentTypeName)
                                                                                           && First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                traineeShips1924LearnerSupport12MonthsEas = easSubmissionValues.Where(x => new[] { "Learner Support: 19-24 Traineeships" }.Contains(x.PaymentTypeName)
                                                                                           && First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);

                loansAreasCosts6MonthsEas = easSubmissionValues.Where(x => new[] { "Authorised Claims: Advanced Learner Loans Bursary" }.Contains(x.PaymentTypeName)
                                                                           && First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                loansAreasCosts12MonthsEas = easSubmissionValues.Where(x => new[] { "Authorised Claims: Advanced Learner Loans Bursary" }.Contains(x.PaymentTypeName)
                                                                           && First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                loansExcessSupport6MonthsEas = easSubmissionValues.Where(x => new[] { "Excess Support: Advanced Learner Loans Bursary" }.Contains(x.PaymentTypeName)
                                                                           && First6MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                loansExcessSupport12MonthsEas = easSubmissionValues.Where(x => new[] { "Excess Support: Advanced Learner Loans Bursary" }.Contains(x.PaymentTypeName)
                                                                            && First12MonthsArray.Contains(x.CollectionPeriod)).Sum(y => y.PaymentValue);
                adultFundingClaimModel.OtherLearningProgrammeFunding6Months = otherLearningProgramFunding6MonthsFm35 + otherLearningProgramFunding6MonthsEas;
                adultFundingClaimModel.OtherLearningProgrammeFunding12Months = otherLearningProgramFunding12MonthsFm35 + otherLearningProgramFunding12MonthsEas;
                adultFundingClaimModel.OtherLearningLearningSupport6Months = otherLearningSupport6MonthsFm35 + otherLearningSupport6MonthsEas;
                adultFundingClaimModel.OtherLearningLearningSupport12Months = otherLearningSupport12MonthsFm35 + otherLearningSupport12MonthsEas;
                adultFundingClaimModel.Traineeships1924ProgrammeFunding6Months = traineeShips1924ProgrammeFunding6MonthsFm35 + traineeShips1924ProgrammeFunding6MonthsEas;
                adultFundingClaimModel.Traineeships1924ProgrammeFunding12Months = traineeShips1924ProgrammeFunding12MonthsFm35 + traineeShips1924ProgrammeFunding12MonthsEas;
                adultFundingClaimModel.Traineeships1924LearningSupport6Months = traineeShips1924LearningSupport6MonthsFm35 + traineeShips1924LearningSupport6MonthsEas;
                adultFundingClaimModel.Traineeships1924LearningSupport12Months = traineeShips1924LearningSupport12MonthsFm35 + traineeShips1924LearningSupport12MonthsEas;
                adultFundingClaimModel.Traineeships1924LearnerSupport6Months = traineeShips1924LearnerSupport6MonthsEas;
                adultFundingClaimModel.Traineeships1924LearnerSupport12Months = traineeShips1924LearnerSupport12MonthsEas;

                adultFundingClaimModel.LoansBursaryFunding6Months = loansBursaryFunding6Months;
                adultFundingClaimModel.LoansBursaryFunding12Months = loansBursaryFunding12Months;
                adultFundingClaimModel.LoansAreaCosts6Months = loansAreaCosts6Months + loansAreasCosts6MonthsEas;
                adultFundingClaimModel.LoansAreaCosts12Months = loansAreaCosts12Months + loansAreasCosts12MonthsEas;
                adultFundingClaimModel.LoansExcessSupport6Months = loansExcessSupport6MonthsEas;
                adultFundingClaimModel.LoansExcessSupport12Months = loansExcessSupport12MonthsEas;

                adultFundingClaimModel.ProviderName = providerName ?? "Unknown";
                adultFundingClaimModel.Ukprn = ukPrn;
                adultFundingClaimModel.ReportGeneratedAt = "Report generated at: " + dateTimeProvider.GetNowUtc().ToString("hh:mm:ss tt") + " on " +
                                                      dateTimeProvider.GetNowUtc().ToString("dd/MM/yyyy");

                if (message == null) // NON - ILR Submission.
                {
                    if (ilrSourceFileInfo.Filename == null)
                    {
                        adultFundingClaimModel.IlrFile = "NA";
                        adultFundingClaimModel.FilePreparationDate = "NA";
                    }
                    else
                    {
                        adultFundingClaimModel.IlrFile = ilrSourceFileInfo.Filename;
                        adultFundingClaimModel.FilePreparationDate = ilrSourceFileInfo.FilePreparationDate.GetValueOrDefault().ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    adultFundingClaimModel.IlrFile = ilrfileName;
                    adultFundingClaimModel.FilePreparationDate =
                        message.HeaderEntity.CollectionDetailsEntity.FilePreparationDate.ToString("dd/MM/yyyy");
                }

                adultFundingClaimModel.ApplicationVersion = versionInfo.ServiceReleaseVersion;
                adultFundingClaimModel.ComponentSetVersion = "NA";
                adultFundingClaimModel.LargeEmployerData = largeEmployersData;
                adultFundingClaimModel.OrganisationData = orgData;
                adultFundingClaimModel.PostcodeData = postcodeData;
                adultFundingClaimModel.LarsData = larsData;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed building Adult funding claim report, ex: {ex}");
                throw;
            }

            return adultFundingClaimModel;
        }

        private static List<FM35LearningDeliveryValues> GetFM35LearningDeliveryPeriodisedValues(FM35Global fm35Global, int ukPrn)
        {
            var result = new List<FM35LearningDeliveryValues>();
            if (fm35Global?.Learners == null)
            {
                return result;
            }

            foreach (var learner in fm35Global.Learners)
            {
                if (learner.LearningDeliveries == null)
                {
                    continue;
                }

                foreach (var ld in learner.LearningDeliveries)
                {
                    if (ld.LearningDeliveryPeriodisedValues == null)
                    {
                        continue;
                    }

                    foreach (var ldpv in ld.LearningDeliveryPeriodisedValues)
                    {
                        result.Add(new FM35LearningDeliveryValues
                        {
                            AimSeqNumber = ld.AimSeqNumber ?? 0,
                            LearnRefNumber = learner.LearnRefNumber,
                            AttributeName = ldpv.AttributeName,
                            Period1 = ldpv.Period1,
                            Period2 = ldpv.Period2,
                            Period3 = ldpv.Period3,
                            Period4 = ldpv.Period4,
                            Period5 = ldpv.Period5,
                            Period6 = ldpv.Period6,
                            Period7 = ldpv.Period7,
                            Period8 = ldpv.Period8,
                            Period9 = ldpv.Period9,
                            Period10 = ldpv.Period10,
                            Period11 = ldpv.Period11,
                            Period12 = ldpv.Period12,
                            UKPRN = ukPrn,
                            FundLine = ld.LearningDeliveryValue?.FundLine
                        });
                    }
                }
            }

            return result;
        }

        private static List<ALBLearningDeliveryValues> GetAlbLearningDeliveryPeriodisedValues(ALBGlobal albGlobal, int ukPrn)
        {
            var result = new List<ALBLearningDeliveryValues>();
            if (albGlobal?.Learners == null)
            {
                return result;
            }

            foreach (var learner in albGlobal.Learners)
            {
                if (learner.LearningDeliveries == null)
                {
                    continue;
                }

                foreach (var ld in learner.LearningDeliveries)
                {
                    if (ld.LearningDeliveryPeriodisedValues == null)
                    {
                        continue;
                    }

                    foreach (var ldpv in ld.LearningDeliveryPeriodisedValues)
                    {
                        result.Add(new ALBLearningDeliveryValues
                        {
                            AimSeqNumber = ld.AimSeqNumber,
                            LearnRefNumber = learner.LearnRefNumber,
                            AttributeName = ldpv.AttributeName,
                            Period1 = ldpv.Period1,
                            Period2 = ldpv.Period2,
                            Period3 = ldpv.Period3,
                            Period4 = ldpv.Period4,
                            Period5 = ldpv.Period5,
                            Period6 = ldpv.Period6,
                            Period7 = ldpv.Period7,
                            Period8 = ldpv.Period8,
                            Period9 = ldpv.Period9,
                            Period10 = ldpv.Period10,
                            Period11 = ldpv.Period11,
                            Period12 = ldpv.Period12,
                            UKPRN = ukPrn,
                            FundLine = ld.LearningDeliveryValue?.FundLine
                        });
                    }
                }
            }

            return result;
        }

        private static decimal AlbDeliveryValues6Months(
            List<ALBLearningDeliveryValues> albLearningDeliveryValues,
            string[] attributes,
            string[] fundLines)
        {
            decimal value = 0;
            var otherLearningProgramFunding = albLearningDeliveryValues.Where(x =>
                attributes.Contains(x.AttributeName) && fundLines.Contains(x.FundLine)).ToList();

            foreach (var deliveryValues in otherLearningProgramFunding)
            {
                value = value +
                    deliveryValues.Period1.GetValueOrDefault() + deliveryValues.Period2.GetValueOrDefault() + deliveryValues.Period3.GetValueOrDefault() +
                    deliveryValues.Period4.GetValueOrDefault() + deliveryValues.Period5.GetValueOrDefault() + deliveryValues.Period6.GetValueOrDefault();
            }

            return value;
        }

        private static decimal AlbDeliveryValues12Months(
            List<ALBLearningDeliveryValues> albLearningDeliveryValues,
            string[] attributes,
            string[] fundLines)
        {
            decimal value = 0;
            var otherLearningProgramFunding = albLearningDeliveryValues.Where(x =>
                attributes.Contains(x.AttributeName) && fundLines.Contains(x.FundLine)).ToList();

            foreach (var deliveryValues in otherLearningProgramFunding)
            {
                value = value +
                     deliveryValues.Period1.GetValueOrDefault() + deliveryValues.Period2.GetValueOrDefault() + deliveryValues.Period3.GetValueOrDefault() +
                    deliveryValues.Period4.GetValueOrDefault() + deliveryValues.Period5.GetValueOrDefault() + deliveryValues.Period6.GetValueOrDefault() +
                    deliveryValues.Period7.GetValueOrDefault() + deliveryValues.Period8.GetValueOrDefault() + deliveryValues.Period9.GetValueOrDefault() +
                    deliveryValues.Period10.GetValueOrDefault() + deliveryValues.Period11.GetValueOrDefault() + deliveryValues.Period12.GetValueOrDefault();
            }

            return value;
        }

        private static decimal Fm35DeliveryValues6Months(
            List<FM35LearningDeliveryValues> fm35LearningDeliveryPeriodisedValues,
            string[] attributes,
            string[] fundLines)
        {
            decimal value = 0;
            var otherLearningProgramFunding = fm35LearningDeliveryPeriodisedValues.Where(x =>
                attributes.Contains(x.AttributeName) && fundLines.Contains(x.FundLine)).ToList();

            foreach (var deliveryValues in otherLearningProgramFunding)
            {
                value = value +
                     deliveryValues.Period1.GetValueOrDefault() + deliveryValues.Period2.GetValueOrDefault() + deliveryValues.Period3.GetValueOrDefault() +
                    deliveryValues.Period4.GetValueOrDefault() + deliveryValues.Period5.GetValueOrDefault() + deliveryValues.Period6.GetValueOrDefault();
            }

            return value;
        }

        private static decimal Fm35DeliveryValues12Months(
            List<FM35LearningDeliveryValues> fm35LearningDeliveryPeriodisedValues,
            string[] attributes,
            string[] fundLines)
        {
            decimal value = 0;
            var otherLearningProgramFunding = fm35LearningDeliveryPeriodisedValues.Where(x =>
                attributes.Contains(x.AttributeName) && fundLines.Contains(x.FundLine)).ToList();

            foreach (var deliveryValues in otherLearningProgramFunding)
            {
                value = value +
                      deliveryValues.Period1.GetValueOrDefault() + deliveryValues.Period2.GetValueOrDefault() + deliveryValues.Period3.GetValueOrDefault() +
                    deliveryValues.Period4.GetValueOrDefault() + deliveryValues.Period5.GetValueOrDefault() + deliveryValues.Period6.GetValueOrDefault() +
                    deliveryValues.Period7.GetValueOrDefault() + deliveryValues.Period8.GetValueOrDefault() + deliveryValues.Period9.GetValueOrDefault() +
                    deliveryValues.Period10.GetValueOrDefault() + deliveryValues.Period11.GetValueOrDefault() + deliveryValues.Period12.GetValueOrDefault();
            }

            return value;
        }
    }
}