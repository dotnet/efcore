using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Annotations
{
    public interface IModelAttribute
    {
        void Apply([NotNull] InternalModelBuilder modelBuilder);
    }
}