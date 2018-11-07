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
    ///     debug information on service provider creation.
    /// </summary>
    public class ServiceProviderDebugInfoEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="newDebugInfo"> The debug information for the new provider. </param>
        /// <param name="cachedDebugInfos"> The debug information for existing providers. </param>
        public ServiceProviderDebugInfoEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IDictionary<string, string> newDebugInfo,
            [NotNull] IList<IDictionary<string, string>> cachedDebugInfos)
            : base(eventDefinition, messageGenerator)
        {
            NewDebugInfo = newDebugInfo;
            CachedDebugInfos = cachedDebugInfos;
        }

        /// <summary>
        ///     The debug information for the new provider.
        /// </summary>
        public virtual IDictionary<string, string> NewDebugInfo { get; }

        /// <summary>
        ///     The debug information for existing providers.
        /// </summary>
        public virtual IList<IDictionary<string, string>> CachedDebugInfos { get; }
    }
}
