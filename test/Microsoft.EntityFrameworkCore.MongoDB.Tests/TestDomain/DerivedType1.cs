#if !(NET451 && DRIVER_NOT_SIGNED)
using System;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    public class DerivedType1 : RootType, IEquatable<DerivedType1>
    {
        public int IntProperty { get; set; } = new Random().Next();

        public override bool Equals(RootType other)
            => Equals(other as DerivedType1);

        public bool Equals(DerivedType1 other)
            => base.Equals(other) &&
               IntProperty == other?.IntProperty;
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)