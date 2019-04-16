// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
namespace Microsoft.EntityFrameworkCore
{
    public class BuiltInDataTypesSqliteTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqliteTest.BuiltInDataTypesSqliteFixture>
    {
        public BuiltInDataTypesSqliteTest(BuiltInDataTypesSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public virtual void Can_insert_and_query_decimal()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 14,
                        TestNullableDecimal = 3m
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 14);

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 14 && e.TestNullableDecimal == 3m));
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(66));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Id == 66), 66);
            }
        }

        private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(78L, entity.Integer);
            Assert.Equal(84.4, entity.Real);
            Assert.Equal("don't", entity.SomeString);
            Assert.Equal("G", entity.Text);
            Assert.Equal(new byte[] { 86 }, entity.Blob);
        }

        private static MappedDataTypes CreateMappedDataTypes(int id)
            => new MappedDataTypes
            {
                Id = id,
                Int = 77,
                Integer = 78L,
                Real = 84.4,
                SomeString = "don't",
                Text = "G",
                Blob = new byte[] { 86 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(69));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 69), 69);
            }
        }

        private static void AssertMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(78L, entity.Integer);
            Assert.Equal(84.4, entity.Real);
            Assert.Equal("don't", entity.SomeString);
            Assert.Equal("G", entity.Text);
            Assert.Equal(new byte[] { 86 }, entity.Blob);
        }

        private static MappedNullableDataTypes CreateMappedNullableDataTypes(int id)
            => new MappedNullableDataTypes
            {
                Id = id,
                Int = 77,
                Integer = 78L,
                Real = 84.4,
                SomeString = "don't",
                Text = "G",
                Blob = new byte[] { 86 }
            };

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
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 78), 78);
            }
        }

        private static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Null(entity.Integer);
            Assert.Null(entity.Real);
            Assert.Null(entity.Text);
            Assert.Null(entity.SomeString);
            Assert.Null(entity.Blob);
            Assert.Null(entity.Int);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
        {
            // Size expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal("Into", entity.Nvarchar);
            Assert.Equal(new byte[] { 10, 11, 12, 13 }, entity.Binary);
        }

        private static MappedSizedDataTypes CreateMappedSizedDataTypes(int id)
            => new MappedSizedDataTypes
            {
                Id = id,
                Nvarchar = "Into",
                Binary = new byte[] { 10, 11, 12, 13 }
            };

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
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 78), 78);
            }
        }

        private static void AssertNullMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Null(entity.Nvarchar);
            Assert.Null(entity.Binary);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
        {
            // Scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypes(MappedScaledDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(83.3f, entity.Float);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(101.1m, entity.Decimal);
        }

        private static MappedScaledDataTypes CreateMappedScaledDataTypes(int id)
            => new MappedScaledDataTypes
            {
                Id = id,
                Float = 83.3f,
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Decimal = 101.1m
            };

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

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_Identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(66));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.AltId == 66), 66);
            }
        }

        private static void AssertMappedDataTypesWithIdentity(MappedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.AltId);
            Assert.Equal(78L, entity.Integer);
            Assert.Equal(84.4, entity.Real);
            Assert.Equal("don't", entity.SomeString);
            Assert.Equal("G", entity.Text);
            Assert.Equal(new byte[] { 86 }, entity.Blob);
        }

        private static MappedDataTypesWithIdentity CreateMappedDataTypesWithIdentity(int id)
            => new MappedDataTypesWithIdentity
            {
                AltId = id,
                Int = 77,
                Integer = 78L,
                Real = 84.4,
                SomeString = "don't",
                Text = "G",
                Blob = new byte[] { 86 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_Identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(69));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 69), 69);
            }
        }

        private static void AssertMappedNullableDataTypesWithIdentity(MappedNullableDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.AltId);
            Assert.Equal(78L, entity.Integer);
            Assert.Equal(84.4, entity.Real);
            Assert.Equal("don't", entity.SomeString);
            Assert.Equal("G", entity.Text);
            Assert.Equal(new byte[] { 86 }, entity.Blob);
        }

        private static MappedNullableDataTypesWithIdentity CreateMappedNullableDataTypesWithIdentity(int id)
            => new MappedNullableDataTypesWithIdentity
            {
                AltId = id,
                Int = 77,
                Integer = 78L,
                Real = 84.4,
                SomeString = "don't",
                Text = "G",
                Blob = new byte[] { 86 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_Identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(
                    new MappedNullableDataTypesWithIdentity
                    {
                        AltId = 78
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 78), 78);
            }
        }

        private static void AssertNullMappedNullableDataTypesWithIdentity(MappedNullableDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.AltId);
            Assert.Null(entity.Integer);
            Assert.Null(entity.Real);
            Assert.Null(entity.Text);
            Assert.Null(entity.SomeString);
            Assert.Null(entity.Blob);
            Assert.Null(entity.Int);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_Identity()
        {
            // Size expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 77), 77);
            }
        }

        private static void AssertMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.AltId);
            Assert.Equal("Into", entity.Nvarchar);
            Assert.Equal(new byte[] { 10, 11, 12, 13 }, entity.Binary);
        }

        private static MappedSizedDataTypesWithIdentity CreateMappedSizedDataTypesWithIdentity(int id)
            => new MappedSizedDataTypesWithIdentity
            {
                AltId = id,
                Nvarchar = "Into",
                Binary = new byte[] { 10, 11, 12, 13 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_Identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(
                    new MappedSizedDataTypesWithIdentity
                    {
                        AltId = 78
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 78), 78);
            }
        }

        private static void AssertNullMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.AltId);
            Assert.Null(entity.Nvarchar);
            Assert.Null(entity.Binary);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_Identity()
        {
            // Scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.AltId == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypesWithIdentity(MappedScaledDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.AltId);
            Assert.Equal(83.3f, entity.Float);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(101.1m, entity.Decimal);
        }

        private static MappedScaledDataTypesWithIdentity CreateMappedScaledDataTypesWithIdentity(int id)
            => new MappedScaledDataTypesWithIdentity
            {
                AltId = id,
                Float = 83.3f,
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Decimal = 101.1m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_Identity()
        {
            // Precision and scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    new MappedPrecisionAndScaledDataTypesWithIdentity
                    {
                        AltId = 77,
                        Decimal = 101.1m
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.AltId == 77);

                Assert.Equal(101.1m, entity.Decimal);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(166));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(167));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(168));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Id == 166), 166);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Id == 167), 167);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Id == 168), 168);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(169));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(170));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(171));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 169), 169);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 170), 170);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 171), 171);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Id = 278
                    });
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Id = 279
                    });
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Id = 280
                    });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 278), 278);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 279), 279);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Id == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_in_batch()
        {
            // Size expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(177));
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(178));
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 177), 177);
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 178), 178);
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(
                    new MappedSizedDataTypes
                    {
                        Id = 278
                    });
                context.Set<MappedSizedDataTypes>().Add(
                    new MappedSizedDataTypes
                    {
                        Id = 279
                    });
                context.Set<MappedSizedDataTypes>().Add(
                    new MappedSizedDataTypes
                    {
                        Id = 280
                    });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 278), 278);
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 279), 279);
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_in_batch()
        {
            // Scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(177));
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(178));
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 177), 177);
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 178), 178);
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_in_batch()
        {
            // Precision and scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(
                    new MappedPrecisionAndScaledDataTypes
                    {
                        Id = 177,
                        Decimal = 101.1m
                    });
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(
                    new MappedPrecisionAndScaledDataTypes
                    {
                        Id = 178,
                        Decimal = 101.1m
                    });
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(
                    new MappedPrecisionAndScaledDataTypes
                    {
                        Id = 179,
                        Decimal = 101.1m
                    });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 177);

                Assert.Equal(101.1m, entity.Decimal);

                entity = context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 178);

                Assert.Equal(101.1m, entity.Decimal);

                entity = context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 179);

                Assert.Equal(101.1m, entity.Decimal);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_Identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(166));
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(167));
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(168));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.AltId == 166), 166);
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.AltId == 167), 167);
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.AltId == 168), 168);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_Identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(169));
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(170));
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(171));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 169), 169);
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 170), 170);
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 171), 171);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_Identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(
                    new MappedNullableDataTypesWithIdentity
                    {
                        AltId = 278
                    });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(
                    new MappedNullableDataTypesWithIdentity
                    {
                        AltId = 279
                    });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(
                    new MappedNullableDataTypesWithIdentity
                    {
                        AltId = 280
                    });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 278), 278);
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 279), 279);
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.AltId == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_Identity_in_batch()
        {
            // Size expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(177));
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(178));
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 177), 177);
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 178), 178);
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_Identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(
                    new MappedSizedDataTypesWithIdentity
                    {
                        AltId = 278
                    });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(
                    new MappedSizedDataTypesWithIdentity
                    {
                        AltId = 279
                    });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(
                    new MappedSizedDataTypesWithIdentity
                    {
                        AltId = 280
                    });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 278), 278);
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 279), 279);
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.AltId == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_Identity_in_batch()
        {
            // Scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(177));
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(178));
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.AltId == 177), 177);
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.AltId == 178), 178);
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.AltId == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_Identity_in_batch()
        {
            // Precision and scale expected to be ignored, but everything should still work

            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    new MappedPrecisionAndScaledDataTypesWithIdentity
                    {
                        AltId = 177,
                        Decimal = 101.1m
                    });
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    new MappedPrecisionAndScaledDataTypesWithIdentity
                    {
                        AltId = 178,
                        Decimal = 101.1m
                    });
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    new MappedPrecisionAndScaledDataTypesWithIdentity
                    {
                        AltId = 179,
                        Decimal = 101.1m
                    });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.AltId == 177);
                Assert.Equal(101.1m, entity.Decimal);

                entity = context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.AltId == 178);
                Assert.Equal(101.1m, entity.Decimal);

                entity = context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.AltId == 179);
                Assert.Equal(101.1m, entity.Decimal);
            }
        }

        [Fact]
        public void Can_get_column_types_from_built_model()
        {
            using (var context = CreateContext())
            {
                var typeMapper = context.GetService<IRelationalTypeMappingSource>();

                foreach (var property in context.Model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
                {
                    var columnType = property.Relational().ColumnType;
                    Assert.NotNull(columnType);

                    if (property[RelationalAnnotationNames.ColumnType] == null)
                    {
                        Assert.Equal(
                            columnType.ToLowerInvariant(),
                            typeMapper.FindMapping(property).StoreType.ToLowerInvariant());
                    }
                }
            }
        }

#if !Test21
        [Fact]
        public virtual void Can_query_Min_of_converted_types()
        {
            using (var context = CreateContext())
            {
                var min = new BuiltInNullableDataTypes
                {
                    Id = 201,
                    PartitionId = 200,
                    TestNullableDecimal = 2,
                    TestNullableDateTime = new DateTime(2018, 2, 2, 0, 0, 0),
                    TestNullableDateTimeOffset = new DateTimeOffset(2018, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    TestNullableTimeSpan = TimeSpan.FromDays(2),
                    TestNullableUnsignedInt64 = 0,
                    TestNullableCharacter = 'A'
                };
                context.Add(min);

                var max = new BuiltInNullableDataTypes
                {
                    Id = 202,
                    PartitionId = 200,

                    TestNullableDecimal = 10,
                    TestNullableDateTime = new DateTime(2018, 10, 10, 0, 0, 0),
                    TestNullableDateTimeOffset = new DateTimeOffset(2018, 1, 1, 11, 0, 0, TimeSpan.FromHours(-2)),
                    TestNullableTimeSpan = TimeSpan.FromDays(10),
                    TestNullableUnsignedInt64 = ulong.MaxValue,
                    TestNullableCharacter = 'B'
                };
                context.Add(max);

                context.SaveChanges();

                var result = context.Set<BuiltInNullableDataTypes>()
                    .Where(e => e.PartitionId == 200)
                    .GroupBy(_ => true)
                    .Select(
                        g => new BuiltInNullableDataTypes
                        {
                            TestNullableDecimal = g.Min(e => e.TestNullableDecimal),
                            TestNullableDateTime = g.Min(e => e.TestNullableDateTime),
                            TestNullableDateTimeOffset = g.Min(e => e.TestNullableDateTimeOffset),
                            TestNullableTimeSpan = g.Min(e => e.TestNullableTimeSpan),
                            TestNullableUnsignedInt64 = g.Min(e => e.TestNullableUnsignedInt64),
                            TestNullableCharacter = g.Min(e => e.TestNullableCharacter)
                        })
                    .ToList()[0];

                Assert.Equal(min.TestNullableDecimal, result.TestNullableDecimal);
                Assert.Equal(min.TestNullableDateTime, result.TestNullableDateTime);
                Assert.Equal(min.TestNullableDateTimeOffset, result.TestNullableDateTimeOffset);
                Assert.Equal(min.TestNullableTimeSpan, result.TestNullableTimeSpan);
                Assert.Equal(min.TestNullableUnsignedInt64, result.TestNullableUnsignedInt64);
                Assert.Equal(min.TestNullableCharacter, result.TestNullableCharacter);
            }
        }

        [Fact]
        public virtual void Can_query_Max_of_converted_types()
        {
            using (var context = CreateContext())
            {
                var min = new BuiltInNullableDataTypes
                {
                    Id = 203,
                    PartitionId = 201,
                    TestNullableDecimal = 2,
                    TestNullableDateTime = new DateTime(2018, 2, 2, 0, 0, 0),
                    TestNullableDateTimeOffset = new DateTimeOffset(2018, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    TestNullableTimeSpan = TimeSpan.FromDays(2),
                    TestNullableUnsignedInt64 = 0,
                    TestNullableCharacter = 'A'
                };
                context.Add(min);

                var max = new BuiltInNullableDataTypes
                {
                    Id = 204,
                    PartitionId = 201,
                    TestNullableDecimal = 10,
                    TestNullableDateTime = new DateTime(2018, 10, 10, 0, 0, 0),
                    TestNullableDateTimeOffset = new DateTimeOffset(2018, 1, 1, 11, 0, 0, TimeSpan.FromHours(-2)),
                    TestNullableTimeSpan = TimeSpan.FromDays(10),
                    TestNullableUnsignedInt64 = ulong.MaxValue,
                    TestNullableCharacter = 'B'
                };
                context.Add(max);

                context.SaveChanges();

                var result = context.Set<BuiltInNullableDataTypes>()
                    .Where(e => e.PartitionId == 201)
                    .GroupBy(_ => true)
                    .Select(
                        g => new BuiltInNullableDataTypes
                        {
                            TestNullableDecimal = g.Max(e => e.TestNullableDecimal),
                            TestNullableDateTime = g.Max(e => e.TestNullableDateTime),
                            TestNullableDateTimeOffset = g.Max(e => e.TestNullableDateTimeOffset),
                            TestNullableTimeSpan = g.Max(e => e.TestNullableTimeSpan),
                            TestNullableUnsignedInt64 = g.Max(e => e.TestNullableUnsignedInt64),
                            TestNullableCharacter = g.Max(e => e.TestNullableCharacter)
                        })
                    .ToList()[0];

                Assert.Equal(max.TestNullableDecimal, result.TestNullableDecimal);
                Assert.Equal(max.TestNullableDateTime, result.TestNullableDateTime);
                Assert.Equal(max.TestNullableDateTimeOffset, result.TestNullableDateTimeOffset);
                Assert.Equal(max.TestNullableTimeSpan, result.TestNullableTimeSpan);
                Assert.Equal(max.TestNullableUnsignedInt64, result.TestNullableUnsignedInt64);
                Assert.Equal(max.TestNullableCharacter, result.TestNullableCharacter);
            }
        }

        [Fact]
        public virtual void Can_query_Average_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 205,
                        PartitionId = 202,
                        TestNullableDecimal = 1.000000000000001m
                    });

                context.Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 206,
                        PartitionId = 202,
                        TestNullableDecimal = 1.000000000000001m
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInNullableDataTypes>()
                    .Where(e => e.PartitionId == 202)
                    .Average(e => e.TestNullableDecimal);

                Assert.Equal(1.000000000000001m, result);
            }
        }

        [Fact]
        public virtual void Can_query_Sum_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 205,
                        PartitionId = 203,
                        TestDecimal = 1.000000000000001m
                    });

                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 206,
                        PartitionId = 203,
                        TestDecimal = 1.000000000000001m
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Where(e => e.PartitionId == 203)
                    .Sum(e => e.TestDecimal);

                Assert.Equal(2.000000000000002m, result);
            }
        }

        [Fact]
        public virtual void Can_query_negation_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 207,
                        PartitionId = 204,
                        TestDecimal = 1.000000000000001m,
                        TestTimeSpan = TimeSpan.FromMinutes(1)
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new BuiltInDataTypes
                        {
                            Id = e.Id,
                            TestDecimal = -e.TestDecimal,
                            TestTimeSpan = -e.TestTimeSpan
                        })
                    .First(e => e.Id == 207);

                Assert.Equal(-1.000000000000001m, result.TestDecimal);
                Assert.Equal(TimeSpan.FromMinutes(-1), result.TestTimeSpan);
            }
        }

        [Fact]
        public virtual void Can_query_add_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 208,
                        PartitionId = 204,
                        TestDecimal = 1.000000000000001m,
                        TestDateTime = new DateTime(2018, 1, 1, 0, 0, 0),
                        TestDateTimeOffset = new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        TestTimeSpan = TimeSpan.FromMinutes(1),
                        TestUnsignedInt64 = ulong.MaxValue - 1ul
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new BuiltInDataTypes
                        {
                            Id = e.Id,
                            TestDecimal = e.TestDecimal + 1m,
                            TestDateTime = e.TestDateTime + new TimeSpan(0, 1, 0),
                            TestDateTimeOffset = e.TestDateTimeOffset + new TimeSpan(0, 1, 0),
                            TestTimeSpan = e.TestTimeSpan + new TimeSpan(0, 1, 0),
                            TestUnsignedInt64 = e.TestUnsignedInt64 + 1ul
                        })
                    .First(e => e.Id == 208);

                Assert.Equal(2.000000000000001m, result.TestDecimal);
                Assert.Equal(new DateTime(2018, 1, 1, 0, 1, 0), result.TestDateTime);
                Assert.Equal(new DateTimeOffset(2018, 1, 1, 0, 1, 0, TimeSpan.Zero), result.TestDateTimeOffset);
                Assert.Equal(TimeSpan.FromMinutes(2), result.TestTimeSpan);
                Assert.Equal(ulong.MaxValue, result.TestUnsignedInt64);
            }
        }

        [Fact]
        public virtual void Can_query_subtract_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 209,
                        PartitionId = 204,
                        TestDecimal = 2.000000000000001m,
                        TestDateTime = new DateTime(2018, 1, 1, 0, 1, 0),
                        TestDateTimeOffset = new DateTimeOffset(2018, 1, 1, 0, 1, 0, TimeSpan.Zero),
                        TestTimeSpan = TimeSpan.FromMinutes(2),
                        TestUnsignedInt64 = ulong.MaxValue
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal - 1m,
                            TestDateTime1 = e.TestDateTime - new TimeSpan(0, 1, 0),
                            TestDateTime2 = e.TestDateTime - new DateTime(2018, 1, 1, 0, 0, 0),
                            TestDateTimeOffset1 = e.TestDateTimeOffset - new TimeSpan(0, 1, 0),
                            TestDateTimeOffset2 = e.TestDateTimeOffset - new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero),
                            TestTimeSpan = e.TestTimeSpan - new TimeSpan(0, 1, 0),
                            TestUnsignedInt64 = e.TestUnsignedInt64 - 1ul
                        })
                    .First(e => e.Id == 209);

                Assert.Equal(1.000000000000001m, result.TestDecimal);
                Assert.Equal(new DateTime(2018, 1, 1, 0, 0, 0), result.TestDateTime1);
                Assert.Equal(TimeSpan.FromMinutes(1), result.TestDateTime2);
                Assert.Equal(new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero), result.TestDateTimeOffset1);
                Assert.Equal(TimeSpan.FromMinutes(1), result.TestDateTimeOffset2);
                Assert.Equal(TimeSpan.FromMinutes(1), result.TestTimeSpan);
                Assert.Equal(ulong.MaxValue - 1ul, result.TestUnsignedInt64);
            }
        }

        [Fact]
        public virtual void Can_query_less_than_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 210,
                        PartitionId = 204,
                        TestDecimal = 2,
                        TestDateTime = new DateTime(2018, 2, 2, 0, 0, 0),
                        TestDateTimeOffset = new DateTimeOffset(2018, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        TestTimeSpan = TimeSpan.FromDays(2),
                        TestUnsignedInt64 = 0,
                        TestCharacter = 'A'
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal < 10m,
                            TestDateTime = e.TestDateTime < new DateTime(2018, 10, 10, 0, 0, 0),
                            TestDateTimeOffset = e.TestDateTimeOffset < new DateTimeOffset(2018, 1, 1, 11, 0, 0, TimeSpan.FromHours(-2)),
                            TestTimeSpan = e.TestTimeSpan < new TimeSpan(10, 0, 0, 0),
                            TestUnsignedInt64 = e.TestUnsignedInt64 < ulong.MaxValue,
                            TestCharacter = e.TestCharacter < 'B'
                        })
                    .First(e => e.Id == 210);

                Assert.True(result.TestDecimal);
                Assert.True(result.TestDateTime);
                Assert.True(result.TestDateTimeOffset);
                Assert.True(result.TestTimeSpan);
                Assert.True(result.TestUnsignedInt64);
                Assert.True(result.TestCharacter);
            }
        }

        [Fact]
        public virtual void Can_query_less_than_or_equal_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 211,
                        PartitionId = 204,
                        TestDecimal = 2,
                        TestDateTime = new DateTime(2018, 2, 2, 0, 0, 0),
                        TestDateTimeOffset = new DateTimeOffset(2018, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        TestTimeSpan = TimeSpan.FromDays(2),
                        TestUnsignedInt64 = 0,
                        TestCharacter = 'A'
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal <= 10m,
                            TestDateTime = e.TestDateTime <= new DateTime(2018, 10, 10, 0, 0, 0),
                            TestDateTimeOffset = e.TestDateTimeOffset <= new DateTimeOffset(2018, 1, 1, 11, 0, 0, TimeSpan.FromHours(-2)),
                            TestTimeSpan = e.TestTimeSpan <= new TimeSpan(10, 0, 0, 0),
                            TestUnsignedInt64 = e.TestUnsignedInt64 <= ulong.MaxValue,
                            TestCharacter = e.TestCharacter <= 'B'
                        })
                    .First(e => e.Id == 211);

                Assert.True(result.TestDecimal);
                Assert.True(result.TestDateTime);
                Assert.True(result.TestDateTimeOffset);
                Assert.True(result.TestTimeSpan);
                Assert.True(result.TestUnsignedInt64);
                Assert.True(result.TestCharacter);
            }
        }

        [Fact]
        public virtual void Can_query_greater_than_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 212,
                        PartitionId = 204,
                        TestDecimal = 2,
                        TestDateTime = new DateTime(2018, 2, 2, 0, 0, 0),
                        TestDateTimeOffset = new DateTimeOffset(2018, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        TestTimeSpan = TimeSpan.FromDays(2),
                        TestUnsignedInt64 = 0,
                        TestCharacter = 'A'
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal > 10m,
                            TestDateTime = e.TestDateTime > new DateTime(2018, 10, 10, 0, 0, 0),
                            TestDateTimeOffset = e.TestDateTimeOffset > new DateTimeOffset(2018, 1, 1, 11, 0, 0, TimeSpan.FromHours(-2)),
                            TestTimeSpan = e.TestTimeSpan > new TimeSpan(10, 0, 0, 0),
                            TestUnsignedInt64 = e.TestUnsignedInt64 > ulong.MaxValue,
                            TestCharacter = e.TestCharacter > 'B'
                        })
                    .First(e => e.Id == 212);

                Assert.False(result.TestDecimal);
                Assert.False(result.TestDateTime);
                Assert.False(result.TestDateTimeOffset);
                Assert.False(result.TestTimeSpan);
                Assert.False(result.TestUnsignedInt64);
                Assert.False(result.TestCharacter);
            }
        }

        [Fact]
        public virtual void Can_query_greater_than_or_equal_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 213,
                        PartitionId = 204,
                        TestDecimal = 2,
                        TestDateTime = new DateTime(2018, 2, 2, 0, 0, 0),
                        TestDateTimeOffset = new DateTimeOffset(2018, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        TestTimeSpan = TimeSpan.FromDays(2),
                        TestUnsignedInt64 = 0,
                        TestCharacter = 'A'
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal >= 10m,
                            TestDateTime = e.TestDateTime >= new DateTime(2018, 10, 10, 0, 0, 0),
                            TestDateTimeOffset = e.TestDateTimeOffset >= new DateTimeOffset(2018, 1, 1, 11, 0, 0, TimeSpan.FromHours(-2)),
                            TestTimeSpan = e.TestTimeSpan >= new TimeSpan(10, 0, 0, 0),
                            TestUnsignedInt64 = e.TestUnsignedInt64 >= ulong.MaxValue,
                            TestCharacter = e.TestCharacter >= 'B'
                        })
                    .First(e => e.Id == 213);

                Assert.False(result.TestDecimal);
                Assert.False(result.TestDateTime);
                Assert.False(result.TestDateTimeOffset);
                Assert.False(result.TestTimeSpan);
                Assert.False(result.TestUnsignedInt64);
                Assert.False(result.TestCharacter);
            }
        }

        [Fact]
        public virtual void Can_query_divide_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 214,
                        PartitionId = 204,
                        TestDecimal = 2.000000000000002m,
                        TestTimeSpan = TimeSpan.FromMinutes(2),
                        TestUnsignedInt64 = ulong.MaxValue
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal / 2m,
#if !NET461
                            TestTimeSpan1 = e.TestTimeSpan / 2.0,
                            TestTimeSpan2 = e.TestTimeSpan / new TimeSpan(0, 2, 0),
#endif
                            TestUnsignedInt64 = e.TestUnsignedInt64 / 5ul
                        })
                    .First(e => e.Id == 214);

                Assert.Equal(1.000000000000001m, result.TestDecimal);
#if !NET461
                Assert.Equal(TimeSpan.FromMinutes(1), result.TestTimeSpan1);
                Assert.Equal(1.0, result.TestTimeSpan2);
#endif
                Assert.Equal(ulong.MaxValue / 5, result.TestUnsignedInt64);
            }
        }

        [Fact]
        public virtual void Can_query_multiply_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 215,
                        PartitionId = 204,
                        TestDecimal = 1.000000000000001m,
                        TestTimeSpan = TimeSpan.FromMinutes(1),
                        TestUnsignedInt64 = ulong.MaxValue / 5
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new
                        {
                            e.Id,
                            TestDecimal = e.TestDecimal * 2m,
#if !NET461
                            TestTimeSpan1 = e.TestTimeSpan * 2.0,
                            TestTimeSpan2 = 2.0 * e.TestTimeSpan,
#endif
                            TestUnsignedInt64 = e.TestUnsignedInt64 * 5ul
                        })
                    .First(e => e.Id == 215);

                Assert.Equal(2.000000000000002m, result.TestDecimal);
#if !NET461
                Assert.Equal(TimeSpan.FromMinutes(2), result.TestTimeSpan1);
                Assert.Equal(TimeSpan.FromMinutes(2), result.TestTimeSpan2);
#endif
                Assert.Equal(ulong.MaxValue, result.TestUnsignedInt64);
            }
        }

        [Fact]
        public virtual void Can_query_modulo_of_converted_types()
        {
            using (var context = CreateContext())
            {
                context.Add(
                    new BuiltInDataTypes
                    {
                        Id = 216,
                        PartitionId = 204,
                        TestDecimal = 3.000000000000003m,
                        TestUnsignedInt64 = 10000000000000000001
                    });

                context.SaveChanges();

                var result = context.Set<BuiltInDataTypes>()
                    .Select(
                        e => new BuiltInDataTypes
                        {
                            Id = e.Id,
                            TestDecimal = e.TestDecimal % 2.000000000000002m,
                            TestUnsignedInt64 = e.TestUnsignedInt64 % 10000000000000000000
                        })
                    .First(e => e.Id == 216);

                Assert.Equal(1.000000000000001m, result.TestDecimal);
                Assert.Equal(1ul, result.TestUnsignedInt64);
            }
        }
#endif

        public class BuiltInDataTypesSqliteFixture : BuiltInDataTypesFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Integer).HasColumnType("Integer");
                        b.Property(e => e.Real).HasColumnType("Real");
                        b.Property(e => e.Text).HasColumnType("Text").IsRequired();
                        b.Property(e => e.Blob).HasColumnType("Blob").IsRequired();
                        b.Property(e => e.SomeString).HasColumnType("SomeString").IsRequired();
                        b.Property(e => e.Int).HasColumnType("Int");
                    });

                modelBuilder.Entity<MappedNullableDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Integer).HasColumnType("Integer");
                        b.Property(e => e.Real).HasColumnType("Real");
                        b.Property(e => e.Text).HasColumnType("Text");
                        b.Property(e => e.Blob).HasColumnType("Blob");
                        b.Property(e => e.SomeString).HasColumnType("SomeString");
                        b.Property(e => e.Int).HasColumnType("Int");
                    });

                modelBuilder.Entity<MappedSizedDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Nvarchar).HasColumnType("nvarchar(3)");
                        b.Property(e => e.Binary).HasColumnType("varbinary(3)");
                    });

                modelBuilder.Entity<MappedScaledDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Float).HasColumnType("real(3)");
                        b.Property(e => e.Datetimeoffset).HasColumnType("datetimeoffset(3)");
                        b.Property(e => e.Datetime2).HasColumnType("datetime2(3)");
                        b.Property(e => e.Decimal).HasColumnType("decimal(3)");
                    });

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Decimal).HasColumnType("decimal(5, 2)");
                    });

                modelBuilder.Entity<MappedDataTypesWithIdentity>(
                    b =>
                    {
                        b.Property(e => e.Integer).HasColumnType("Integer");
                        b.Property(e => e.Real).HasColumnType("Real");
                        b.Property(e => e.Text).HasColumnType("Text").IsRequired();
                        b.Property(e => e.Blob).HasColumnType("Blob").IsRequired();
                        b.Property(e => e.SomeString).HasColumnType("SomeString").IsRequired();
                        b.Property(e => e.Int).HasColumnType("Int");
                    });

                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>(
                    b =>
                    {
                        b.Property(e => e.Integer).HasColumnType("Integer");
                        b.Property(e => e.Real).HasColumnType("Real");
                        b.Property(e => e.Text).HasColumnType("Text");
                        b.Property(e => e.Blob).HasColumnType("Blob");
                        b.Property(e => e.SomeString).HasColumnType("SomeString");
                        b.Property(e => e.Int).HasColumnType("Int");
                    });

                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>(
                    b =>
                    {
                        b.Property(e => e.Nvarchar).HasColumnType("nvarchar(3)");
                        b.Property(e => e.Binary).HasColumnType("varbinary(3)");
                    });

                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>(
                    b =>
                    {
                        b.Property(e => e.Float).HasColumnType("real(3)");
                        b.Property(e => e.Datetimeoffset).HasColumnType("datetimeoffset(3)");
                        b.Property(e => e.Datetime2).HasColumnType("datetime2(3)");
                        b.Property(e => e.Decimal).HasColumnType("decimal(3)");
                    });

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>(b => b.Property(e => e.Decimal).HasColumnType("decimal(5, 2)"));
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c.Log(RelationalEventId.QueryClientEvaluationWarning)
                        .Log(RelationalEventId.ValueConversionSqlLiteralWarning));

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }

        protected class MappedDataTypes
        {
            public int Id { get; set; }
            public long Integer { get; set; }
            public double Real { get; set; }
            public string Text { get; set; }
            public byte[] Blob { get; set; }
            public string SomeString { get; set; }
            public int Int { get; set; }
        }

        protected class MappedSizedDataTypes
        {
            public int Id { get; set; }
            public string Nvarchar { get; set; }
            public byte[] Binary { get; set; }
        }

        protected class MappedScaledDataTypes
        {
            public int Id { get; set; }
            public float Float { get; set; }
            public DateTimeOffset Datetimeoffset { get; set; }
            public DateTime Datetime2 { get; set; }
            public decimal Decimal { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypes
        {
            public int Id { get; set; }
            public decimal Decimal { get; set; }
        }

        protected class MappedNullableDataTypes
        {
            public int Id { get; set; }
            public long? Integer { get; set; }
            public double? Real { get; set; }
            public string Text { get; set; }
            public byte[] Blob { get; set; }
            public string SomeString { get; set; }
            public int? Int { get; set; }
        }

        protected class MappedDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int AltId { get; set; }
            public long Integer { get; set; }
            public double Real { get; set; }
            public string Text { get; set; }
            public byte[] Blob { get; set; }
            public string SomeString { get; set; }
            public int Int { get; set; }
        }

        protected class MappedSizedDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int AltId { get; set; }
            public string Nvarchar { get; set; }
            public byte[] Binary { get; set; }
        }

        protected class MappedScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int AltId { get; set; }
            public float Float { get; set; }
            public DateTimeOffset Datetimeoffset { get; set; }
            public DateTime Datetime2 { get; set; }
            public decimal Decimal { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int AltId { get; set; }
            public decimal Decimal { get; set; }
        }

        protected class MappedNullableDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int AltId { get; set; }
            public long? Integer { get; set; }
            public double? Real { get; set; }
            public string Text { get; set; }
            public byte[] Blob { get; set; }
            public string SomeString { get; set; }
            public int? Int { get; set; }
        }
    }
}
