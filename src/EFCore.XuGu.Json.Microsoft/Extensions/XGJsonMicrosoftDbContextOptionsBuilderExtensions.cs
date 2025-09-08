// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Json specific extension methods for <see cref="XGDbContextOptionsBuilder" />.
    /// </summary>
    public static class XGJsonMicrosoftDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Use System.Text.Json to access XuGu JSON data.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure XuGu. </param>
        /// <param name="options">
        ///     Configures the context to use the specified change tracking option as the default for all JSON column
        ///     mapped properties.
        /// </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static XGDbContextOptionsBuilder UseMicrosoftJson(
            [NotNull] this XGDbContextOptionsBuilder optionsBuilder,
            XGCommonJsonChangeTrackingOptions options = XGCommonJsonChangeTrackingOptions.RootPropertyOnly)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = (coreOptionsBuilder.Options.FindExtension<XGJsonMicrosoftOptionsExtension>() ??
                            new XGJsonMicrosoftOptionsExtension())
                .WithJsonChangeTrackingOptions(options);

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static XGDbContextOptionsBuilder UseMicrosoftJson(
            [NotNull] this XGDbContextOptionsBuilder optionsBuilder, XGJsonChangeTrackingOptions options)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = (coreOptionsBuilder.Options.FindExtension<XGJsonMicrosoftOptionsExtension>() ??
                             new XGJsonMicrosoftOptionsExtension())
                .WithJsonChangeTrackingOptions(options);

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
