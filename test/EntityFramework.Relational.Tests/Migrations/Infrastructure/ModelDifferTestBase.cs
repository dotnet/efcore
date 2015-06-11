// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class ModelDifferTestBase
    {
        protected void Execute(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<IReadOnlyList<MigrationOperation>> assertAction)
        {
            var sourceModelBuilder = CreateModelBuilder();
            buildSourceAction(sourceModelBuilder);

            var targetModelBuilder = CreateModelBuilder();
            buildTargetAction(targetModelBuilder);

            var modelDiffer = CreateModelDiffer();

            var operations = modelDiffer.GetDifferences(sourceModelBuilder.Model, targetModelBuilder.Model);

            assertAction(operations);
        }

        protected virtual ModelBuilder CreateModelBuilder() => TestHelpers.Instance.CreateConventionBuilder();
        protected virtual ModelDiffer CreateModelDiffer() => new ModelDiffer(new ConcreteTypeMapper(), new TestMetadataExtensionProvider());

        private class TestMetadataExtensionProvider : IRelationalMetadataExtensionProvider
        {
            public IRelationalEntityTypeExtensions Extensions(IEntityType entityType) => entityType.Relational();
            public IRelationalForeignKeyExtensions Extensions(IForeignKey foreignKey) => foreignKey.Relational();
            public IRelationalIndexExtensions Extensions(IIndex index) => index.Relational();
            public IRelationalKeyExtensions Extensions(IKey key) => key.Relational();
            public IRelationalModelExtensions Extensions(IModel model) => model.Relational();
            public IRelationalPropertyExtensions Extensions(IProperty property) => property.Relational();
        }

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), new RelationalTypeMapping("int") },
                        { typeof(long), new RelationalTypeMapping("bigint") },
                        { typeof(string), new RelationalTypeMapping("nvarchar(4000)") }
                    };

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
                = new Dictionary<string, RelationalTypeMapping>();
        }
    }
}
