using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

public class SnapshotModelProcessor : ISnapshotModelProcessor
{
    private readonly IModelRuntimeInitializer _modelRuntimeInitializer;

    public virtual IModel? Process(IReadOnlyModel? model, bool resetVersion = false)
    {
        if (model == null || model is not Model mutableModel || mutableModel.IsReadOnly)
        {
            return null;
        }

        var version = model.GetProductVersion();
        if (version != null)
        {
            ProcessElement(model, version);
            UpdateSequences(model, version);

            foreach (var entityType in model.GetEntityTypes())
            {
                ProcessElement(entityType, version);
                ProcessCollection(entityType.GetProperties(), version);
                ProcessCollection(entityType.GetKeys(), version);
                ProcessCollection(entityType.GetIndexes(), version);

                foreach (var element in entityType.GetForeignKeys())
                {
                    ProcessElement(element, version);
                    ProcessElement(element.DependentToPrincipal, version);
                    ProcessElement(element.PrincipalToDependent, version);
                }

                ProcessComplexProperties(entityType, version);
            }
        }

        mutableModel.RemoveAnnotation("ChangeDetector.SkipDetectChanges");
        if (resetVersion)
        {
            mutableModel.SetProductVersion(ProductInfo.GetVersion());
        }

        return _modelRuntimeInitializer.Initialize((IModel)model, designTime: true, validationLogger: null);
    }

    private void ProcessCollection(IEnumerable<IReadOnlyAnnotatable> metadata, string version)
    {
        foreach (var element in metadata)
        {
            ProcessElement(element, version);
        }
    }

    private void ProcessComplexProperties(IReadOnlyTypeBase typeBase, string version)
    {
        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            ProcessElement(complexProperty, version);
            ProcessComplexProperties(complexProperty.ComplexType, version);
        }
    }
}