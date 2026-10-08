// Existing SnapshotModelProcessorTest harness excerpt.
private static void AssertSameSnapshot(Type snapshotType, DbContext context)
{
    var differ = context.GetService<IMigrationsModelDiffer>();
    var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType)!;
    var reporter = new TestOperationReporter();
    var modelRuntimeInitializer =
        SqlServerTestHelpers.Instance.CreateContextServices().GetRequiredService<IModelRuntimeInitializer>();

    var model = PreprocessModel(snapshot);
    model = new SnapshotModelProcessor(reporter, modelRuntimeInitializer).Process(model, resetVersion: true)!;
    var currentModel = context.GetService<IDesignTimeModel>().Model;

    var differences = differ.GetDifferences(
        model.GetRelationalModel(),
        currentModel.GetRelationalModel());

    Assert.Empty(differences);

    var generator = CSharpMigrationsGeneratorTest.CreateMigrationsCodeGenerator();
    var oldSnapshotCode = generator.GenerateSnapshot(
        "MyNamespace",
        context.GetType(),
        "MySnapshot",
        model);
    var newSnapshotCode = generator.GenerateSnapshot(
        "MyNamespace",
        context.GetType(),
        "MySnapshot",
        currentModel);

    Assert.Equal(newSnapshotCode, oldSnapshotCode);
}

private static IModel PreprocessModel(ModelSnapshot snapshot)
{
    var model = snapshot.Model;
    if (model.FindAnnotation(RelationalAnnotationNames.MaxIdentifierLength) == null)
    {
        ((Model)model)[RelationalAnnotationNames.MaxIdentifierLength] = 128;
    }

    return model;
}