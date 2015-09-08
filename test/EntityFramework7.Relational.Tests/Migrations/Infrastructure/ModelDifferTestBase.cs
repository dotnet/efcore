// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
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
        protected virtual ModelDiffer CreateModelDiffer()
            => new ModelDiffer(new ConcreteTypeMapper(), new TestMetadataExtensionProvider(), new MigrationAnnotationProvider());

        private class TestMetadataExtensionProvider : IRelationalMetadataExtensionProvider
        {
            public IRelationalEntityTypeAnnotations Extensions(IEntityType entityType) => entityType.Relational();
            public IRelationalForeignKeyAnnotations Extensions(IForeignKey foreignKey) => foreignKey.Relational();
            public IRelationalIndexAnnotations Extensions(IIndex index) => index.Relational();
            public IRelationalKeyAnnotations Extensions(IKey key) => key.Relational();
            public IRelationalModelAnnotations Extensions(IModel model) => model.Relational();
            public IRelationalPropertyAnnotations Extensions(IProperty property) => property.Relational();
        }

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), new RelationalTypeMapping("int") },
                        { typeof(long), new RelationalTypeMapping("bigint") },
                        { typeof(string), new RelationalTypeMapping("nvarchar(4000)") },
                        { typeof(byte[]), new RelationalTypeMapping("varbinary(8000)") }
                    };

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
                = new Dictionary<string, RelationalTypeMapping>();
        }
    }
}
