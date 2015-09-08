// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalDatabaseProviderServices : IDatabaseProviderServices
    {
        IMigrationAnnotationProvider MigrationAnnotationProvider { get; }
        IHistoryRepository HistoryRepository { get; }
        IMigrationSqlGenerator MigrationSqlGenerator { get; }
        IRelationalConnection RelationalConnection { get; }
        IRelationalTypeMapper TypeMapper { get; }
        IUpdateSqlGenerator UpdateSqlGenerator { get; }
        IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        ICommandBatchPreparer CommandBatchPreparer { get; }
        IBatchExecutor BatchExecutor { get; }
        IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }
        IRelationalDatabaseCreator RelationalDatabaseCreator { get; }
        IRelationalMetadataExtensionProvider MetadataExtensionProvider { get; }
        ISqlStatementExecutor SqlStatementExecutor { get; }
        IMethodCallTranslator CompositeMethodCallTranslator { get; }
        IMemberTranslator CompositeMemberTranslator { get; }
        IParameterNameGeneratorFactory ParameterNameGeneratorFactory { get; }
    }
}
