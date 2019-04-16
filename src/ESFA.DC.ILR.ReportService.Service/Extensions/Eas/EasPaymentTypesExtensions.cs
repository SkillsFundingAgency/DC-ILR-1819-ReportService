using ESFA.DC.ILR.ReportService.Model.Eas;

namespace ESFA.DC.ILR.ReportService.Service.Extensions.Eas
{
    public static class EasPaymentTypesExtensions
    {
        public static EasPaymentType ToEasPaymentTypes(this EAS1819.EF.PaymentTypes paymentTypes)
        {
            return new EasPaymentType()
            {
                PaymentId = paymentTypes.PaymentId,
                PaymentName = paymentTypes.PaymentName
            };
        }
    }
}
