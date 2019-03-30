// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="SelectExpression" />
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
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class SelectExpressionDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="SelectExpression" />.
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
        /// <param name="querySqlGeneratorFactory"> The query SQL generator factory. </param>
        /// <param name="typeMappingSource"> The type mapper. </param>
        /// <param name="commandBuilderFactory"> The command builder factory. </param>
        public SelectExpressionDependencies(
            [NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            [NotNull] IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            QuerySqlGeneratorFactory = querySqlGeneratorFactory;
            TypeMappingSource = typeMappingSource;
            CommandBuilderFactory = commandBuilderFactory;
        }

        /// <summary>
        ///     The query SQL generator factory.
        /// </summary>
        public IQuerySqlGeneratorFactory QuerySqlGeneratorFactory { get; }

        /// <summary>
        ///     Gets the type mapper.
        /// </summary>
        public IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     Gets the type mapper.
        /// </summary>
        public IRelationalCommandBuilderFactory CommandBuilderFactory { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="querySqlGeneratorFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public SelectExpressionDependencies With([NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory)
            => new SelectExpressionDependencies(querySqlGeneratorFactory, TypeMappingSource, CommandBuilderFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="typeMappingSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public SelectExpressionDependencies With([NotNull] IRelationalTypeMappingSource typeMappingSource)
            => new SelectExpressionDependencies(QuerySqlGeneratorFactory, typeMappingSource, CommandBuilderFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="commandLogger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public SelectExpressionDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
            => new SelectExpressionDependencies(QuerySqlGeneratorFactory, TypeMappingSource, CommandBuilderFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="queryLogger"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public SelectExpressionDependencies With([NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> queryLogger)
            => new SelectExpressionDependencies(QuerySqlGeneratorFactory, TypeMappingSource, CommandBuilderFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="commandBuilderFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public SelectExpressionDependencies With([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
            => new SelectExpressionDependencies(QuerySqlGeneratorFactory, TypeMappingSource, commandBuilderFactory);
    }
}
