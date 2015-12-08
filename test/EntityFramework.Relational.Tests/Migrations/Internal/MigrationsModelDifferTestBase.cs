// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class MigrationsModelDifferTestBase
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
        protected virtual MigrationsModelDiffer CreateModelDiffer()
            => new MigrationsModelDiffer(
                new ConcreteTypeMapper(),
                new TestAnnotationProvider(),
                new MigrationsAnnotationProvider());

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            public ConcreteTypeMapper()
            {
                _simpleMappings
                    = new Dictionary<Type, RelationalTypeMapping>
                        {
                            { typeof(int), new RelationalTypeMapping("int", typeof(int)) },
                            { typeof(bool), new RelationalTypeMapping("boolean", typeof(bool)) }
                        };

                _simpleNameMappings
                    = new Dictionary<string, RelationalTypeMapping>
                        {
                            { "varchar", new RelationalTypeMapping("varchar", typeof(string)) },
                            { "bigint", new RelationalTypeMapping("bigint", typeof(long)) }
                        };
            }

            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

            public override RelationalTypeMapping FindMapping([NotNull] Type clrType)
                => clrType == typeof(string)
                    ? new RelationalTypeMapping("varchar(4000)", typeof(string))
                    : base.FindMapping(clrType);

            protected override RelationalTypeMapping FindCustomMapping([NotNull] IProperty property)
                => property.ClrType == typeof(string) && property.GetMaxLength().HasValue
                    ? new RelationalTypeMapping("varchar(" + property.GetMaxLength() + ")", typeof(string))
                    : base.FindCustomMapping(property);
        }
    }
}
