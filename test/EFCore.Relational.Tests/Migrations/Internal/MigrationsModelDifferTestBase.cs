// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

public abstract class MigrationsModelDifferTestBase
{
    protected void Execute(
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertAction,
        bool skipSourceConventions = false)
        => Execute(m => { }, buildSourceAction, buildTargetAction, assertAction, null, skipSourceConventions);

    protected void Execute(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertAction,
        bool skipSourceConventions = false)
        => Execute(buildCommonAction, buildSourceAction, buildTargetAction, assertAction, null, skipSourceConventions);

    protected void Execute(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertActionUp,
        Action<IReadOnlyList<MigrationOperation>> assertActionDown,
        bool skipSourceConventions = false)
        => Execute(
            buildCommonAction, buildSourceAction, buildTargetAction, assertActionUp, assertActionDown, null, skipSourceConventions);

    protected void Execute(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertActionUp,
        Action<IReadOnlyList<MigrationOperation>> assertActionDown,
        Action<DbContextOptionsBuilder> builderOptionsAction,
        bool skipSourceConventions = false,
        bool enableSensitiveLogging = true)
    {
        var sourceModelBuilder = CreateModelBuilder(skipSourceConventions);
        buildCommonAction(sourceModelBuilder);
        buildSourceAction(sourceModelBuilder);

        var targetModelBuilder = CreateModelBuilder(skipConventions: false);
        buildCommonAction(targetModelBuilder);
        buildTargetAction(targetModelBuilder);

        var sourceModel = sourceModelBuilder.FinalizeModel(designTime: true, skipValidation: true);
        var targetModel = targetModelBuilder.FinalizeModel(designTime: true, skipValidation: true);

        var targetOptionsBuilder = TestHelpers
            .AddProviderOptions(new DbContextOptionsBuilder())
            .UseModel(targetModel);

        if (enableSensitiveLogging)
        {
            targetOptionsBuilder = targetOptionsBuilder.EnableSensitiveDataLogging();
        }

        if (builderOptionsAction != null)
        {
            builderOptionsAction(targetOptionsBuilder);
        }

        var modelDiffer = CreateModelDiffer(targetOptionsBuilder.Options);

        var operationsUp = modelDiffer.GetDifferences(sourceModel.GetRelationalModel(), targetModel.GetRelationalModel());
        assertActionUp(operationsUp);

        if (assertActionDown != null)
        {
            var operationsDown = modelDiffer.GetDifferences(targetModel.GetRelationalModel(), sourceModel.GetRelationalModel());
            assertActionDown(operationsDown);
        }

        var noopOperations = modelDiffer.GetDifferences(sourceModel.GetRelationalModel(), sourceModel.GetRelationalModel());
        Assert.Empty(noopOperations);

        noopOperations = modelDiffer.GetDifferences(targetModel.GetRelationalModel(), targetModel.GetRelationalModel());
        Assert.Empty(noopOperations);
    }

    protected void AssertMultidimensionalArray<T>(T[,] values, params Action<T>[] assertions)
        => Assert.Collection(values.Cast<T>(), assertions);

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

    protected virtual TestHelpers.TestModelBuilder CreateModelBuilder(bool skipConventions)
        => TestHelpers.CreateConventionBuilder(configureConventions: skipConventions ? c => c.RemoveAllConventions() : null);

    protected virtual MigrationsModelDiffer CreateModelDiffer(DbContextOptions options)
        => new(
            new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
            new MigrationsAnnotationProvider(
                new MigrationsAnnotationProviderDependencies()),
            new RelationalAnnotationProvider(
                new RelationalAnnotationProviderDependencies()),
            TestServiceFactory.Instance.Create<IRowIdentityMapFactory>(),
            TestHelpers.CreateContext(options).GetService<CommandBatchPreparerDependencies>());
}
