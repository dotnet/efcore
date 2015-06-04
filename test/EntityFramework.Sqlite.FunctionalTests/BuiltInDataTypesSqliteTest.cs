// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class BuiltInDataTypesSqliteTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqliteFixture>
    {
        public BuiltInDataTypesSqliteTest(BuiltInDataTypesSqliteFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(
                    new MappedDataTypes
                        {
                            Id = 66,
                            Int = 77,
                            Integer = 78L,
                            Real = 84.4,
                            SomeString = "don't",
                            Text = "G",
                            Blob = new byte[] { 86 }
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedDataTypes>().Single(e => e.Id == 66);

                Assert.Equal(77, entity.Int);
                Assert.Equal(78L, entity.Integer);
                Assert.Equal(84.4, entity.Real);
                Assert.Equal("don't", entity.SomeString);
                Assert.Equal("G", entity.Text);
                Assert.Equal(new byte[] { 86 }, entity.Blob);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                        {
                            Id = 69,
                            Int = 77,
                            Integer = 78L,
                            Real = 84.4,
                            SomeString = "don't",
                            Text = "G",
                            Blob = new byte[] { 86 }
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Id == 69);

                Assert.Equal(77, entity.Int);
                Assert.Equal(78L, entity.Integer);
                Assert.Equal(84.4, entity.Real);
                Assert.Equal("don't", entity.SomeString);
                Assert.Equal("G", entity.Text);
                Assert.Equal(new byte[] { 86 }, entity.Blob);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                        {
                            Id = 78
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Id == 78);

                Assert.Null(entity.Integer);
                Assert.Null(entity.Real);
                Assert.Null(entity.Text);
                Assert.Null(entity.SomeString);
                Assert.Null(entity.Blob);
                Assert.Null(entity.Int);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
        {
            // Size expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(
                    new MappedSizedDataTypes
                        {
                            Id = 77,
                            Nvarchar = "Into",
                            Binary = new byte[] { 10, 11, 12, 13 }
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedSizedDataTypes>().Single(e => e.Id == 77);

                Assert.Equal("Into", entity.Nvarchar);
                Assert.Equal(new byte[] { 10, 11, 12, 13 }, entity.Binary);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(
                    new MappedSizedDataTypes
                        {
                            Id = 78
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedSizedDataTypes>().Single(e => e.Id == 78);

                Assert.Null(entity.Nvarchar);
                Assert.Null(entity.Binary);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
        {
            // Scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(
                    new MappedScaledDataTypes
                        {
                            Id = 77,
                            Float = 83.3f,
                            Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                            Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                            Decimal = 101.1m
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77);

                Assert.Equal(83.3f, entity.Float);
                Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
                Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
                Assert.Equal(101.1m, entity.Decimal);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale()
        {
            // Precision and scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(
                    new MappedPrecisionAndScaledDataTypes
                        {
                            Id = 77,
                            Decimal = 101.1m
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 77);

                Assert.Equal(101.1m, entity.Decimal);
            }
        }
    }
}
