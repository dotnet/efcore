// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalQueryCompilationContext" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    /// </summary>
    public sealed class RelationalEntityQueryableExpressionVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a
        ///         <see cref="RelationalEntityQueryableExpressionVisitorFactory" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="selectExpressionFactory"> The select expression factory. </param>
        /// <param name="materializerFactory"> The materializer factory. </param>
        /// <param name="shaperCommandContextFactory"> The shaper command context factory. </param>
        public RelationalEntityQueryableExpressionVisitorDependencies(
            [NotNull] IModel model,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));

            Model = model;
            SelectExpressionFactory = selectExpressionFactory;
            MaterializerFactory = materializerFactory;
            ShaperCommandContextFactory = shaperCommandContextFactory;
        }

        /// <summary>
        ///     The model.
        /// </summary>
        public IModel Model { get; }

        /// <summary>
        ///     The select expression factory.
        /// </summary>
        public ISelectExpressionFactory SelectExpressionFactory { get; }

        /// <summary>
        ///     The materializer factory.
        /// </summary>
        public IMaterializerFactory MaterializerFactory { get; }

        /// <summary>
        ///     The shaper command context factory.
        /// </summary>
        public IShaperCommandContextFactory ShaperCommandContextFactory { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="model"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalEntityQueryableExpressionVisitorDependencies With([NotNull] IModel model)
            => new RelationalEntityQueryableExpressionVisitorDependencies(
                model,
                SelectExpressionFactory,
                MaterializerFactory,
                ShaperCommandContextFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="selectExpressionFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalEntityQueryableExpressionVisitorDependencies With([NotNull] ISelectExpressionFactory selectExpressionFactory)
            => new RelationalEntityQueryableExpressionVisitorDependencies(
                Model,
                selectExpressionFactory,
                MaterializerFactory,
                ShaperCommandContextFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="materializerFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalEntityQueryableExpressionVisitorDependencies With([NotNull] IMaterializerFactory materializerFactory)
            => new RelationalEntityQueryableExpressionVisitorDependencies(
                Model,
                SelectExpressionFactory,
                materializerFactory,
                ShaperCommandContextFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="shaperCommandContextFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalEntityQueryableExpressionVisitorDependencies With([NotNull] IShaperCommandContextFactory shaperCommandContextFactory)
            => new RelationalEntityQueryableExpressionVisitorDependencies(
                Model,
                SelectExpressionFactory,
                MaterializerFactory,
                shaperCommandContextFactory);
    }
}
