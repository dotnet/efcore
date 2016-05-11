// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Migrations.Internal
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
            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

            public override RelationalTypeMapping FindMapping(Type clrType)
                => clrType == typeof(string)
                    ? new RelationalTypeMapping("varchar(4000)", typeof(string), dbType: null, unicode: false, size: 4000)
                    : base.FindMapping(clrType);

            protected override RelationalTypeMapping FindCustomMapping(IProperty property)
                => property.ClrType == typeof(string) && property.GetMaxLength().HasValue
                    ? new RelationalTypeMapping("nvarchar(" + property.GetMaxLength() + ")", typeof(string), dbType: null, unicode: false, size: property.GetMaxLength())
                    : base.FindCustomMapping(property);

            private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), new RelationalTypeMapping("int", typeof(int)) },
                        { typeof(bool), new RelationalTypeMapping("boolean", typeof(bool)) }
                    };

            private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>
                    {
                        { "varchar", new RelationalTypeMapping("varchar", typeof(string), dbType: null, unicode: false, size: null, hasNonDefaultUnicode: true) },
                        { "bigint", new RelationalTypeMapping("bigint", typeof(long)) }
                    };

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
                => _simpleMappings;

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
                => _simpleNameMappings;
        }
    }
}
