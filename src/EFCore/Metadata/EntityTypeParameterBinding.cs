// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Describes the binding of a <see cref="IEntityType" />, which may or may not also have and associated
    ///     <see cref="IServiceProperty" />, to a parameter in a constructor, factory method, or similar.
    /// </summary>
    public class EntityTypeParameterBinding : ServiceParameterBinding
    {
        /// <summary>
        ///     Creates a new <see cref="EntityTypeParameterBinding" /> instance for the given service type.
        /// </summary>
        /// <param name="serviceProperties"> The associated <see cref="IServiceProperty" /> objects, or <see langword="null" />. </param>
        public EntityTypeParameterBinding(IPropertyBase[]? serviceProperties = null)
            : base(typeof(IEntityType), typeof(IEntityType), serviceProperties)
        {
        }

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="materializationExpression"> The expression representing the materialization context. </param>
        /// <param name="entityTypeExpression"> The expression representing the <see cref="IEntityType" /> constant. </param>
        /// <returns> The expression tree. </returns>
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression)
            => Check.NotNull(entityTypeExpression, nameof(entityTypeExpression));

        /// <summary>
        ///     Creates a copy that contains the given consumed properties.
        /// </summary>
        /// <param name="consumedProperties"> The new consumed properties. </param>
        /// <returns> A copy with replaced consumed properties. </returns>
        public override ParameterBinding With(IPropertyBase[] consumedProperties)
            => new EntityTypeParameterBinding(consumedProperties);
    }
}
