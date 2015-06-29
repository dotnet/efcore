// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public abstract class RelationalEntityServicesBuilderExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        [Fact]
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            VerifySingleton<IParameterNameGeneratorFactory>();
            VerifySingleton<IComparer<ModificationCommand>>();
            VerifySingleton<IMigrationIdGenerator>();
            VerifySingleton<SqlStatementExecutor>();
            VerifySingleton<UntypedValueBufferFactoryFactory>();
            VerifySingleton<TypedValueBufferFactoryFactory>();
            VerifySingleton<IMigrationModelFactory>();
            VerifySingleton<RelationalModelValidator>();

            VerifyScoped<IMigrator>();
            VerifyScoped<IMigrationAssembly>();
            VerifyScoped<RelationalQueryContextFactory>();
            VerifyScoped<BatchExecutor>();
            VerifyScoped<ModelDiffer>();
            VerifyScoped<RelationalValueGeneratorSelector>();
            VerifyScoped<CommandBatchPreparer>();

            VerifyScoped<ISqlStatementExecutor>();
            VerifyScoped<IMethodCallTranslator>();
            VerifyScoped<IMemberTranslator>();
            VerifyScoped<IModelDiffer>();
            VerifyScoped<IHistoryRepository>();
            VerifyScoped<IMigrationSqlGenerator>();
            VerifyScoped<IRelationalConnection>();
            VerifyScoped<IRelationalTypeMapper>();
            VerifyScoped<IModificationCommandBatchFactory>();
            VerifyScoped<ICommandBatchPreparer>();
            VerifyScoped<IRelationalValueBufferFactoryFactory>();
            VerifyScoped<IRelationalDatabaseCreator>();
            VerifyScoped<IUpdateSqlGenerator>();
            VerifyScoped<IRelationalMetadataExtensionProvider>();
        }
    }
}
