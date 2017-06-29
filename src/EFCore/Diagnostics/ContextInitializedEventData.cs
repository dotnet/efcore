// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for context initialization events.
    /// </summary>
    public class ContextInitializedEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="context"> The <see cref="DbContext" /> that is initialized. </param>
        /// <param name="contextOptions"> The <see cref="DbContextOptions" /> being used. </param>
        public ContextInitializedEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] DbContext context,
            [NotNull] DbContextOptions contextOptions)
            : base(eventDefinition, messageGenerator)
        {
            Context = context;
            ContextOptions = contextOptions;
        }

        /// <summary>
        ///     The <see cref="DbContext" /> that is initialized.
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        ///     The <see cref="DbContextOptions" /> being used.
        /// </summary>
        public virtual DbContextOptions ContextOptions { get; }
    }
}
