// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Carries information about a parameter binding.
    /// </summary>
    public readonly struct ParameterBindingInfo
    {
        /// <summary>
        ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
        /// </summary>
        /// <param name="entityType"> The entity type for this binding. </param>
        /// <param name="materializationContextExpression"> The expression tree from which the parameter value will come. </param>
        public ParameterBindingInfo(
            [NotNull] IEntityType entityType,
            [NotNull] Expression materializationContextExpression)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(materializationContextExpression, nameof(materializationContextExpression));

            EntityType = entityType;
            MaterializationContextExpression = materializationContextExpression;
        }

        /// <summary>
        ///     The entity type for this binding.
        /// </summary>
        public IEntityType EntityType { get; }

        /// <summary>
        ///     The expression tree from which the parameter value will come.
        /// </summary>
        public Expression MaterializationContextExpression { get; }

        /// <summary>
        ///     Gets the index into the <see cref="ValueBuffer" /> where the property value can be found.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The index where its value can be found. </returns>
        public int GetValueBufferIndex([NotNull] IPropertyBase property) => property.GetIndex();
    }
}
