// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload base class for events that
    ///     reference a sequence.
    /// </summary>
    public class SequenceEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="sequence"> The sequence. </param>
        public SequenceEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            IReadOnlySequence sequence)
            : base(eventDefinition, messageGenerator)
        {
            Sequence = sequence;
        }

        /// <summary>
        ///     The sequence.
        /// </summary>
        public virtual IReadOnlySequence Sequence { get; }
    }
}
