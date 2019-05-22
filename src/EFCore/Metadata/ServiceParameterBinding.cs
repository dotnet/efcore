// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
        private Func<MaterializationContext, IEntityType, object, object> _serviceDelegate;

        /// <summary>
        ///     Creates a new <see cref="ServiceParameterBinding" /> instance for the given service type
        ///     or metadata type.
        /// </summary>
        /// <param name="parameterType"> The parameter CLR type. </param>
        /// <param name="serviceType"> The service or metadata CLR type. </param>
        /// <param name="serviceProperty"> The associated <see cref="IServiceProperty" />, or null. </param>
        protected ServiceParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [CanBeNull] IPropertyBase serviceProperty = null)
            : base(
                parameterType, serviceProperty != null
                    ? new[]
                    {
                        serviceProperty
                    }
                    : Array.Empty<IPropertyBase>())
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
            [NotNull] Expression materializationExpression,
            [NotNull] Expression entityTypeExpression);

        /// <summary>
        ///     A delegate to set a CLR service property on an entity instance.
        /// </summary>
        public virtual Func<MaterializationContext, IEntityType, object, object> ServiceDelegate
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _serviceDelegate, this, b =>
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
