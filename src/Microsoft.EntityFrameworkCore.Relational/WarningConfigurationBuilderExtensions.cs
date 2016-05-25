// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class WarningConfigurationBuilderExtensions
    {
        public static WarningsConfigurationBuilder Throw(
            this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params RelationalEventId[] relationalEventIds)
        {
            Check.NotNull(relationalEventIds, nameof(relationalEventIds));

            warningsConfigurationBuilder.Configuration
                .AddExplicit(relationalEventIds.Cast<object>(), WarningBehavior.Throw);

            return warningsConfigurationBuilder;
        }

        public static WarningsConfigurationBuilder Log(
            this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params RelationalEventId[] relationalEventIds)
        {
            Check.NotNull(relationalEventIds, nameof(relationalEventIds));

            warningsConfigurationBuilder.Configuration
                .AddExplicit(relationalEventIds.Cast<object>(), WarningBehavior.Log);

            return warningsConfigurationBuilder;
        }

        public static WarningsConfigurationBuilder Ignore(
            this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params RelationalEventId[] relationalEventIds)
        {
            Check.NotNull(relationalEventIds, nameof(relationalEventIds));

            warningsConfigurationBuilder.Configuration
                .AddExplicit(relationalEventIds.Cast<object>(), WarningBehavior.Ignore);

            return warningsConfigurationBuilder;
        }
    }
}
