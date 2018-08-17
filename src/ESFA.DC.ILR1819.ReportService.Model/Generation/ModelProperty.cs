using System.Reflection;

namespace ESFA.DC.ILR1819.ReportService.Model.Generation
{
    public sealed class ModelProperty
    {
        public object Name { get; }

        public PropertyInfo MethodInfo { get; }

        public ModelProperty(object name, PropertyInfo methodInfo)
        {
            Name = name;
            MethodInfo = methodInfo;
        }
    }
}
