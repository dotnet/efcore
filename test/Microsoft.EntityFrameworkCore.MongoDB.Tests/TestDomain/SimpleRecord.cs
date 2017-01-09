#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    public class SimpleRecord : IEquatable<SimpleRecord>
    {
        public SimpleRecord() { }

        public SimpleRecord(ObjectId id)
        {
            Id = id;
        }

        [Key]
        public ObjectId Id { get; private set; }

        public string StringProperty { get; set; } = Guid.NewGuid().ToString(format: "B");

        public int IntProperty { get; set; } = new Random().Next(minValue: 0, maxValue: 10000);

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object obj)
            => Equals(obj as SimpleRecord);

        public bool Equals(SimpleRecord other)
            => Id.Equals(other?.Id) &&
                   string.Equals(StringProperty, other?.StringProperty) &&
                   IntProperty == other?.IntProperty;
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)