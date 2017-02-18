using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class ObjectIdValueGenerator : ValueGenerator<ObjectId>
    {
        public ObjectIdValueGenerator(bool generatesTemporaryValue = false)
        {
            GeneratesTemporaryValues = generatesTemporaryValue;
        }

        public override ObjectId Next(EntityEntry entry)
        => ObjectId.GenerateNewId();

        public override bool GeneratesTemporaryValues { get; }
    }
}