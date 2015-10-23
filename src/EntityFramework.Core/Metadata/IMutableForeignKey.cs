using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMutableForeignKey : IForeignKey, IMutableAnnotatable
    {
        new IReadOnlyList<IMutableProperty> Properties { get; }
        new IMutableKey PrincipalKey { get; }
        new IMutableEntityType DeclaringEntityType { get; }
        new IMutableEntityType PrincipalEntityType { get; }
        new IMutableNavigation DependentToPrincipal { get; [param: CanBeNull] set; }
        new IMutableNavigation PrincipalToDependent { get; [param: CanBeNull] set; }
        new bool? IsUnique { get; set; }
        new bool? IsRequired { get; set; }
        new DeleteBehavior? DeleteBehavior { get; set; }
    }
}