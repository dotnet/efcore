// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <inheritdoc />
    public class ContextParameterBinding : ServiceParameterBinding
    {
        /// <summary>
        ///     Creates a new <see cref="ServiceParameterBinding" /> instance for the given service type.
        /// </summary>
        /// <param name="contextType"> The <see cref="DbContext" /> CLR type. </param>
        /// <param name="serviceProperty"> The associated <see cref="IServiceProperty" />, or null. </param>
        public ContextParameterBinding(
            [NotNull] Type contextType,
            [CanBeNull] IPropertyBase serviceProperty = null)
            : base(contextType, contextType, serviceProperty)
        {
        }

        /// <inheritdoc />
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression)
        {
            Check.NotNull(materializationExpression, nameof(materializationExpression));
            Check.NotNull(entityTypeExpression, nameof(entityTypeExpression));

            var propertyExpression
                = Expression.Property(
                    materializationExpression,
                    MaterializationContext.ContextProperty);

            return ServiceType != typeof(DbContext)
                ? (Expression)Expression.TypeAs(propertyExpression, ServiceType)
                : propertyExpression;
        }
    }
}
