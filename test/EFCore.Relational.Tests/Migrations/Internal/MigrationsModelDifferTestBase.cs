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
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update;
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
            => Execute(buildCommonAction, buildSourceAction, buildTargetAction, assertActionUp, assertActionDown, null);

        protected void Execute(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<IReadOnlyList<MigrationOperation>> assertActionUp,
            Action<IReadOnlyList<MigrationOperation>> assertActionDown,
            Action<DbContextOptionsBuilder> builderOptionsAction)
        {
            var sourceModelBuilder = CreateModelBuilder();
            buildCommonAction(sourceModelBuilder);
            buildSourceAction(sourceModelBuilder);
            sourceModelBuilder.FinalizeModel();
            var sourceOptionsBuilder = TestHelpers
                .AddProviderOptions(new DbContextOptionsBuilder())
                .UseModel(sourceModelBuilder.Model)
                .EnableSensitiveDataLogging();

            var targetModelBuilder = CreateModelBuilder();
            buildCommonAction(targetModelBuilder);
            buildTargetAction(targetModelBuilder);
            targetModelBuilder.FinalizeModel();
            var targetOptionsBuilder = TestHelpers
                .AddProviderOptions(new DbContextOptionsBuilder())
                .UseModel(targetModelBuilder.Model)
                .EnableSensitiveDataLogging();

            if (builderOptionsAction != null)
            {
                builderOptionsAction(sourceOptionsBuilder);
                builderOptionsAction(targetOptionsBuilder);
            }

            var modelDiffer = CreateModelDiffer(targetOptionsBuilder.Options);

            var operationsUp = modelDiffer.GetDifferences(sourceModelBuilder.Model, targetModelBuilder.Model);
            assertActionUp(operationsUp);

            if (assertActionDown != null)
            {
                modelDiffer = CreateModelDiffer(sourceOptionsBuilder.Options);

                var operationsDown = modelDiffer.GetDifferences(targetModelBuilder.Model, sourceModelBuilder.Model);
                assertActionDown(operationsDown);
            }
        }

        protected void AssertMultidimensionalArray<T>(T[,] values, params Action<T>[] assertions)
            => Assert.Collection(ToOnedimensionalArray(values), assertions);

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

        protected abstract TestHelpers TestHelpers { get; }
        protected virtual ModelBuilder CreateModelBuilder() => TestHelpers.CreateConventionBuilder(skipValidation: true);

        protected virtual MigrationsModelDiffer CreateModelDiffer(DbContextOptions options)
        {
            var ctx = TestHelpers.CreateContext(options);
            return new MigrationsModelDiffer(
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                new MigrationsAnnotationProvider(
                    new MigrationsAnnotationProviderDependencies()),
                ctx.GetService<IChangeDetector>(),
                ctx.GetService<IUpdateAdapterFactory>(),
                ctx.GetService<CommandBatchPreparerDependencies>());
        }
    }
}
