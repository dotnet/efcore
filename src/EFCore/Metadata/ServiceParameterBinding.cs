// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Describes the binding from an EF dependency injection service, or metadata type, which may or
    ///     may not also have and associated <see cref="IServiceProperty" />, to a parameter in
    ///     a constructor, factory method, or similar.
    /// </summary>
    public abstract class ServiceParameterBinding : ParameterBinding
    {
        private Func<MaterializationContext, IEntityType, object, object>? _serviceDelegate;

        /// <summary>
        ///     Creates a new <see cref="ServiceParameterBinding" /> instance for the given service type
        ///     or metadata type.
        /// </summary>
        /// <param name="parameterType"> The parameter CLR type. </param>
        /// <param name="serviceType"> The service or metadata CLR type. </param>
        /// <param name="serviceProperties"> The associated <see cref="IServiceProperty" /> instances, or null. </param>
        protected ServiceParameterBinding(
            Type parameterType,
            Type serviceType,
            IPropertyBase[]? serviceProperties = null)
            : base(parameterType, serviceProperties)
        {
            Check.NotNull(serviceType, nameof(serviceType));

            ServiceType = serviceType;
        }

        /// <summary>
        ///     The EF internal service CLR type.
        /// </summary>
        public virtual Type ServiceType { get; }

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="bindingInfo"> The binding information. </param>
        /// <returns> The expression tree. </returns>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
            => BindToParameter(
                bindingInfo.MaterializationContextExpression,
                Expression.Constant(bindingInfo.EntityType));

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="materializationExpression"> The expression representing the materialization context. </param>
        /// <param name="entityTypeExpression"> The expression representing the <see cref="IEntityType" /> constant. </param>
        /// <returns> The expression tree. </returns>
        public abstract Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression);

        /// <summary>
        ///     A delegate to set a CLR service property on an entity instance.
        /// </summary>
        public virtual Func<MaterializationContext, IEntityType, object, object> ServiceDelegate
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _serviceDelegate, this, static b =>
                {
                    var materializationContextParam = Expression.Parameter(typeof(MaterializationContext));
                    var entityTypeParam = Expression.Parameter(typeof(IEntityType));
                    var entityParam = Expression.Parameter(typeof(object));

                    return Expression.Lambda<Func<MaterializationContext, IEntityType, object, object>>(
                        b.BindToParameter(materializationContextParam, entityTypeParam),
                        materializationContextParam,
                        entityTypeParam,
                        entityParam).Compile();
                });
    }
}
