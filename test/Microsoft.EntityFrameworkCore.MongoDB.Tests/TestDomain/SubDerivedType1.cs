#if !(NET451 && DRIVER_NOT_SIGNED)
using System;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    public class SubDerivedType1 : SubRootType1, IEquatable<SubDerivedType1>
    {
        public SubDerivedType1()
        {
            var random = new Random();
            var data = new byte[random.Next(minValue: 25, maxValue: 50)];
            random.NextBytes(data);
            DataProperty = data;
        }

        public byte[] DataProperty { get; set; }

        public override bool Equals(SubRootType1 other)
            => Equals(other as SubDerivedType1);

        public bool Equals(SubDerivedType1 other)
        {
            bool retVal = base.Equals(other) &&
                          DataProperty?.Length == other?.DataProperty?.Length;
            for (int i = 0; retVal && i < DataProperty?.Length; i++)
            {
                retVal = DataProperty[i] == other.DataProperty[i];
            }
            return retVal;
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)