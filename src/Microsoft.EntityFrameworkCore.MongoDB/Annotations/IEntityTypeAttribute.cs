using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Annotations
{
    public interface IEntityTypeAttribute
    {
        void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder);
    }
}