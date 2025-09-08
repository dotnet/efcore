// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     XuGu specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class XGJsonNewtonsoftPropertyBuilderExtensions
    {
        /// <summary>
        /// Uses specific change tracking options for this JSON propery, that specify how inner properties or array
        /// elements will be tracked. Applies to simple strings, POCOs and DOM objects. Using `null` restores all
        /// defaults.
        /// </summary>
        /// <param name="propertyBuilder">The builder for the property being configured.</param>
        /// <param name="options">The change tracking options to configure for the property.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseJsonChangeTrackingOptions(
            [NotNull] this PropertyBuilder propertyBuilder,
            XGCommonJsonChangeTrackingOptions? options)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = propertyBuilder.Metadata;
            property.SetJsonChangeTrackingOptions(options);

            return propertyBuilder;
        }
    }
}
