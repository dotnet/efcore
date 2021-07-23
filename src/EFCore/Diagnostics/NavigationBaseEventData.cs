﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have an <see cref="INavigationBase" />.
    /// </summary>
    public class NavigationBaseEventData : EventData, INavigationBaseEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="navigationBase"> The navigation base. </param>
        public NavigationBaseEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            IReadOnlyNavigationBase navigationBase)
            : base(eventDefinition, messageGenerator)
        {
            NavigationBase = navigationBase;
        }

        /// <summary>
        ///     The navigation base.
        /// </summary>
        public virtual IReadOnlyNavigationBase NavigationBase { get; }

        INavigationBase INavigationBaseEventData.NavigationBase
            => (INavigationBase)NavigationBase;
    }
}
