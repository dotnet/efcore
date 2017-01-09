#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Annotations;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    [DerivedType(typeof(DerivedType1))]
    [DerivedType(typeof(SubRootType1))]
    public abstract class RootType : IEquatable<RootType>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ObjectId Id { get; private set; }

        public string StringProperty { get; set; } = Guid.NewGuid().ToString(format: "D");

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object obj)
            => Equals(obj as RootType);

        public virtual bool Equals(RootType other)
            => Id.Equals(other?.Id) &&
                   string.Equals(StringProperty, other?.StringProperty);

    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)