// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for events that have
    ///     a <see cref="ValueConverter" />.
    /// </summary>
    public class ValueConverterEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="mappingClrType"> The CLR type. </param>
        /// <param name="valueConverter"> The <see cref="ValueConverter" />. </param>
        public ValueConverterEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] Type mappingClrType,
            [NotNull] ValueConverter valueConverter)
            : base(eventDefinition, messageGenerator)
        {
            MappingClrType = mappingClrType;
            ValueConverter = valueConverter;
        }

        /// <summary>
        ///     The CLR type.
        /// </summary>
        public virtual Type MappingClrType { get; }

        /// <summary>
        ///     The <see cref="ValueConverter" />.
        /// </summary>
        public virtual ValueConverter ValueConverter { get; }
    }
}
