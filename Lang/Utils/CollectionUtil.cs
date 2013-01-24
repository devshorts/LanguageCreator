using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang
{
    public static class CollectionUtil
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var i in list)
            {
                action(i);
            }
        }

        public static Boolean IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
