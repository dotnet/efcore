// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IRelationalDatabaseProviderServices : IDatabaseProviderServices
    {
        IMigrationsAnnotationProvider MigrationsAnnotationProvider { get; }
        IHistoryRepository HistoryRepository { get; }
        IMigrationsSqlGenerator MigrationsSqlGenerator { get; }
        IRelationalConnection RelationalConnection { get; }
        IRelationalTypeMapper TypeMapper { get; }
        IUpdateSqlGenerator UpdateSqlGenerator { get; }
        IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        ICommandBatchPreparer CommandBatchPreparer { get; }
        IBatchExecutor BatchExecutor { get; }
        IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }
        IRelationalDatabaseCreator RelationalDatabaseCreator { get; }
        IRelationalAnnotationProvider AnnotationProvider { get; }
        IMethodCallTranslator CompositeMethodCallTranslator { get; }
        IMemberTranslator CompositeMemberTranslator { get; }
        IExpressionFragmentTranslator CompositeExpressionFragmentTranslator { get; }
        IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }
        ISqlGenerationHelper SqlGenerationHelper { get; }
        IQuerySqlGeneratorFactory QuerySqlGeneratorFactory { get; }
    }
}
