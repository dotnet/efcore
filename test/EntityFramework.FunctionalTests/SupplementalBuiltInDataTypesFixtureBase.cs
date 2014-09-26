// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    /// <summary>
    ///     See also <see cref="BuiltInDataTypesFixtureBase" />.
    ///     Not all built-in data types are supported on all providers yet.
    ///     At the same time, not all conventions (e.g. Ignore) are available yet.
    ///     So this class provides a base fixture for those data types which are
    ///     only supported on some providers.
    ///     Over time, the aim is to transfer as many data types as possible into
    ///     BuiltInDataTypesFixtureBase and ultimately to delete this class.
    /// </summary>
    public abstract class SupplementalBuiltInDataTypesFixtureBase
    {
        public abstract DbContext CreateContext();

        public virtual IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<SupplementalBuiltInNonNullableDataTypes>(b =>
                {
                    b.Key(dt => dt.Id0);
                    // having 2 non-null properies is needed for Azure Table Storage (and should be supported by all other providers)
                    b.Property(dt => dt.Id0);
                    b.Property(dt => dt.Id1);
                    b.Property(dt => dt.TestUnsignedInt32);
                    b.Property(dt => dt.TestUnsignedInt64);
                    b.Property(dt => dt.TestUnsignedInt16);
                    b.Property(dt => dt.TestCharacter);
                    b.Property(dt => dt.TestSignedByte);
                });

            builder.Entity<SupplementalBuiltInNullableDataTypes>(b =>
                {
                    b.Key(dt => dt.Id0);
                    // having 2 non-null properies is needed for Azure Table Storage (and should be supported by all other providers)
                    b.Property(dt => dt.Id0);
                    b.Property(dt => dt.Id1);
                    b.Property(dt => dt.TestNullableUnsignedInt32);
                    b.Property(dt => dt.TestNullableUnsignedInt64);
                    b.Property(dt => dt.TestNullableInt16);
                    b.Property(dt => dt.TestNullableUnsignedInt16);
                    b.Property(dt => dt.TestNullableCharacter);
                    b.Property(dt => dt.TestNullableSignedByte);
                });

            return model;
        }
    }

    public class SupplementalBuiltInNonNullableDataTypes
    {
        public int Id0 { get; set; }
        public int Id1 { get; set; }
        public uint TestUnsignedInt32 { get; set; }
        public ulong TestUnsignedInt64 { get; set; }
        public ushort TestUnsignedInt16 { get; set; }
        public char TestCharacter { get; set; }
        public sbyte TestSignedByte { get; set; }
    }

    public class SupplementalBuiltInNullableDataTypes
    {
        public int Id0 { get; set; }
        public int Id1 { get; set; }
        public uint? TestNullableUnsignedInt32 { get; set; }
        public ulong? TestNullableUnsignedInt64 { get; set; }
        public short? TestNullableInt16 { get; set; }
        public ushort? TestNullableUnsignedInt16 { get; set; }
        public char? TestNullableCharacter { get; set; }
        public sbyte? TestNullableSignedByte { get; set; }
    }
}
