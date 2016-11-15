// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Relational.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public class TestRelationalDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public TestRelationalDatabaseProviderServices(IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IConventionSetBuilder ConventionSetBuilder => GetService<TestRelationalConventionSetBuilder>();
        public override IModelSource ModelSource => GetService<TestRelationalModelSource>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<TestRelationalValueGeneratorCache>();
        public override IRelationalAnnotationProvider AnnotationProvider => GetService<TestAnnotationProvider>();
        public override IRelationalTypeMapper TypeMapper => GetService<TestRelationalTypeMapper>();
        public override IQuerySqlGeneratorFactory QuerySqlGeneratorFactory => GetService<TestQuerySqlGeneratorFactory>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<TestRelationalCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<TestRelationalCompositeMemberTranslator>();
        public override IHistoryRepository HistoryRepository { get; }
        public override IRelationalConnection RelationalConnection => GetService<FakeRelationalConnection>();
        public override ISqlGenerationHelper SqlGenerationHelper => GetService<RelationalSqlGenerationHelper>();
        public override IUpdateSqlGenerator UpdateSqlGenerator { get; }
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory { get; }
        public override IRelationalDatabaseCreator RelationalDatabaseCreator { get; }
    }
}
