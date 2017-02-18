using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class MongoDbValueGeneratorSelector : ValueGeneratorSelector
    {
        public MongoDbValueGeneratorSelector([NotNull] IValueGeneratorCache cache)
            : base(cache)
        {
        }

        public override ValueGenerator Create([NotNull] IProperty property,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType().UnwrapEnumType() == typeof(ObjectId)
                ? new ObjectIdValueGenerator(property.ValueGenerated != ValueGenerated.Never)
                : base.Create(property, entityType);
        }
    }
}