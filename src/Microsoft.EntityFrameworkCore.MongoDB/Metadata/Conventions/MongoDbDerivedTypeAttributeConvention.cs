using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbDerivedTypeAttributeConvention : EntityTypeAttributeConvention<DerivedTypeAttribute>
    {
        public override InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] DerivedTypeAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(attribute, nameof(attribute));
            attribute.Apply(entityTypeBuilder);
            return entityTypeBuilder;
        }
    }
}