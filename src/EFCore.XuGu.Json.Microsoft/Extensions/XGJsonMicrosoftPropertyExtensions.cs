// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Extensions.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for XuGu-specific metadata.
    /// </summary>
    public static class XGJsonMicrosoftPropertyExtensions
    {
        /// <summary>
        /// Sets specific change tracking options for this JSON property, that specify how inner properties or array
        /// elements will be tracked. Applies to simple strings, POCOs and DOM objects. Using `null` restores all
        /// defaults.
        /// </summary>
        /// <param name="property">The JSON property to set the change tracking options for.</param>
        /// <param name="options">The change tracking option to configure for the JSON property.</param>
        public static void SetJsonChangeTrackingOptions([NotNull] this IMutableProperty property, XGCommonJsonChangeTrackingOptions? options)
            => property.SetJsonChangeTrackingOptions(options?.ToJsonChangeTrackingOptions());
    }
}
