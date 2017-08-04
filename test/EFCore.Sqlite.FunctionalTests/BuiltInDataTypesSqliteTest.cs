// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public virtual void Can_perform_query_with_ansi_strings()
        {
            Can_perform_query_with_ansi_strings_test(supportsAnsi: false);
        }

        [Fact]
        public virtual void Can_query_using_any_nullable_data_type_as_literal()
        {
            Can_query_using_any_nullable_data_type_as_literal_helper(strictEquality: false);
        }

        [Fact(Skip = "See issue #8205")]
        public virtual void Can_insert_and_query_decimal()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 13,
                        TestNullableDecimal = 3m
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 13);

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 13 && e.TestNullableDecimal == 3m));
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
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Id = 78 });

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
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 78 });

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
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { AltId = 78 });

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
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { AltId = 78 });

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
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Id = 278 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Id = 279 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Id = 280 });

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
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 278 });
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 279 });
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 280 });

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
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { AltId = 278 });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { AltId = 279 });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { AltId = 280 });

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
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { AltId = 278 });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { AltId = 279 });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { AltId = 280 });

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
                var typeMapper = context.GetService<IRelationalTypeMapper>();

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

        public class BuiltInDataTypesSqliteFixture : BuiltInDataTypesFixtureBase
        {
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text").IsRequired();
                    b.Property(e => e.Blob).HasColumnType("Blob").IsRequired();
                    b.Property(e => e.SomeString).HasColumnType("SomeString").IsRequired();
                    b.Property(e => e.Int).HasColumnType("Int");
                });

                modelBuilder.Entity<MappedNullableDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text");
                    b.Property(e => e.Blob).HasColumnType("Blob");
                    b.Property(e => e.SomeString).HasColumnType("SomeString");
                    b.Property(e => e.Int).HasColumnType("Int");
                });

                modelBuilder.Entity<MappedSizedDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Nvarchar).HasColumnType("nvarchar(3)");
                    b.Property(e => e.Binary).HasColumnType("varbinary(3)");
                });

                modelBuilder.Entity<MappedScaledDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Float).HasColumnType("real(3)");
                    b.Property(e => e.Datetimeoffset).HasColumnType("datetimeoffset(3)");
                    b.Property(e => e.Datetime2).HasColumnType("datetime2(3)");
                    b.Property(e => e.Decimal).HasColumnType("decimal(3)");
                });

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Decimal).HasColumnType("decimal(5, 2)");
                });

                modelBuilder.Entity<MappedDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text").IsRequired();
                    b.Property(e => e.Blob).HasColumnType("Blob").IsRequired();
                    b.Property(e => e.SomeString).HasColumnType("SomeString").IsRequired();
                    b.Property(e => e.Int).HasColumnType("Int");
                });

                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text");
                    b.Property(e => e.Blob).HasColumnType("Blob");
                    b.Property(e => e.SomeString).HasColumnType("SomeString");
                    b.Property(e => e.Int).HasColumnType("Int");
                });

                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Nvarchar).HasColumnType("nvarchar(3)");
                    b.Property(e => e.Binary).HasColumnType("varbinary(3)");
                });

                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Float).HasColumnType("real(3)");
                    b.Property(e => e.Datetimeoffset).HasColumnType("datetimeoffset(3)");
                    b.Property(e => e.Datetime2).HasColumnType("datetime2(3)");
                    b.Property(e => e.Decimal).HasColumnType("decimal(3)");
                });

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>(b => { b.Property(e => e.Decimal).HasColumnType("decimal(5, 2)"); });
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(c => c
                    .Log(RelationalEventId.QueryClientEvaluationWarning));

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
