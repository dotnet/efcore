// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

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

            var modelDiffer = CreateModelDiffer(targetModelBuilder.Model);

            var operationsUp = modelDiffer.GetDifferences(sourceModelBuilder.Model, targetModelBuilder.Model);
            assertActionUp(operationsUp);

            if (assertActionDown != null)
            {
                modelDiffer = CreateModelDiffer(sourceModelBuilder.Model);

                var operationsDown = modelDiffer.GetDifferences(targetModelBuilder.Model, sourceModelBuilder.Model);
                assertActionDown(operationsDown);
            }
        }

        protected void AssertMultidimensionalArray<T>(T[,] values, params Action<T>[] assertions)
        {
            Assert.Collection(ToOnedimensionalArray(values), assertions);
        }

        protected static T[] ToOnedimensionalArray<T>(T[,] values, bool firstDimension = false)
        {
            Debug.Assert(
                values.GetLength(firstDimension ? 1 : 0) == 1,
                $"Length of dimension {(firstDimension ? 1 : 0)} is not 1.");

            var result = new T[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                result[i] = firstDimension
                    ? values[i, 0]
                    : values[0, i];
            }

            return result;
        }

        protected static T[][] ToJaggedArray<T>(T[,] twoDimensionalArray, bool firstDimension = false)
        {
            var rowsFirstIndex = twoDimensionalArray.GetLowerBound(0);
            var rowsLastIndex = twoDimensionalArray.GetUpperBound(0);
            var numberOfRows = rowsLastIndex - rowsFirstIndex + 1;

            var columnsFirstIndex = twoDimensionalArray.GetLowerBound(1);
            var columnsLastIndex = twoDimensionalArray.GetUpperBound(1);
            var numberOfColumns = columnsLastIndex - columnsFirstIndex + 1;

            var jaggedArray = new T[numberOfRows][];
            for (var i = 0; i < numberOfRows; i++)
            {
                jaggedArray[i] = new T[numberOfColumns];

                for (var j = 0; j < numberOfColumns; j++)
                {
                    jaggedArray[i][j] = twoDimensionalArray[i + rowsFirstIndex, j + columnsFirstIndex];
                }
            }
            return jaggedArray;
        }

        protected abstract ModelBuilder CreateModelBuilder();

        protected virtual MigrationsModelDiffer CreateModelDiffer(IModel model)
        {
            var ctx = RelationalTestHelpers.Instance.CreateContext(model);
            return new MigrationsModelDiffer(
                new FallbackRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<ConcreteTypeMapper>()),
                new MigrationsAnnotationProvider(
                    new MigrationsAnnotationProviderDependencies()),
                ctx.GetService<IChangeDetector>(),
                ctx.GetService<StateManagerDependencies>(),
                ctx.GetService<CommandBatchPreparerDependencies>());
        }

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            public ConcreteTypeMapper(
                RelationalTypeMapperDependencies dependencies)
                : base(dependencies)
            {
            }

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
                    { typeof(short), new ShortTypeMapping("smallint") },
                    { typeof(long), new LongTypeMapping("bigint") },
                    { typeof(bool), new BoolTypeMapping("boolean") },
                    { typeof(DateTime), new DateTimeTypeMapping("datetime2") }
                };

            private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>
                {
                    { "varchar", new StringTypeMapping("varchar") },
                    { "bigint", new LongTypeMapping("bigint") }
                };

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
                => _simpleMappings;

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
                => _simpleNameMappings;
        }
    }
}
