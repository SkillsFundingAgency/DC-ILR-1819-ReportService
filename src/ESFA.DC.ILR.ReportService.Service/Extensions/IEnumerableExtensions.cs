using System;
using System.Collections.Generic;

namespace ESFA.DC.ILR.ReportService.Service.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static int DistinctByCount<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            HashSet<string> seenKeys = new HashSet<string>();
            foreach (TSource element in source)
            {
                string val = keySelector(element);
                if (!string.IsNullOrEmpty(val))
                {
                    seenKeys.Add(val);
                }
            }

            return seenKeys.Count;
        }
    }
}
