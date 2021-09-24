// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Carries information about a parameter binding.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information.
    /// </remarks>
    public readonly struct ParameterBindingInfo
    {
        /// <summary>
        ///     Creates a new <see cref="ParameterBindingInfo" /> to define a parameter binding.
        /// </summary>
        /// <param name="entityType">The entity type for this binding.</param>
        /// <param name="materializationContextExpression">The expression tree from which the parameter value will come.</param>
        public ParameterBindingInfo(
            IEntityType entityType,
            Expression materializationContextExpression)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(entityType, nameof(materializationContextExpression));

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
        /// <param name="property">The property.</param>
        /// <returns>The index where its value can be found.</returns>
        public int GetValueBufferIndex(IPropertyBase property)
            => property.GetIndex();
    }
}
