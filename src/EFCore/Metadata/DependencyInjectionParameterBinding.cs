// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Describes the binding from an EF internal dependency injection service, which may or may not
    ///     also have and associated <see cref="IServiceProperty" />, to a parameter in a constructor,
    ///     factory method, or similar.
    /// </summary>
    public class DependencyInjectionParameterBinding : ServiceParameterBinding
    {
        private static readonly MethodInfo _getServiceMethod
            = typeof(InfrastructureExtensions).GetMethod(nameof(InfrastructureExtensions.GetService));

        /// <summary>
        ///     Creates a new <see cref="DependencyInjectionParameterBinding" /> instance for the given service type.
        /// </summary>
        /// <param name="parameterType"> The parameter CLR type. </param>
        /// <param name="serviceType"> The service CLR types, as resolved from dependency injection </param>
        /// <param name="serviceProperty"> The associated <see cref="IServiceProperty" />, or null. </param>
        public DependencyInjectionParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [CanBeNull] IPropertyBase serviceProperty = null)
            : base(parameterType, serviceType, serviceProperty)
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
        {
            Check.NotNull(materializationExpression, nameof(materializationExpression));
            Check.NotNull(entityTypeExpression, nameof(entityTypeExpression));

            return Expression.Call(
                _getServiceMethod.MakeGenericMethod(ServiceType),
                Expression.Convert(
                    Expression.Property(
                        materializationExpression,
                        MaterializationContext.ContextProperty),
                    typeof(IInfrastructure<IServiceProvider>)));
        }
    }
}
