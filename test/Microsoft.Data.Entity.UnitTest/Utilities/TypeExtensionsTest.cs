// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class TypeExtensionsTest
    {
        [Fact]
        public void ElementTypeShouldReturnElementTypeFromSequenceType()
        {
            Assert.Equal(typeof(string), typeof(IEnumerable<string>).ElementType());
            Assert.Equal(typeof(string), typeof(IQueryable<string>).ElementType());
        }

        [Fact]
        public void ElementTypeShouldReturnInputTypeWhenNotSequenceType()
        {
            Assert.Equal(typeof(string), typeof(string));
        }

        [Fact]
        public void GetAnyProperty_returns_any_property()
        {
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ElDiabloEnElOjo").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ANightIn").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("MySister").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TinyTears").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Seaweed").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("VertrauenII").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TalkToMe").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("NoMoreAffairs").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Singing").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("TravellingLight").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("CherryBlossoms").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("ShesGone").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("Mistakes").DeclaringType);
            Assert.Null(typeof(TindersticksII).GetAnyProperty("VertrauenIII"));
            Assert.Same(typeof(TindersticksII), typeof(TindersticksII).GetAnyProperty("SleepySong").DeclaringType);

            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ElDiabloEnElOjo").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ANightIn").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("MySister").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TinyTears").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("Seaweed").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("VertrauenII").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TalkToMe").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("NoMoreAffairs").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("Singing").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("TravellingLight").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("CherryBlossoms").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("ShesGone").DeclaringType);
            Assert.Same(typeof(TindersticksII), typeof(TindersticksIIVinyl).GetAnyProperty("Mistakes").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIIVinyl).GetAnyProperty("VertrauenIII").DeclaringType);
            Assert.Throws<AmbiguousMatchException>(() => typeof(TindersticksIICd).GetAnyProperty("SleepySong"));

            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("ANightIn").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("MySister").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TinyTears").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("SnowyInFSharpMinor").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Seaweed").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("VertrauenII").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TalkToMe").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("NoMoreAffairs").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Singing").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("TravellingLight").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("CherryBlossoms").DeclaringType);
            Assert.Same(typeof(TindersticksIIVinyl), typeof(TindersticksIICd).GetAnyProperty("ShesGone").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("Mistakes").DeclaringType);
            Assert.Same(typeof(TindersticksIICd), typeof(TindersticksIICd).GetAnyProperty("VertrauenIII").DeclaringType);
            Assert.Throws<AmbiguousMatchException>(() => typeof(TindersticksIICd).GetAnyProperty("SleepySong"));
        }

        public class TindersticksII
        {
            public virtual int ElDiabloEnElOjo { get; set; }
            internal virtual int ANightIn { get; set; }
            private int MySister { get; set; }
            protected virtual int TinyTears { get; set; }
            public virtual int SnowyInFSharpMinor { get; private set; }
            public virtual int Seaweed { private get; set; }
            public virtual int VertrauenII { get; protected set; }
            public virtual int TalkToMe { protected get; set; }

            public virtual int NoMoreAffairs
            {
                get { return 1995; }
            }

            public virtual int Singing
            {
                set { }
            }

            public virtual int TravellingLight { get; set; }
            public int CherryBlossoms { get; set; }
            public int ShesGone { get; set; }
            public virtual int Mistakes { get; set; }
            public int SleepySong { get; set; }
        }

        public class TindersticksIIVinyl : TindersticksII
        {
            public override int ElDiabloEnElOjo { get; set; }
            internal override int ANightIn { get; set; }
            private int MySister { get; set; }
            protected override int TinyTears { get; set; }

            public override int SnowyInFSharpMinor
            {
                get { return 1995; }
            }

            public override int Seaweed
            {
                set { }
            }

            public override int VertrauenII { get; protected set; }
            public override int TalkToMe { protected get; set; }

            public override int NoMoreAffairs
            {
                get { return 1995; }
            }

            public override int Singing
            {
                set { }
            }

            public new virtual int TravellingLight { get; set; }
            public new virtual int CherryBlossoms { get; set; }
            public new int ShesGone { get; set; }
            public virtual int VertrauenIII { get; set; }
            public new static int SleepySong { get; set; }
        }

        public class TindersticksIICd : TindersticksIIVinyl
        {
            internal override int ANightIn { get; set; }
            private int MySister { get; set; }
            protected override int TinyTears { get; set; }

            public override int SnowyInFSharpMinor
            {
                get { return 1995; }
            }

            public override int Seaweed
            {
                set { }
            }

            public override int VertrauenII { get; protected set; }
            public override int TalkToMe { protected get; set; }

            public override int NoMoreAffairs
            {
                get { return 1995; }
            }

            public override int Singing
            {
                set { }
            }

            public override int TravellingLight { get; set; }
            public override int CherryBlossoms { get; set; }
            public override int Mistakes { get; set; }
            public override int VertrauenIII { get; set; }
            public new static int SleepySong { get; set; }
        }
    }
}
