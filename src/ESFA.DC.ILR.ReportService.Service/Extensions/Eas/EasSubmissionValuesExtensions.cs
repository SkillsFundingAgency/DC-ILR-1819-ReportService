using ESFA.DC.ILR.ReportService.Model.Eas;

namespace ESFA.DC.ILR.ReportService.Service.Extensions.Eas
{
    public static class EasSubmissionValuesExtensions
    {
        public static EasSubmissionValues ToEasSubmissionValues(this EAS1819.EF.EasSubmissionValues easSubmissionValues)
        {
            return new EasSubmissionValues()
            {
                CollectionPeriod = easSubmissionValues.CollectionPeriod,
                PaymentId = easSubmissionValues.PaymentId,
                PaymentValue = easSubmissionValues.PaymentValue
            };
        }
    }
}
