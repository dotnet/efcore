// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Reflection;
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information.
    /// </remarks>
    public class DependencyInjectionParameterBinding : ServiceParameterBinding
    {
        private static readonly MethodInfo _getServiceMethod
            = typeof(InfrastructureExtensions).GetMethod(nameof(InfrastructureExtensions.GetService))!;

        /// <summary>
        ///     Creates a new <see cref="DependencyInjectionParameterBinding" /> instance for the given service type.
        /// </summary>
        /// <param name="parameterType">The parameter CLR type.</param>
        /// <param name="serviceType">The service CLR types, as resolved from dependency injection</param>
        /// <param name="serviceProperties">The associated <see cref="IServiceProperty" /> objects, or <see langword="null" />.</param>
        public DependencyInjectionParameterBinding(
            Type parameterType,
            Type serviceType,
            params IPropertyBase[]? serviceProperties)
            : base(parameterType, serviceType, serviceProperties)
        {
        }

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="materializationExpression">The expression representing the materialization context.</param>
        /// <param name="entityTypeExpression">The expression representing the <see cref="IEntityType" /> constant.</param>
        /// <returns>The expression tree.</returns>
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

        /// <summary>
        ///     Creates a copy that contains the given consumed properties.
        /// </summary>
        /// <param name="consumedProperties">The new consumed properties.</param>
        /// <returns>A copy with replaced consumed properties.</returns>
        public override ParameterBinding With(IPropertyBase[] consumedProperties)
            => new DependencyInjectionParameterBinding(ParameterType, ServiceType, consumedProperties);
    }
}
