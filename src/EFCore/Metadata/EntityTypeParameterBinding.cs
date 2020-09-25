// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
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
        /// <param name="serviceProperty"> The associated <see cref="IServiceProperty" />, or null. </param>
        public EntityTypeParameterBinding([CanBeNull] IPropertyBase serviceProperty = null)
            : base(typeof(IEntityType), typeof(IEntityType), serviceProperty)
        {
        }

        /// <inheritdoc />
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression)
            => Check.NotNull(entityTypeExpression, nameof(entityTypeExpression));
    }
}
