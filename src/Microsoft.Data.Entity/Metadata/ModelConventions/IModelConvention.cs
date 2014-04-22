using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public interface IModelConvention
    {
        void Apply([NotNull] EntityType entityType);
    }
}
