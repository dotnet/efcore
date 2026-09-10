// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public abstract class CSharpMigrationsGeneratorTestBase
{
    protected abstract ICollection<BuildReference> GetReferences();

    protected abstract TestHelpers TestHelpers { get; }

    protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        => Test(buildModel, expectedCode, (m, _) => assert(m));

    protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel, IModel> assert, bool validate = false)
    {
        var modelBuilder = CreateConventionalModelBuilder();
        modelBuilder.HasDefaultSchema("DefaultSchema");
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
        buildModel(modelBuilder);

        var model = modelBuilder.FinalizeModel(designTime: true, skipValidation: !validate);

        Test(model, expectedCode, assert);
    }

    protected void Test(IModel model, string expectedCode, Action<IModel, IModel> assert)
    {
        var generator = CreateMigrationsGenerator();
        var code = generator.GenerateSnapshot("RootNamespace", typeof(DbContext), "Snapshot", model);

        var modelFromSnapshot = BuildModelFromSnapshotSource(code);
        assert(modelFromSnapshot, model);

        try
        {
            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);
        }
        catch (EqualException e)
        {
            throw new Exception(e.Message + Environment.NewLine + Environment.NewLine + "-- Actual code:" + Environment.NewLine + code);
        }

        var targetOptionsBuilder = TestHelpers
            .AddProviderOptions(new DbContextOptionsBuilder())
            .UseModel(model)
            .EnableSensitiveDataLogging();

        var modelDiffer = CreateModelDiffer(targetOptionsBuilder.Options);

        var noopOperations = modelDiffer.GetDifferences(modelFromSnapshot.GetRelationalModel(), model.GetRelationalModel());
        Assert.Empty(noopOperations);
    }

    protected abstract TestHelpers.TestModelBuilder CreateConventionalModelBuilder();

    protected abstract CSharpMigrationsGenerator CreateMigrationsGenerator();

    protected virtual IModel BuildModelFromSnapshotSource(string code)
    {
        var build = new BuildSource { Sources = { { "Snapshot.cs", code } } };

        foreach (var buildReference in GetReferences())
        {
            build.References.Add(buildReference);
        }

        var assembly = build.BuildInMemory();
        var snapshotType = assembly.GetType("RootNamespace.Snapshot");

        var buildModelMethod = snapshotType.GetMethod(
            "BuildModel",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(ModelBuilder)],
            null);

        var builder = new ModelBuilder();
        builder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

        buildModelMethod.Invoke(
            Activator.CreateInstance(snapshotType),
            [builder]);

        var services = TestHelpers.CreateContextServices(GetServices());
        var processor = new SnapshotModelProcessor(new TestOperationReporter(), services.GetService<IModelRuntimeInitializer>());
        return processor.Process(builder.Model);
    }
    protected virtual MigrationsModelDiffer CreateModelDiffer(DbContextOptions options)
        => (MigrationsModelDiffer)TestHelpers.CreateContext(options).GetService<IMigrationsModelDiffer>();

    protected virtual IServiceCollection GetServices()
        => new ServiceCollection();

    protected virtual ModelSnapshot CompileModelSnapshot(string code, string modelSnapshotTypeName, Type contextType)
    {
        var build = new BuildSource { Sources = { { "Snapshot.cs", code } } };

        foreach (var buildReference in GetReferences())
        {
            build.References.Add(buildReference);
        }

        var assembly = build.BuildInMemory();

        var snapshotType = assembly.GetType(modelSnapshotTypeName, throwOnError: true, ignoreCase: false);

        var contextTypeAttribute = snapshotType.GetCustomAttribute<DbContextAttribute>();
        Assert.NotNull(contextTypeAttribute);
        Assert.Equal(contextType, contextTypeAttribute.ContextType);

        return (ModelSnapshot)Activator.CreateInstance(snapshotType);
    }

    protected class EntityWithAutoincrement
    {
        public int Id { get; set; }
    }

    protected class EntityWithConverterPk
    {
        public long Id { get; set; }
    }
}
