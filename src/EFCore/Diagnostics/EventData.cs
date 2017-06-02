// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A base class for all Entity Framework <see cref="DiagnosticSource" /> event payloads.
    /// </summary>
    public class EventData
    {
        private readonly EventDefinitionBase _eventDefinition;
        private readonly Func<EventDefinitionBase, EventData, string> _messageGenerator;

        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        public EventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator)
        {
            _eventDefinition = eventDefinition;
            _messageGenerator = messageGenerator;
        }

        /// <summary>
        ///     The <see cref="EventId" /> that defines the message ID and name.
        /// </summary>
        public virtual EventId EventId => _eventDefinition.EventId;

        /// <summary>
        ///     The <see cref="LogLevel" /> that would be used to log message for this event.
        /// </summary>
        public virtual LogLevel LogLevel => _eventDefinition.Level;

        /// <summary>
        ///     A logger message describing this event.
        /// </summary>
        /// <returns> A logger message describing this event. </returns>
        public override string ToString() => _messageGenerator(_eventDefinition, this);
    }
}
