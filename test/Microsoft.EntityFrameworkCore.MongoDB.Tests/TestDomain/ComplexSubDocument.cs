using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    [ComplexType]
    public class ComplexSubDocument : IEquatable<ComplexSubDocument>
    {
        public ComplexType ComplexValue { get; set; } = new ComplexType();

        public IList<ComplexType> ComplexValueList { get; set; } = new List<ComplexType>
        {
            new ComplexType(),
            new ComplexType(),
            new ComplexType()
        };

        public override int GetHashCode()
            => base.GetHashCode();

        public override bool Equals(object obj)
            => Equals(obj as ComplexSubDocument);

        public bool Equals(ComplexSubDocument other)
            => Equals(ComplexValue, other?.ComplexValue) &&
               ComplexValueList?.Count == other?.ComplexValueList?.Count &&
               (ComplexValueList?.All(complexValue => other?.ComplexValueList?.Contains(complexValue) ?? false) ?? false);
    }
}