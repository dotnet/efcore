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
            [NotNull] this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params InMemoryEventId[] inMemoryEventIds)
        {
            Check.NotNull(warningsConfigurationBuilder, nameof(warningsConfigurationBuilder));
            Check.NotNull(inMemoryEventIds, nameof(inMemoryEventIds));

            warningsConfigurationBuilder.Configuration
                .AddExplicit(inMemoryEventIds.Cast<object>(), WarningBehavior.Throw);

            return warningsConfigurationBuilder;
        }

        public static WarningsConfigurationBuilder Log(
            [NotNull] this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params InMemoryEventId[] inMemoryEventIds)
        {
            Check.NotNull(warningsConfigurationBuilder, nameof(warningsConfigurationBuilder));
            Check.NotNull(inMemoryEventIds, nameof(inMemoryEventIds));

            warningsConfigurationBuilder.Configuration
                .AddExplicit(inMemoryEventIds.Cast<object>(), WarningBehavior.Log);

            return warningsConfigurationBuilder;
        }

        public static WarningsConfigurationBuilder Ignore(
            [NotNull] this WarningsConfigurationBuilder warningsConfigurationBuilder,
            [NotNull] params InMemoryEventId[] inMemoryEventIds)
        {
            Check.NotNull(warningsConfigurationBuilder, nameof(warningsConfigurationBuilder));
            Check.NotNull(inMemoryEventIds, nameof(inMemoryEventIds));

            warningsConfigurationBuilder.Configuration
                .AddExplicit(inMemoryEventIds.Cast<object>(), WarningBehavior.Ignore);

            return warningsConfigurationBuilder;
        }
    }
}
