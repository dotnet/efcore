namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

public class SnapshotModelProcessor
{
    public virtual IModel? Process(IReadOnlyModel? model, bool resetVersion = false)
    {
        if (model == null
            || model is not Model mutableModel
            || mutableModel.IsReadOnly)
        {
            return null;
        }

        var version = model.GetProductVersion();
        if (version != null)
        {
            ProcessElement(model, version);
            foreach (var entityType in model.GetEntityTypes())
            {
                ProcessElement(entityType, version);
                ProcessComplexProperties(entityType, version);
            }
        }

        if (resetVersion)
        {
            mutableModel.SetProductVersion(ProductInfo.GetVersion());
        }

        return _modelRuntimeInitializer.Initialize((IModel)model, designTime: true, validationLogger: null);
    }

    private void ProcessComplexProperties(IReadOnlyTypeBase typeBase, string version)
    {
        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            ProcessElement(complexProperty, version);

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