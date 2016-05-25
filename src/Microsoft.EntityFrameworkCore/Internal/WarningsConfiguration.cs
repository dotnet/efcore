// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class WarningsConfiguration
    {
        private readonly Dictionary<object, WarningBehavior> _explicitBehaviors
            = new Dictionary<object, WarningBehavior>();

        public virtual WarningBehavior DefaultBehavior { get; set; } = WarningBehavior.Log;

        public virtual void AddExplicit(
            [NotNull] IEnumerable<object> eventIds, WarningBehavior warningBehavior)
        {
            Check.NotNull(eventIds, nameof(eventIds));

            foreach (var eventId in eventIds)
            {
                _explicitBehaviors[eventId] = warningBehavior;
            }
        }

        public virtual WarningBehavior GetBehavior([NotNull] object eventId)
        {
            Check.NotNull(eventId, nameof(eventId));

            WarningBehavior warningBehavior;
            return _explicitBehaviors.TryGetValue(eventId, out warningBehavior)
                ? warningBehavior
                : DefaultBehavior;
        }
    }
}
