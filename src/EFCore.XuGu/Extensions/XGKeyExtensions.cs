// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IKey" /> for MySQL-specific metadata.
    /// </summary>
    public static class XGKeyExtensions
    {
        /// <summary>
        ///     Returns prefix lengths for the key.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The prefix lengths.
        /// A value of `0` indicates, that the full length should be used for that column. </returns>
        public static int[] PrefixLength([NotNull] this IKey key)
            => (key is RuntimeKey)
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : (int[])key[XGAnnotationNames.IndexPrefixLength];

        /// <summary>
        ///     Sets prefix lengths for the key.
        /// </summary>
        /// <param name="values"> The prefix lengths to set.
        /// A value of `0` indicates, that the full length should be used for that column. </param>
        /// <param name="key"> The key. </param>
        public static void SetPrefixLength([NotNull] this IMutableKey key, int[] values)
            => key.SetOrRemoveAnnotation(
                XGAnnotationNames.IndexPrefixLength,
                values);

        /// <summary>
        ///     Sets prefix lengths for the key.
        /// </summary>
        /// <param name="values"> The prefix lengths to set.
        /// A value of `0` indicates, that the full length should be used for that column. </param>
        /// <param name="key"> The key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetPrefixLength(
            [NotNull] this IConventionKey key, int[] values, bool fromDataAnnotation = false)
            => key.SetOrRemoveAnnotation(
                XGAnnotationNames.IndexPrefixLength,
                values,
                fromDataAnnotation);

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for prefix lengths of the key.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for prefix lengths of the key. </returns>
        public static ConfigurationSource? GetPrefixLengthConfigurationSource([NotNull] this IConventionKey property)
            => property.FindAnnotation(XGAnnotationNames.IndexPrefixLength)?.GetConfigurationSource();
    }
}
