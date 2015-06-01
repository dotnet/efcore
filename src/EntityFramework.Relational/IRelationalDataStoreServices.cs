// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public interface IRelationalDataStoreServices : IDataStoreServices
    {
        IModelDiffer ModelDiffer { get; }
        IHistoryRepository HistoryRepository { get; }
        IMigrationSqlGenerator MigrationSqlGenerator { get; }
        IRelationalConnection RelationalConnection { get; }
        IRelationalTypeMapper TypeMapper { get; }
        ISqlGenerator SqlGenerator { get; }
        IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        ICommandBatchPreparer CommandBatchPreparer { get; }
        IBatchExecutor BatchExecutor { get; }
        IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }
        IRelationalDataStoreCreator RelationalDataStoreCreator { get; }
        IRelationalMetadataExtensionProvider MetadataExtensionProvider { get; }
        ISqlStatementExecutor SqlStatementExecutor { get; }
    }
}
