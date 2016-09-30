// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class WarningsConfiguration
    {
        private readonly Dictionary<object, WarningBehavior> _explicitBehaviors
            = new Dictionary<object, WarningBehavior>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual WarningBehavior DefaultBehavior { get; set; } = WarningBehavior.Log;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddExplicit(
            [NotNull] IEnumerable<object> eventIds, WarningBehavior warningBehavior)
        {
            Check.NotNull(eventIds, nameof(eventIds));

            foreach (var eventId in eventIds)
            {
                _explicitBehaviors[eventId] = warningBehavior;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual WarningBehavior GetBehavior([NotNull] object eventId)
        {
            Check.NotNull(eventId, nameof(eventId));

            WarningBehavior warningBehavior;
            return _explicitBehaviors.TryGetValue(eventId, out warningBehavior)
                ? warningBehavior
                : DefaultBehavior;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void TryAddExplicit([NotNull] object eventId, WarningBehavior warningBehavior)
        {
            Check.NotNull(eventId, nameof(eventId));

            if (!_explicitBehaviors.ContainsKey(eventId))
            {
                _explicitBehaviors[eventId] = warningBehavior;
            }
        }
    }
}
