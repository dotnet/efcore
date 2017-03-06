// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="WarningsConfigurationBuilder" />.
    /// </summary>
    public static class WarningConfigurationBuilderExtensions
    {
        /// <summary>
        ///     Causes an exception to be thrown when the specified relational database warnings are generated.
        /// </summary>
        /// <param name="warningsConfigurationBuilder"> The builder being used to configure warnings. </param>
        /// <param name="relationalEventIds">
        ///     The <see cref="RelationalEventId" />(s) for the warnings.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static WarningsConfigurationBuilder Throw(
            [NotNull] this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params RelationalEventId[] relationalEventIds)
        {
            Check.NotNull(warningsConfigurationBuilder, nameof(warningsConfigurationBuilder));
            Check.NotNull(relationalEventIds, nameof(relationalEventIds));

            return warningsConfigurationBuilder.WithOption(
                e => e.WithExplicit(relationalEventIds.Cast<object>(), WarningBehavior.Throw));
        }

        /// <summary>
        ///     Causes a warning to be logged when the specified relational database warnings are generated.
        /// </summary>
        /// <param name="warningsConfigurationBuilder"> The builder being used to configure warnings. </param>
        /// <param name="relationalEventIds">
        ///     The <see cref="RelationalEventId" />(s) for the warnings.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static WarningsConfigurationBuilder Log(
            [NotNull] this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params RelationalEventId[] relationalEventIds)
        {
            Check.NotNull(warningsConfigurationBuilder, nameof(warningsConfigurationBuilder));
            Check.NotNull(relationalEventIds, nameof(relationalEventIds));

            return warningsConfigurationBuilder.WithOption(
                e => e.WithExplicit(relationalEventIds.Cast<object>(), WarningBehavior.Log));
        }

        /// <summary>
        ///     Causes nothing to happen when the specified relational database warnings are generated.
        /// </summary>
        /// <param name="warningsConfigurationBuilder"> The builder being used to configure warnings. </param>
        /// <param name="relationalEventIds">
        ///     The <see cref="RelationalEventId" />(s) for the warnings.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static WarningsConfigurationBuilder Ignore(
            [NotNull] this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params RelationalEventId[] relationalEventIds)
        {
            Check.NotNull(warningsConfigurationBuilder, nameof(warningsConfigurationBuilder));
            Check.NotNull(relationalEventIds, nameof(relationalEventIds));

            return warningsConfigurationBuilder.WithOption(
                e => e.WithExplicit(relationalEventIds.Cast<object>(), WarningBehavior.Ignore));
        }

        private static WarningsConfigurationBuilder WithOption(
            this WarningsConfigurationBuilder warningsConfigurationBuilder,
            Func<WarningsConfiguration, WarningsConfiguration> withFunc)
        {
            var optionsBuilder = warningsConfigurationBuilder.OptionsBuilder;

            var coreOptionsExtension = optionsBuilder.Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            coreOptionsExtension.WithWarningsConfiguration(withFunc(coreOptionsExtension.WarningsConfiguration));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
                coreOptionsExtension.WithWarningsConfiguration(withFunc(coreOptionsExtension.WarningsConfiguration)));

            return warningsConfigurationBuilder;
        }
    }
}
