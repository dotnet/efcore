// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     invalid include path information.
    /// </summary>
    public class InvalidIncludePathEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="navigationChain"> Navigation chain included to this point. </param>
        /// <param name="navigationName"> The name of the invalid navigation. </param>
        public InvalidIncludePathEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] string navigationChain,
            [NotNull] string navigationName)
            : base(eventDefinition, messageGenerator)
        {
            NavigationChain = navigationChain;
            NavigationName = navigationName;
        }

        /// <summary>
        ///     Navigation chain included to this point.
        /// </summary>
        public virtual string NavigationChain { get; }

        /// <summary>
        ///     The name of the invalid navigation.
        /// </summary>
        public virtual string NavigationName { get; }
    }
}
