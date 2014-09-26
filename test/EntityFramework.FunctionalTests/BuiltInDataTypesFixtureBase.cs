// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    /// <summary>
    ///     See also <see cref="SupplementalBuiltInDataTypesFixtureBase" />.
    ///     Not all built-in data types are supported on all providers yet.
    ///     At the same time, not all conventions (e.g. Ignore) are available yet.
    ///     So this class provides a base fixture for those data types which are
    ///     supported on all current providers.
    ///     Over time, the aim is to transfer as many data types as possible into
    ///     this class and ultimately to delete <see cref="SupplementalBuiltInDataTypesFixtureBase" />.
    /// </summary>
    public abstract class BuiltInDataTypesFixtureBase
    {
        public abstract DbContext CreateContext();

        public virtual IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<BuiltInNonNullableDataTypes>(b =>
                {
                    b.Key(dt => dt.Id0);
                    // having 2 non-null properies is needed for Azure Table Storage (and should be supported by all other providers)
                    b.Property(dt => dt.Id0);
                    b.Property(dt => dt.Id1);
                    b.Property(dt => dt.TestInt32);
                    b.Property(dt => dt.TestInt64);
                    b.Property(dt => dt.TestDouble);
                    b.Property(dt => dt.TestDecimal);
                    b.Property(dt => dt.TestDateTime);
                    b.Property(dt => dt.TestDateTimeOffset);
                    b.Property(dt => dt.TestSingle);
                    b.Property(dt => dt.TestBoolean);
                    b.Property(dt => dt.TestByte);
                    b.Property(dt => dt.TestInt16);
                });

            builder.Entity<BuiltInNullableDataTypes>(b =>
                {
                    b.Key(dt => dt.Id0);
                    // having 2 non-null properies is needed for Azure Table Storage (and should be supported by all other providers)
                    b.Property(dt => dt.Id0);
                    b.Property(dt => dt.Id1);
                    b.Property(dt => dt.TestNullableInt32);
                    b.Property(dt => dt.TestString);
                    b.Property(dt => dt.TestNullableInt64);
                    b.Property(dt => dt.TestNullableDouble);
                    b.Property(dt => dt.TestNullableDecimal);
                    b.Property(dt => dt.TestNullableDateTime);
                    b.Property(dt => dt.TestNullableDateTimeOffset);
                    b.Property(dt => dt.TestNullableSingle);
                    b.Property(dt => dt.TestNullableBoolean);
                    b.Property(dt => dt.TestNullableByte);
                    b.Property(dt => dt.TestNullableInt16);
                });

            return model;
        }
    }

    public class BuiltInNonNullableDataTypes
    {
        public int Id0 { get; set; }
        public int Id1 { get; set; }
        public int TestInt32 { get; set; }
        public long TestInt64 { get; set; }
        public double TestDouble { get; set; }
        public decimal TestDecimal { get; set; }
        public DateTime TestDateTime { get; set; }
        public DateTimeOffset TestDateTimeOffset { get; set; }
        public float TestSingle { get; set; }
        public bool TestBoolean { get; set; }
        public byte TestByte { get; set; }
        public uint TestUnsignedInt32 { get; set; }
        public ulong TestUnsignedInt64 { get; set; }
        public short TestInt16 { get; set; }

        public override bool Equals(object other)
        {
            var otherAsThisType = other as BuiltInNonNullableDataTypes;
            return otherAsThisType != null && Equals(this, otherAsThisType);
        }

        protected bool Equals(BuiltInNonNullableDataTypes other)
        {
            return Id0 == other.Id0 && Id1 == other.Id1;
        }

        public static bool operator ==(BuiltInNonNullableDataTypes left, BuiltInNonNullableDataTypes right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BuiltInNonNullableDataTypes left, BuiltInNonNullableDataTypes right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Id0.GetHashCode() * 397 ^ (Id1.GetHashCode());
            }
        }
    }

    public class BuiltInNullableDataTypes
    {
        public int Id0 { get; set; }
        public int Id1 { get; set; }
        public int? TestNullableInt32 { get; set; }
        public string TestString { get; set; }
        public long? TestNullableInt64 { get; set; }
        public double? TestNullableDouble { get; set; }
        public decimal? TestNullableDecimal { get; set; }
        public DateTime? TestNullableDateTime { get; set; }
        public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
        public float? TestNullableSingle { get; set; }
        public bool? TestNullableBoolean { get; set; }
        public byte? TestNullableByte { get; set; }
        public short? TestNullableInt16 { get; set; }

        public override bool Equals(object other)
        {
            var otherAsThisType = other as BuiltInNullableDataTypes;
            return otherAsThisType != null && Equals(otherAsThisType);
        }

        protected bool Equals(BuiltInNullableDataTypes other)
        {
            return Id0 == other.Id0 && Id1 == other.Id1;
        }

        public static bool operator ==(BuiltInNullableDataTypes left, BuiltInNullableDataTypes right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BuiltInNullableDataTypes left, BuiltInNullableDataTypes right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Id0.GetHashCode() * 397 ^ (Id1.GetHashCode());
            }
        }
    }
}
