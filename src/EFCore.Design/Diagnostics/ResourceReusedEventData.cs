// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that identify
    ///     a resource such as a name or filename that is being re-used.
    /// </summary>
    public class ResourceReusedEventData : EventDataBase
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="resourceName"> The resource name. </param>
        public ResourceReusedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventDataBase, string> messageGenerator,
            [NotNull] string resourceName)
            : base(eventDefinition, messageGenerator)
        {
            ResourceName = resourceName;
        }

        /// <summary>
        ///     The resource name.
        /// </summary>
        public virtual string ResourceName { get; }
    }
}