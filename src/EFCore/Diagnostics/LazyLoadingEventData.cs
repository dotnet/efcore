// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events from <see cref="ILazyLoader" />
    /// </summary>
    public class LazyLoadingEventData : DbContextEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="context"> The current <see cref="DbContext" />. </param>
        /// <param name="entity"> The entity instance on which lazy-loading was initiated. </param>
        /// <param name="navigationPropertyName"> The navigation property name of the relationship to be loaded. </param>
        public LazyLoadingEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            DbContext context,
            object entity,
            string navigationPropertyName)
            : base(eventDefinition, messageGenerator, context)
        {
            Entity = entity;
            NavigationPropertyName = navigationPropertyName;
        }

        /// <summary>
        ///     The entity instance on which lazy-loading was initiated.
        /// </summary>
        public virtual object Entity { get; }

        /// <summary>
        ///     The navigation property name of the relationship to be loaded.
        /// </summary>
        public virtual string NavigationPropertyName { get; }
    }
}
