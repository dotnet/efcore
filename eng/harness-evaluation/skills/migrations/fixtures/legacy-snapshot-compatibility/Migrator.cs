namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

public class Migrator
{
    private IModel? FinalizeModel(IModel? model)
    {
        if (model == null)
        {
            return null;
        }

        ProcessSnapshotModel(model);
        return _modelRuntimeInitializer.Initialize(model);
    }

    private static void ProcessSnapshotModel(IModel model)
    {
        var version = model.GetProductVersion();
        if (version != null
            && model is Model mutableModel
            && !mutableModel.IsReadOnly)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                ProcessComplexProperties(entityType, version);
            }
        }
    }

    private static void ProcessComplexProperties(IReadOnlyTypeBase typeBase, string version)
    {
        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            if (complexProperty is IMutableComplexProperty mutableComplexProperty)
            {
                UpdateComplexPropertyNullability(mutableComplexProperty, version);
            }

            ProcessComplexProperties(complexProperty.ComplexType, version);
        }
    }

    private static void UpdateComplexPropertyNullability(
        IMutableComplexProperty complexProperty,
        string version)
    {
        if (!IsPreEFCore10Version(version)
            || complexProperty is not ComplexProperty mutableComplexPropertyInternal)
        {
            return;
        }

        if (mutableComplexPropertyInternal.GetIsNullableConfigurationSource() == null
            && !complexProperty.ClrType.IsNullableType())
        {
            mutableComplexPropertyInternal.SetIsNullable(false, ConfigurationSource.Explicit);
        }
    }

    private static bool IsPreEFCore10Version(string version)
        => version.StartsWith("1.", StringComparison.Ordinal)
            || version.StartsWith("2.", StringComparison.Ordinal)
            || version.StartsWith("3.", StringComparison.Ordinal)
            || version.StartsWith("5.", StringComparison.Ordinal)
            || version.StartsWith("6.", StringComparison.Ordinal)
            || version.StartsWith("7.", StringComparison.Ordinal)
            || version.StartsWith("8.", StringComparison.Ordinal)
            || version.StartsWith("9.", StringComparison.Ordinal);
}