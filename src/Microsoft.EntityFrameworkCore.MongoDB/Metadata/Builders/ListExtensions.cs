using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public static class ListExtensions
    {
        public static IList<TBase> Replace<TBase, TReplacement>([NotNull] this IList<TBase> list,
            [NotNull] TReplacement replacement)
            where TReplacement : TBase
        {
            Check.NotNull(list, nameof(list));
            Check.NotNull(replacement, nameof(replacement));
            list
                .OfType<TReplacement>()
                .Select(item => list.IndexOf(item))
                .ToList()
                .ForEach(index => list[index] = replacement);
            return list;
        }

        public static IList<TBase> With<TBase, TItem>([NotNull] this IList<TBase> list,
            [NotNull] TItem item)
            where TItem : TBase
        {
            Check.NotNull(list, nameof(list));
            Check.NotNull(item, nameof(item));
            list.Add(item);
            return list;
        }
    }
}