// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     multiple <see cref="IServiceProvider" /> containers.
    /// </summary>
    public class ServiceProvidersEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="serviceProviders"> The <see cref="IServiceProvider" />s. </param>
        public ServiceProvidersEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] ICollection<IServiceProvider> serviceProviders)
            : base(eventDefinition, messageGenerator)
        {
            ServiceProviders = serviceProviders;
        }

        /// <summary>
        ///     The <see cref="IServiceProvider" />s.
        /// </summary>
        public virtual ICollection<IServiceProvider> ServiceProviders { get; }
    }
}
