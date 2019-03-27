// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Represents configuration for which warnings should be thrown, logged, or ignored.
    ///         by database providers or extensions. These options are set using <see cref="WarningsConfigurationBuilder" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are designed to be immutable. To change an option, call one of the 'With...'
    ///         methods to obtain a new instance with the option changed.
    ///     </para>
    /// </summary>
    public class WarningsConfiguration
    {
        private Dictionary<int, WarningBehavior> _explicitBehaviors
            = new Dictionary<int, WarningBehavior>();

        private WarningBehavior _defaultBehavior = WarningBehavior.Log;

        private long? _serviceProviderHash;

        /// <summary>
        ///     Creates a new, empty configuration, with all options set to their defaults.
        /// </summary>
        public WarningsConfiguration()
        {
        }

        /// <summary>
        ///     Called by a derived class constructor when implementing the <see cref="Clone" /> method.
        /// </summary>
        /// <param name="copyFrom"> The instance that is being cloned. </param>
        protected WarningsConfiguration([NotNull] WarningsConfiguration copyFrom)
        {
            _defaultBehavior = copyFrom._defaultBehavior;
            _explicitBehaviors = copyFrom._explicitBehaviors;
        }

        /// <summary>
        ///     Override this method in a derived class to ensure that any clone created is also of that class.
        /// </summary>
        /// <returns> A clone of this instance, which can be modified before being returned as immutable. </returns>
        protected virtual WarningsConfiguration Clone() => new WarningsConfiguration(this);

        /// <summary>
        ///     The option set from the <see cref="DefaultBehavior" /> method.
        /// </summary>
        public virtual WarningBehavior DefaultBehavior => _defaultBehavior;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
        /// </summary>
        /// <param name="warningBehavior"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual WarningsConfiguration WithDefaultBehavior(WarningBehavior warningBehavior)
        {
            var clone = Clone();

            clone._defaultBehavior = warningBehavior;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with the given explicit <see cref="WarningBehavior" /> set for
        ///     all given event IDs.
        ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
        /// </summary>
        /// <param name="eventIds"> The event IDs for which the behavior should be set. </param>
        /// <param name="warningBehavior"> The behavior to set. </param>
        /// <returns> A new instance with the behaviors set. </returns>
        public virtual WarningsConfiguration WithExplicit(
            [NotNull] IEnumerable<EventId> eventIds, WarningBehavior warningBehavior)
        {
            var clone = Clone();

            clone._explicitBehaviors = new Dictionary<int, WarningBehavior>(_explicitBehaviors);

            foreach (var eventId in eventIds)
            {
                clone._explicitBehaviors[eventId.Id] = warningBehavior;
            }

            return clone;
        }

        /// <summary>
        ///     Gets the <see cref="WarningBehavior" /> set for the given event ID, or the <see cref="DefaultBehavior" />
        ///     if no explicit behavior has been set.
        /// </summary>
        public virtual WarningBehavior? GetBehavior(EventId eventId)
            => _explicitBehaviors.TryGetValue(eventId.Id, out var warningBehavior)
                ? (WarningBehavior?)warningBehavior
                : null;

        /// <summary>
        ///     Creates a new instance with the given explicit <see cref="WarningBehavior" /> set for
        ///     the given event ID, but only if no explicit behavior has already been set.
        ///     It is unusual to call this method directly. Instead use <see cref="WarningsConfigurationBuilder" />.
        /// </summary>
        /// <param name="eventId"> The event ID for which the behavior should be set. </param>
        /// <param name="warningBehavior"> The behavior to set. </param>
        /// <returns> A new instance with the behavior set, or this instance if a behavior was already set. </returns>
        public virtual WarningsConfiguration TryWithExplicit(EventId eventId, WarningBehavior warningBehavior)
            => _explicitBehaviors.ContainsKey(eventId.Id)
                ? this
                : WithExplicit(new[] { eventId }, warningBehavior);

        /// <summary>
        ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
        ///     to be needed.
        /// </summary>
        /// <returns> A hash over options that require a new service provider when changed. </returns>
        public virtual long GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = (long)_defaultBehavior.GetHashCode();

                if (_explicitBehaviors != null)
                {
                    hashCode = _explicitBehaviors.Aggregate(
                        hashCode,
                        (t, e) => (t * 397) ^ (((long)e.Value.GetHashCode() * 3163) ^ (long)e.Key.GetHashCode()));
                }

                _serviceProviderHash = hashCode;
            }

            return _serviceProviderHash.Value;
        }
    }
}
