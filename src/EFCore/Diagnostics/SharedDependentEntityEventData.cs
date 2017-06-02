// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that reference
    ///     two <see cref="IEntityType" /> instances.
    /// </summary>
    public class SharedDependentEntityEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="firstEntityType"> The first <see cref="IEntityType" />. </param>
        /// <param name="secondEntityType"> The second <see cref="IEntityType" />. </param>
        public SharedDependentEntityEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IEntityType firstEntityType,
            [NotNull] IEntityType secondEntityType)
            : base(eventDefinition, messageGenerator)
        {
            FirstEntityType = firstEntityType;
            SecondEntityType = secondEntityType;
        }

        /// <summary>
        ///     The first <see cref="IEntityType" />.
        /// </summary>
        public virtual IEntityType FirstEntityType { get; }

        /// <summary>
        ///     The second <see cref="IEntityType" />.
        /// </summary>
        public virtual IEntityType SecondEntityType { get; }
    }
}
