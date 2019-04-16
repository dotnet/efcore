// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DbContextOptionsExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string BuildOptionsFragment([NotNull] this IDbContextOptions contextOptions)
        {
            var builder = new StringBuilder();
            foreach (var extension in contextOptions.Extensions)
            {
                builder.Append(extension.LogFragment);
            }

            var fragment = builder.ToString();

            return string.IsNullOrWhiteSpace(fragment) ? "None" : fragment;
        }
    }
}
