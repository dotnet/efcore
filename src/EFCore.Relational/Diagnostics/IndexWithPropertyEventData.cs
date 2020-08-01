// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for
    ///     the events involving an invalid property name on an index.
    /// </summary>
    public class IndexWithPropertyEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload for indexes with a invalid property.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="entityType"> The entity type on which the index is defined. </param>
        /// <param name="indexName"> The name of the index. </param>
        /// <param name="indexPropertyNames"> The names of the properties which define the index. </param>
        /// <param name="invalidPropertyName"> The property name which is invalid. </param>
        public IndexWithPropertyEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IEntityType entityType,
            [CanBeNull] string indexName,
            [NotNull] List<string> indexPropertyNames,
            [NotNull] string invalidPropertyName)
            : base(eventDefinition, messageGenerator)
        {
            EntityType = entityType;
            Name = indexName;
            PropertyNames = indexPropertyNames;
            PropertyName = invalidPropertyName;
        }

        /// <summary>
        ///     The entity type on which the index is defined.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     The name of the index.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     The list of properties which define the index.
        /// </summary>
        public virtual List<string> PropertyNames { get; }

        /// <summary>
        ///     The name of the specific property.
        /// </summary>
        public virtual string PropertyName { get; }
    }
}
