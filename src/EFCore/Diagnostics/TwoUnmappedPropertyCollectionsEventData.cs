// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     two unmapped property collections.
    /// </summary>
    public class TwoUnmappedPropertyCollectionsEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="firstPropertyCollection"> The first property collection. </param>
        /// <param name="secondPropertyCollection"> The second property collection. </param>
        public TwoUnmappedPropertyCollectionsEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            IEnumerable<Tuple<MemberInfo?, Type>> firstPropertyCollection,
            IEnumerable<Tuple<MemberInfo?, Type>> secondPropertyCollection)
            : base(eventDefinition, messageGenerator)
        {
            FirstPropertyCollection = firstPropertyCollection;
            SecondPropertyCollection = secondPropertyCollection;
        }

        /// <summary>
        ///     The first property collection.
        /// </summary>
        public virtual IEnumerable<Tuple<MemberInfo?, Type>> FirstPropertyCollection { get; }

        /// <summary>
        ///     The second property collection.
        /// </summary>
        public virtual IEnumerable<Tuple<MemberInfo?, Type>> SecondPropertyCollection { get; }
    }
}
