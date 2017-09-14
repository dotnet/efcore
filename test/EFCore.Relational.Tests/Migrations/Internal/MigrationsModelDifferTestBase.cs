// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public abstract class MigrationsModelDifferTestBase
    {
        protected void Execute(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<IReadOnlyList<MigrationOperation>> assertAction)
            => Execute(m => { }, buildSourceAction, buildTargetAction, assertAction, null);

        protected void Execute(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<IReadOnlyList<MigrationOperation>> assertAction)
            => Execute(buildCommonAction, buildSourceAction, buildTargetAction, assertAction, null);

        protected void Execute(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<IReadOnlyList<MigrationOperation>> assertActionUp,
            Action<IReadOnlyList<MigrationOperation>> assertActionDown)
        {
            var sourceModelBuilder = CreateModelBuilder();
            buildCommonAction(sourceModelBuilder);
            buildSourceAction(sourceModelBuilder);

            var targetModelBuilder = CreateModelBuilder();
            buildCommonAction(targetModelBuilder);
            buildTargetAction(targetModelBuilder);

            var ctx = RelationalTestHelpers.Instance.CreateContext(targetModelBuilder.Model);
            var modelDiffer = CreateModelDiffer(ctx);

            var operationsUp = modelDiffer.GetDifferences(sourceModelBuilder.Model, targetModelBuilder.Model);
            assertActionUp(operationsUp);

            if (assertActionDown != null)
            {
                ctx = RelationalTestHelpers.Instance.CreateContext(sourceModelBuilder.Model);
                modelDiffer = CreateModelDiffer(ctx);

                var operationsDown = modelDiffer.GetDifferences(targetModelBuilder.Model, sourceModelBuilder.Model);
                assertActionDown(operationsDown);
            }
        }

        protected abstract ModelBuilder CreateModelBuilder();

        protected virtual MigrationsModelDiffer CreateModelDiffer(DbContext ctx)
            => new MigrationsModelDiffer(
                new ConcreteTypeMapper(new RelationalTypeMapperDependencies()),
                new MigrationsAnnotationProvider(new MigrationsAnnotationProviderDependencies()),
                ctx.GetService<IStateManager>(),
                ctx.GetService<ICommandBatchPreparer>());

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            public ConcreteTypeMapper(RelationalTypeMapperDependencies dependencies)
                : base(dependencies)
            {
            }

            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

            public override RelationalTypeMapping FindMapping(Type clrType)
                => clrType == typeof(string)
                    ? new StringTypeMapping("varchar(4000)", dbType: null, unicode: false, size: 4000)
                    : base.FindMapping(clrType);

            protected override RelationalTypeMapping FindCustomMapping(IProperty property)
                => property.ClrType == typeof(string) && (property.GetMaxLength().HasValue || property.IsUnicode().HasValue)
                    ? new StringTypeMapping(((property.IsUnicode() ?? true) ? "n" : "") + "varchar(" + (property.GetMaxLength() ?? 767) + ")", dbType: null, unicode: false, size: property.GetMaxLength())
                    : base.FindCustomMapping(property);

            private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), new IntTypeMapping("int") },
                    { typeof(bool), new BoolTypeMapping("boolean") }
                };

            private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>
                {
                    { "varchar", new StringTypeMapping("varchar", dbType: null, unicode: false, size: null) },
                    { "bigint", new LongTypeMapping("bigint") }
                };

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
                => _simpleMappings;

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
                => _simpleNameMappings;
        }
    }
}
