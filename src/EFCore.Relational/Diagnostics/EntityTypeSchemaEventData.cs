// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     The <see cref="DiagnosticSource" /> event payload base class for events that
    ///     reference an entity type and a schema
    /// </summary>
    public class EntityTypeSchemaEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="schema"> The schema. </param>
        public EntityTypeSchemaEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IEntityType entityType,
            [NotNull] string schema)
            : base(eventDefinition, messageGenerator)
        {
            EntityType = entityType;
            Schema = schema;
        }

        /// <summary>
        ///     The entity type.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     The schema.
        /// </summary>
        public virtual string Schema { get; }
    }
}
