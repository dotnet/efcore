// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     a <see cref="IServiceProvider" /> container.
    /// </summary>
    public class ServiceProviderEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="serviceProvider"> The <see cref="IServiceProvider" />. </param>
        public ServiceProviderEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IServiceProvider serviceProvider)
            : base(eventDefinition, messageGenerator)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        ///     The <see cref="IServiceProvider" />.
        /// </summary>
        public virtual IServiceProvider ServiceProvider { get; }
    }
}
