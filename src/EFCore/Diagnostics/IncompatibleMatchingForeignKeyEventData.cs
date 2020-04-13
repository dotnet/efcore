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
    ///     A <see cref="DiagnosticSource" /> event payload class for the 
    ///     IncompatibleMatchingForeignKeyProperties event.
    /// </summary>
    public class IncompatibleMatchingForeignKeyEventData : TwoPropertyBaseCollectionsEventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="principalEntityType"> The entity type on the principal end of the relationship. </param>
        /// <param name="dependentEntityType"> The entity type on the dependent end of the relationship. </param>
        /// <param name="firstPropertyCollection"> The first property collection. </param>
        /// <param name="secondPropertyCollection"> The second property collection. </param>
        public IncompatibleMatchingForeignKeyEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] IConventionEntityType principalEntityType,
            [NotNull] IConventionEntityType dependentEntityType,
            [NotNull] IReadOnlyList<IPropertyBase> firstPropertyCollection,
            [NotNull] IReadOnlyList<IPropertyBase> secondPropertyCollection)
            : base(eventDefinition, messageGenerator, firstPropertyCollection, secondPropertyCollection)
        {
            PrincipalEntityType = principalEntityType;
            DependentEntityType = dependentEntityType;
        }

        /// <summary>
        ///     The principal entity type.
        /// </summary>
        public virtual IConventionEntityType PrincipalEntityType { get; }

        /// <summary>
        ///     The dependent entity type.
        /// </summary>
        public virtual IConventionEntityType DependentEntityType { get; }
    }
}
