// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.ValueComparison.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.ValueComparison.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Extensions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class XGInternalJsonNewtonsoftPropertyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void SetJsonChangeTrackingOptions([NotNull] this IMutableProperty property, XGJsonChangeTrackingOptions? options)
        {
            Check.NotNull(property, nameof(property));

            if (options == null)
            {
                // Use globally configured options for this context.
                // This can always be used to get back to the default implementation and options.
                property.SetValueComparer((ValueComparer)null);
                return;
            }

            var valueComparer = property.GetValueComparer() ?? property.FindTypeMapping()?.Comparer;
            property.SetValueComparer(
                valueComparer is IXGJsonValueComparer xgJsonValueComparer
                    ? xgJsonValueComparer.Clone(options.Value)
                    : XGJsonNewtonsoftValueComparer.Create(property.ClrType, options.Value));
        }
    }
}
