using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    [ComplexType]
    public class ComplexType : IEquatable<ComplexType>
    {
        public string StringProperty { get; set; } = Guid.NewGuid().ToString(format: "B");

        public int IntProperty { get; set; } = new Random().Next(minValue: 0, maxValue: 10000);

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object obj)
            => Equals(obj as ComplexType);

        public bool Equals(ComplexType other)
            => StringProperty == other?.StringProperty &&
               IntProperty == other?.IntProperty;
    }
}