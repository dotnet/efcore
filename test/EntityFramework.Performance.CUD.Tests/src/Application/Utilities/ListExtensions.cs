#if K10
using System;
using System.Collections.Generic;

namespace Cud.Utilities
{
    internal static class ListExtensions
    {
        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            foreach (var element in list)
            {
                action(element);
            }
        }
    }
}
#endif