// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Values that are used as the eventId when logging messages from the core Entity Framework components.
    /// </summary>
    public enum CoreEventId
    {
        /// <summary>
        ///     An error occurred while accessing the database.
        /// </summary>
        DatabaseError = 1,

        /// <summary>
        ///     A LINQ query is being compiled.
        /// </summary>
        CompilingQueryModel,

        /// <summary>
        ///     An object model representing a LINQ query was optimized.
        /// </summary>
        OptimizedQueryModel,

        /// <summary>
        ///     A navigation property that was included in a LINQ query is being processed.
        /// </summary>
        IncludingNavigation,

        /// <summary>
        ///     An execution expression was calculated by compiling a LINQ query.
        /// </summary>
        QueryPlan,

        /// <summary>
        ///     A query specified an Include operation that was ignored because the included navigation was not reachable in the final query result.
        /// </summary>
        IncludeIgnoredWarning,

        /// <summary>
        ///     A warning that sensitive data logging is enabled.
        /// </summary>
        SensitiveDataLoggingEnabledWarning,

        /// <summary>
        ///     A warning during model validation indicating a key is configured on shadow properties.
        /// </summary>
        ModelValidationShadowKeyWarning
    }
}
