// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

public abstract class CSharpMigrationsGeneratorTestBase
{
    protected virtual ICollection<BuildReference> GetReferences()
        => new List<BuildReference>
        {
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Design.Tests")
        };

    protected abstract TestHelpers TestHelpers { get; }

    protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        => Test(buildModel, expectedCode, (m, _) => assert(m));

    protected void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel, IModel> assert, bool validate = false)
    {
        var modelBuilder = CreateConventionalModelBuilder();
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

        var builder = CreateConventionalModelBuilder();
        builder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

        buildModelMethod.Invoke(
            Activator.CreateInstance(snapshotType),
            [builder]);

        var services = TestHelpers.CreateContextServices();
        var processor = new SnapshotModelProcessor(new TestOperationReporter(), services.GetService<IModelRuntimeInitializer>());
        return processor.Process(builder.Model);
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