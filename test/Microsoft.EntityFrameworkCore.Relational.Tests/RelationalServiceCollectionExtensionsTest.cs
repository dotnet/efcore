// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Relational.Tests
{
    public abstract class RelationalServiceCollectionExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            VerifySingleton<IParameterNameGeneratorFactory>();
            VerifySingleton<IComparer<ModificationCommand>>();
            VerifySingleton<IMigrationsIdGenerator>();
            VerifySingleton<UntypedRelationalValueBufferFactoryFactory>();
            VerifySingleton<TypedRelationalValueBufferFactoryFactory>();

            VerifyScoped<IMigrator>();
            VerifyScoped<IMigrationsAssembly>();
            VerifyScoped<RelationalDatabase>();
            VerifyScoped<RelationalQueryContextFactory>();
            VerifyScoped<BatchExecutor>();
            VerifyScoped<MigrationsModelDiffer>();
            VerifyScoped<RelationalValueGeneratorSelector>();
            VerifyScoped<CommandBatchPreparer>();
            VerifyScoped<RelationalModelValidator>();

            VerifyScoped<IMethodCallTranslator>();
            VerifyScoped<IMemberTranslator>();
            VerifyScoped<IExpressionFragmentTranslator>();
            VerifyScoped<IMigrationsModelDiffer>();
            VerifyScoped<IHistoryRepository>();
            VerifyScoped<IMigrationsSqlGenerator>();
            VerifyScoped<IRelationalConnection>();
            VerifyScoped<IRelationalTypeMapper>();
            VerifyScoped<IModificationCommandBatchFactory>();
            VerifyScoped<ICommandBatchPreparer>();
            VerifyScoped<IRelationalValueBufferFactoryFactory>();
            VerifyScoped<IRelationalDatabaseCreator>();
            VerifyScoped<IUpdateSqlGenerator>();
            VerifyScoped<IRelationalAnnotationProvider>();
            VerifyScoped<MigrationsSqlGenerator>();
        }

        protected RelationalServiceCollectionExtensionsTest(RelationalTestHelpers testHelpers)
            : base(testHelpers)
        {
        }
    }
}
