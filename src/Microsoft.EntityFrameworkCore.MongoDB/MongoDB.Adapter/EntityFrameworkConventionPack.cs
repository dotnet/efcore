using Microsoft.EntityFrameworkCore.Annotations;
using MongoDB.Bson.Serialization.Conventions;

namespace Microsoft.EntityFrameworkCore.MongoDB.Adapter
{
    public class EntityFrameworkConventionPack : ConventionPack
    {
        public static EntityFrameworkConventionPack Instance { get; } = new EntityFrameworkConventionPack();

        private EntityFrameworkConventionPack()
        {
            AddRange(new IConvention[]
            {
                new AbstractClassMapConvention(),
                new BsonClassMapAttributeConvention<DerivedTypeAttribute>(),
                new IgnoreEmptyEnumerablesConvention(),
                new IgnoreNullOrEmptyStringsConvention(),
                new KeyAttributeConvention()
            });
        }
    }
}