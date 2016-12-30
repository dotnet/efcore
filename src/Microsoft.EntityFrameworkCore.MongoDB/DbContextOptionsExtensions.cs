using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextOptionsExtensions
    {
        public static TExtension Extract<TExtension>([NotNull] this IDbContextOptions dbContextOptions)
            where TExtension : IDbContextOptionsExtension
        {
            Check.NotNull(dbContextOptions, nameof(dbContextOptions));

            IList<TExtension> extensions = dbContextOptions.Extensions
                .OfType<TExtension>()
                .ToList();

            if (extensions.Count == 0)
            {
                throw new InvalidOperationException($"No provider has been configured with a {nameof(TExtension)} extension.");
            }
            if (extensions.Count > 1)
            {
                throw new InvalidOperationException($"Multiple providers have been configured with a {nameof(TExtension)} extension.");
            }
            return extensions[index: 0];
        }
    }
}