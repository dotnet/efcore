// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The primary services needed to interact with a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalDatabaseProviderServices : IDatabaseProviderServices
    {
        /// <summary>
        ///     The <see cref="IMigrationsAnnotationProvider" /> for the provider.
        /// </summary>
        IMigrationsAnnotationProvider MigrationsAnnotationProvider { get; }

        /// <summary>
        ///     The <see cref="IHistoryRepository" /> for the provider.
        /// </summary>
        IHistoryRepository HistoryRepository { get; }

        /// <summary>
        ///     The <see cref="IMigrationsSqlGenerator" /> for the provider.
        /// </summary>
        IMigrationsSqlGenerator MigrationsSqlGenerator { get; }

        /// <summary>
        ///     The <see cref="IRelationalConnection" /> for the provider.
        /// </summary>
        IRelationalConnection RelationalConnection { get; }

        /// <summary>
        ///     The <see cref="IRelationalTypeMapper" /> for the provider.
        /// </summary>
        IRelationalTypeMapper TypeMapper { get; }

        /// <summary>
        ///     The <see cref="IUpdateSqlGenerator" /> for the provider.
        /// </summary>
        IUpdateSqlGenerator UpdateSqlGenerator { get; }

        /// <summary>
        ///     The <see cref="IModificationCommandBatchFactory" /> for the provider.
        /// </summary>
        IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }

        /// <summary>
        ///     The <see cref="ICommandBatchPreparer" /> for the provider.
        /// </summary>
        ICommandBatchPreparer CommandBatchPreparer { get; }

        /// <summary>
        ///     The <see cref="IBatchExecutor" /> for the provider.
        /// </summary>
        IBatchExecutor BatchExecutor { get; }

        /// <summary>
        ///     The <see cref="IRelationalValueBufferFactoryFactory" /> for the provider.
        /// </summary>
        IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        /// <summary>
        ///     The <see cref="IRelationalDatabaseCreator" /> for the provider.
        /// </summary>
        IRelationalDatabaseCreator RelationalDatabaseCreator { get; }

        /// <summary>
        ///     The <see cref="IRelationalAnnotationProvider" /> for the provider.
        /// </summary>
        IRelationalAnnotationProvider AnnotationProvider { get; }

        /// <summary>
        ///     The <see cref="IMethodCallTranslator" /> for the provider.
        /// </summary>
        IMethodCallTranslator CompositeMethodCallTranslator { get; }

        /// <summary>
        ///     The <see cref="IMemberTranslator" /> for the provider.
        /// </summary>
        IMemberTranslator CompositeMemberTranslator { get; }

        /// <summary>
        ///     The <see cref="IExpressionFragmentTranslator" /> for the provider.
        /// </summary>
        IExpressionFragmentTranslator CompositeExpressionFragmentTranslator { get; }

        /// <summary>
        ///     The <see cref="IParameterNameGeneratorFactory" /> for the provider.
        /// </summary>
        IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }

        /// <summary>
        ///     The <see cref="ISqlGenerationHelper" /> for the provider.
        /// </summary>
        ISqlGenerationHelper SqlGenerationHelper { get; }

        /// <summary>
        ///     The <see cref="IQuerySqlGeneratorFactory" /> for the provider.
        /// </summary>
        IQuerySqlGeneratorFactory QuerySqlGeneratorFactory { get; }

        /// <summary>
        ///     The <see cref="ISqlTranslatingExpressionVisitorFactory" /> for the provider.
        /// </summary>
        ISqlTranslatingExpressionVisitorFactory SqlTranslatingExpressionVisitorFactory { get; }
    }
}
