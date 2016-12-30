using System;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    public class SubDerivedType2 : SubRootType1, IEquatable<SubDerivedType2>
    {
        public char CharProperty { get; set; } = (char)new Random().Next(minValue: 'A', maxValue: 'z');

        public override bool Equals(SubRootType1 other)
            => Equals(other as SubDerivedType2);

        public bool Equals(SubDerivedType2 other)
            => base.Equals(other) &&
               CharProperty == other?.CharProperty;
    }
}