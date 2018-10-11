// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable UnusedParameter.Local
// ReSharper disable PossibleInvalidOperationException
namespace Microsoft.EntityFrameworkCore
{
    public class BuiltInDataTypesOracleTest : BuiltInDataTypesTestBase<BuiltInDataTypesOracleTest.BuiltInDataTypesOracleFixture>
    {
        public BuiltInDataTypesOracleTest(BuiltInDataTypesOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Can_perform_query_with_max_length()
        {
            var shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };
            var longString = new string('X', Fixture.LongStringLength);
            var longBinary = new byte[Fixture.LongStringLength];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                    {
                        Id = 799,
                        String3 = shortString,
                        ByteArray5 = shortBinary,
                        String9000 = longString,
                        ByteArray9000 = longBinary
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.String3 == shortString));
                Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.ByteArray5 == shortBinary));

                //Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.String9000 == longString));
                //Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.ByteArray9000 == longBinary));
            }
        }

        [Fact]
        public void Sql_translation_uses_type_mapping_when_constant()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.Time == new TimeSpan(0, 1, 2))
                        .Select(e => e.Int)
                        .ToList();

                Assert.Equal(0, results.Count);
                Assert.Equal(
                    @"SELECT ""e"".""Int""
FROM ""MappedNullableDataTypes"" ""e""
WHERE ""e"".""Time"" = INTERVAL '0 0:1:2.000' DAY TO SECOND",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Sql_translation_uses_type_mapping_when_parameter()
        {
            using (var context = CreateContext())
            {
                var timeSpan = new TimeSpan(2, 1, 0);

                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.Time == timeSpan)
                        .Select(e => e.Int)
                        .ToList();

                Assert.Equal(0, results.Count);
                Assert.Equal(
                    @":timeSpan_0='02:01:00' (DbType = Object)

SELECT ""e"".""Int""
FROM ""MappedNullableDataTypes"" ""e""
WHERE ""e"".""Time"" = :timeSpan_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 999,
                        Bigint = 78L,
                        Smallint = 79,
                        Tinyint = 80,
                        Bit = true,
                        Money = 81.1m,
                        //                        Smallmoney = 82.2m,
                        Float = 83.3,
                        Real = 84.4f,
                        Double_precision = 85.5,
                        //                        Date = new DateTime(1605, 1, 2, 10, 11, 12),
                        Datetimeoffset = new DateTimeOffset(new DateTime(), TimeSpan.Zero),
                        //                        Datetime2 = new DateTime(),
                        //                        Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                        Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                        Time = new TimeSpan(0, 11, 15, 12, 2),
                        //                        VarcharMax = "C",
                        Char_varyingMax = "Your",
                        Character_varyingMax = "strong",
                        //                        NvarcharMax = "don't",
                        National_char_varyingMax = "help",
                        National_character_varyingMax = "anyone!",
                        Text = "Gumball Rules!",
                        Ntext = "Gumball Rules OK!",
                        VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                        //Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                        Image = new byte[] { 97, 98, 99, 100 },
                        Decimal = 101.7m,
                        Dec = 102.8m,
                        Numeric = 103.9m
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999);

                long? param1 = 78L;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Bigint == param1));

                short? param2 = 79;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Smallint == param2));

                byte? param3 = 80;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Tinyint == param3));

                bool? param4 = true;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Bit == param4));

                decimal? param5 = 81.1m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Money == param5));

                //                decimal? param6 = 82.2m;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Smallmoney == param6));

                double? param7a = 83.3;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Float == param7a));

                float? param7b = 84.4f;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Real == param7b));

                double? param7c = 85.5;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Double_precision == param7c));

                //                DateTime? param8 = new DateTime(1605, 1, 2);
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Date == param8));

                DateTimeOffset? param9 = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Datetimeoffset == param9));

                //                DateTime? param10 = new DateTime();
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Datetime2 == param10));

                DateTime? param11 = new DateTime(2019, 1, 2, 14, 11, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Datetime == param11));

                //                DateTime? param12 = new DateTime(2018, 1, 2, 13, 11, 0);
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Smalldatetime == param12));

                TimeSpan? param13 = new TimeSpan(0, 11, 15, 12, 2);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Time == param13));

                //                var param19 = "C";
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.VarcharMax == param19));

                var param20 = "Your";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Char_varyingMax == param20));

                var param21 = "strong";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Character_varyingMax == param21));

                //                var param27 = "don't";
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.NvarcharMax == param27));

                var param28 = "help";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_char_varyingMax == param28));

                var param29 = "anyone!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_character_varyingMax == param29));

                var param35 = new byte[] { 89, 90, 91, 92 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.VarbinaryMax == param35));

                //                var param36 = new byte[] { 93, 94, 95, 96 };
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Binary_varyingMax == param36));

                decimal? param38 = 102m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Decimal == param38));

                decimal? param39 = 103m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Dec == param39));

                decimal? param40 = 104m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Numeric == param40));
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_types_with_nulls()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 911
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911);

                long? param1 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Bigint == param1));

                short? param2 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Smallint == param2));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)(int?)e.Smallint == param2));

                byte? param3 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Tinyint == param3));

                bool? param4 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Bit == param4));

                decimal? param5 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Money == param5));

                //                decimal? param6 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Smallmoney == param6));

                double? param7a = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Float == param7a));

                float? param7b = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Real == param7b));

                double? param7c = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Double_precision == param7c));

                //                DateTime? param8 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Date == param8));

                DateTimeOffset? param9 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Datetimeoffset == param9));

                //                DateTime? param10 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Datetime2 == param10));

                DateTime? param11 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Datetime == param11));

                //                DateTime? param12 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Smalldatetime == param12));

                TimeSpan? param13 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Time == param13));

                //                string param19 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.VarcharMax == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Char_varyingMax == param20));

                string param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Character_varyingMax == param21));

                //                string param27 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NvarcharMax == param27));

                string param28 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.National_char_varyingMax == param28));

                string param29 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.National_character_varyingMax == param29));

                string param30 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Text == param30));

                string param31 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Ntext == param31));

                byte[] param35 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.VarbinaryMax == param35));

                //                byte[] param36 = null;
                //                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Binary_varyingMax == param36));

                byte[] param37 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Image == param37));

                decimal? param38 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Decimal == param38));

                decimal? param39 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Dec == param39));

                decimal? param40 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Numeric == param40));
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            var entity = CreateMappedDataTypes(77);
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(entity);

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='77'
:p1='78'
:p2='1'
:p3='Your' (Nullable = false) (Size = 2000)
:p4='strong' (Nullable = false) (Size = 2000)
:p5='2015-01-02T10:11:12' (DbType = Date)
:p6='2019-01-02T14:11:12' (DbType = DateTime)
:p7='2016-01-02T11:11:12' (DbType = DateTime)
:p8='102.2'
:p9='101.1'
:p10='85.5'
:p11='83.3'
:p12='0x61626364' (Nullable = false) (Size = 8000)
:p13='81.1'
:p14='help' (Nullable = false) (Size = 2000)
:p15='anyone!' (Nullable = false) (Size = 2000)
:p16='Gumball Rules OK!' (Nullable = false) (Size = 2000)
:p17='103.3'
:p18='" + entity.Nvarchar2Max + @"' (Nullable = false) (Size = 2000)
:p19='84.4'
:p20='79'
:p21='Gumball Rules!' (Nullable = false) (Size = 2000)
:p22='11:15:12' (DbType = Object)
:p23='80'
:p24='0x595A5B5C' (Nullable = false) (Size = 2000)
:p25='" + entity.Varchar2Max + "' (Nullable = false) (Size = 2000) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 77), 77);
            }
        }

        private string DumpParameters()
            => Fixture.TestSqlLoggerFactory.Parameters.Single().Replace(", ", _eol);

        private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
        {
            var expected = CreateMappedDataTypes(id);
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.Bigint);
            Assert.Equal(79, entity.Smallint);
            Assert.Equal(80, entity.Tinyint);
            Assert.True(entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            //Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            Assert.Equal(new DateTime(2015, 1, 2, 10, 11, 12), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            //Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            //Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal(expected.Varchar2Max, entity.Varchar2Max);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal(expected.Nvarchar2Max, entity.Nvarchar2Max);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            //Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.Image);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.Dec);
            Assert.Equal(103m, entity.Numeric);
        }

        private static MappedDataTypes CreateMappedDataTypes(int id)
            => new MappedDataTypes
            {
                Int = id,
                Bigint = 78L,
                Smallint = 79,
                Tinyint = 80,
                Bit = true,
                Money = 81.1m,
                //Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                //Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                //Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                Varchar2Max = string.Concat(Enumerable.Repeat("C", 2000)),
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                Nvarchar2Max = string.Concat(Enumerable.Repeat("D", 2000)),
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                //Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='77'
:p1='78' (Nullable = true)
:p2='1' (Nullable = true)
:p3='Your' (Size = 2000)
:p4='strong' (Size = 2000)
:p5='2019-01-02T14:11:12' (Nullable = true) (DbType = DateTime)
:p6='2016-01-02T11:11:12' (Nullable = true) (DbType = DateTime)
:p7='102.2' (Nullable = true)
:p8='101.1' (Nullable = true)
:p9='85.5' (Nullable = true)
:p10='83.3' (Nullable = true)
:p11='0x61626364' (Size = 8000)
:p12='81.1' (Nullable = true)
:p13='help' (Size = 2000)
:p14='anyone!' (Size = 2000)
:p15='Gumball Rules OK!' (Size = 2000)
:p16='103.3' (Nullable = true)
:p17='' (Size = 2000)
:p18='84.4' (Nullable = true)
:p19='79' (Nullable = true)
:p20='Gumball Rules!' (Size = 2000)
:p21='11:15:12' (Nullable = true) (DbType = Object)
:p22='80' (Nullable = true)
:p23='0x595A5B5C' (Size = 2000)
:p24='' (Size = 2000) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.Bigint);
            Assert.Equal(79, entity.Smallint.Value);
            Assert.Equal(80, entity.Tinyint.Value);
            Assert.True(entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            //            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            //            Assert.Equal(new DateTime(2015, 1, 2), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            //            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            //            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            //            Assert.Equal("C", entity.VarcharMax);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            //            Assert.Equal("don't", entity.NvarcharMax);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            //            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.Image);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.Dec);
            Assert.Equal(103m, entity.Numeric);
        }

        private static MappedNullableDataTypes CreateMappedNullableDataTypes(int id)
            => new MappedNullableDataTypes
            {
                Int = id,
                Bigint = 78L,
                Smallint = 79,
                Tinyint = 80,
                Bit = true,
                Money = 81.1m,
                //                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                //                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                //                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                //                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                //                VarcharMax = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                //                NvarcharMax = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                //                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='78'
:p1='' (DbType = Int64)
:p2='' (DbType = Int32)
:p3='' (Size = 2000)
:p4='' (Size = 2000)
:p5='' (DbType = DateTime)
:p6='' (DbType = DateTime)
:p7=''
:p8=''
:p9=''
:p10=''
:p11='' (Size = 8000) (DbType = Binary)
:p12=''
:p13='' (Size = 2000)
:p14='' (Size = 2000)
:p15='' (Size = 2000)
:p16=''
:p17='' (Size = 2000)
:p18=''
:p19='' (DbType = Int16)
:p20='' (Size = 2000)
:p21=''
:p22='' (DbType = Byte)
:p23='' (Size = 2000) (DbType = Binary)
:p24='' (Size = 2000) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78), 78);
            }
        }

        private static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.Bigint);
            Assert.Null(entity.Smallint);
            Assert.Null(entity.Tinyint);
            Assert.Null(entity.Bit);
            Assert.Null(entity.Money);
            //            Assert.Null(entity.Smallmoney);
            Assert.Null(entity.Float);
            Assert.Null(entity.Real);
            Assert.Null(entity.Double_precision);
            //            Assert.Null(entity.Date);
            Assert.Null(entity.Datetimeoffset);
            //            Assert.Null(entity.Datetime2);
            //            Assert.Null(entity.Smalldatetime);
            Assert.Null(entity.Datetime);
            Assert.Null(entity.Time);
            //            Assert.Null(entity.VarcharMax);
            Assert.Null(entity.Char_varyingMax);
            Assert.Null(entity.Character_varyingMax);
            //            Assert.Null(entity.NvarcharMax);
            Assert.Null(entity.National_char_varyingMax);
            Assert.Null(entity.National_character_varyingMax);
            Assert.Null(entity.Text);
            Assert.Null(entity.Ntext);
            Assert.Null(entity.VarbinaryMax);
            //            Assert.Null(entity.Binary_varyingMax);
            Assert.Null(entity.Image);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.Dec);
            Assert.Null(entity.Numeric);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='77'
:p1='0x0A0B0C' (Size = 3)
:p2='Wor' (Size = 3) (DbType = AnsiString)
:p3='Thr' (Size = 3)
:p4='Lon' (Size = 3)
:p5='Let' (Size = 3)
:p6='The' (Size = 3)
:p7='Squ' (Size = 3)
:p8='Col' (Size = 3)
:p9='Won' (Size = 3)
:p10='Int' (Size = 3)
:p11='0x0B0C0D' (Size = 3)
:p12='Tha' (Size = 3) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal("Wor", entity.Char);
            Assert.Equal("Lon", entity.Character);
            Assert.Equal("Tha", entity.Varchar2);
            Assert.Equal("Thr", entity.Char_varying);
            Assert.Equal("Let", entity.Character_varying);
            Assert.Equal("Won", entity.Nchar);
            Assert.Equal("Squ", entity.National_character);
            Assert.Equal("Int", entity.Nvarchar2);
            Assert.Equal("The", entity.National_char_varying);
            Assert.Equal("Col", entity.National_character_varying);
            Assert.Equal(new byte[] { 10, 11, 12 }, entity.Binary);
            Assert.Equal(new byte[] { 11, 12, 13 }, entity.Varbinary);
            //            Assert.Equal(new byte[] { 12, 13, 14 }, entity.Binary_varying);
        }

        private static MappedSizedDataTypes CreateMappedSizedDataTypes(int id)
            => new MappedSizedDataTypes
            {
                Id = id,
                Char = "Wor",
                Character = "Lon",
                Varchar2 = "Tha",
                Char_varying = "Thr",
                Character_varying = "Let",
                Nchar = "Won",
                National_character = "Squ",
                Nvarchar2 = "Int",
                National_char_varying = "The",
                National_character_varying = "Col",
                Binary = new byte[] { 10, 11, 12 },
                Varbinary = new byte[] { 11, 12, 13 }
                //                Binary_varying = new byte[] { 12, 13, 14 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='78'
:p1='' (Size = 3) (DbType = Binary)
:p2='' (Size = 3) (DbType = AnsiString)
:p3='' (Size = 3)
:p4='' (Size = 3)
:p5='' (Size = 3)
:p6='' (Size = 3)
:p7='' (Size = 3)
:p8='' (Size = 3)
:p9='' (Size = 3)
:p10='' (Size = 3)
:p11='' (Size = 3) (DbType = Binary)
:p12='' (Size = 3) (DbType = AnsiString)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypes(context.Set<MappedSizedDataTypes>().Single(e => e.Id == 78), 78);
            }
        }

        private static void AssertNullMappedSizedDataTypes(MappedSizedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Null(entity.Char);
            Assert.Null(entity.Character);
            Assert.Null(entity.Varchar2);
            Assert.Null(entity.Char_varying);
            Assert.Null(entity.Character_varying);
            Assert.Null(entity.Nchar);
            Assert.Null(entity.National_character);
            Assert.Null(entity.Nvarchar2);
            Assert.Null(entity.National_char_varying);
            Assert.Null(entity.National_character_varying);
            Assert.Null(entity.Binary);
            Assert.Null(entity.Varbinary);
            //            Assert.Null(entity.Binary_varying);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='77'
:p1='2017-01-02T12:11:12' (DbType = DateTime)
:p2='2016-01-02T11:11:12' (DbType = DateTime)
:p3='102.2'
:p4='101.1'
:p5='83.3'
:p6='103.3'",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypes(MappedScaledDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(83.3f, entity.Float);
            //            Assert.Equal(85.5f, entity.Double_precision);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.Dec);
            Assert.Equal(103m, entity.Numeric);
        }

        private static MappedScaledDataTypes CreateMappedScaledDataTypes(int id)
            => new MappedScaledDataTypes
            {
                Id = id,
                Float = 83.3f,
                //                Double_precision = 85.5f,
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='77'
:p1='102.2'
:p2='101.1'
:p3='103.3'",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedPrecisionAndScaledDataTypes(MappedPrecisionAndScaledDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(101.1m, entity.Decimal);
            Assert.Equal(102.2m, entity.Dec);
            Assert.Equal(103.3m, entity.Numeric);
        }

        private static MappedPrecisionAndScaledDataTypes CreateMappedPrecisionAndScaledDataTypes(int id)
            => new MappedPrecisionAndScaledDataTypes
            {
                Id = id,
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='78'
:p1='1'
:p2='Your' (Size = 2000)
:p3='strong' (Size = 2000)
:p4='2015-01-02T10:11:12' (DbType = Date)
:p5='2019-01-02T14:11:12' (DbType = DateTime)
:p6='2016-01-02T11:11:12' (DbType = DateTime)
:p7='102.2'
:p8='101.1'
:p9='85.5'
:p10='83.3'
:p11='0x61626364' (Size = 8000)
:p12='77'
:p13='81.1'
:p14='help' (Size = 2000)
:p15='anyone!' (Size = 2000)
:p16='Gumball Rules OK!' (Size = 2000)
:p17='103.3'
:p18='don't' (Size = 2000)
:p19='84.4'
:p20='79'
:p21='Gumball Rules!' (Size = 2000)
:p22='11:15:12' (DbType = Object)
:p23='80'
:p24='0x595A5B5C' (Size = 2000)
:p25='C' (Size = 2000) (DbType = AnsiString)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedDataTypesWithIdentity(MappedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.Bigint);
            Assert.Equal(79, entity.Smallint);
            Assert.Equal(80, entity.Tinyint);
            Assert.True(entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            //            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            Assert.Equal(new DateTime(2015, 1, 2, 10, 11, 12), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            //            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            //            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal("C", entity.Varchar2Max);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal("don't", entity.Nvarchar2Max);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            //            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.Image);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.Dec);
            Assert.Equal(103m, entity.Numeric);
        }

        private static MappedDataTypesWithIdentity CreateMappedDataTypesWithIdentity(int id)
            => new MappedDataTypesWithIdentity
            {
                Int = id,
                Bigint = 78L,
                Smallint = 79,
                Tinyint = 80,
                Bit = true,
                Money = 81.1m,
                //                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                //                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                //                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                Varchar2Max = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                Nvarchar2Max = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                //                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='78' (Nullable = true)
:p1='1' (Nullable = true)
:p2='Your' (Size = 2000)
:p3='strong' (Size = 2000)
:p4='2019-01-02T14:11:12' (Nullable = true) (DbType = DateTime)
:p5='2016-01-02T11:11:12' (Nullable = true) (DbType = DateTime)
:p6='102.2' (Nullable = true)
:p7='101.1' (Nullable = true)
:p8='85.5' (Nullable = true)
:p9='83.3' (Nullable = true)
:p10='0x61626364' (Size = 8000)
:p11='77' (Nullable = true)
:p12='81.1' (Nullable = true)
:p13='help' (Size = 2000)
:p14='anyone!' (Size = 2000)
:p15='Gumball Rules OK!' (Size = 2000)
:p16='103.3' (Nullable = true)
:p17='don't' (Size = 2000)
:p18='84.4' (Nullable = true)
:p19='79' (Nullable = true)
:p20='Gumball Rules!' (Size = 2000)
:p21='11:15:12' (Nullable = true) (DbType = Object)
:p22='80' (Nullable = true)
:p23='0x595A5B5C' (Size = 2000)
:p24='C' (Size = 2000) (DbType = AnsiString)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedNullableDataTypesWithIdentity(MappedNullableDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.Bigint);
            Assert.Equal(79, entity.Smallint.Value);
            Assert.Equal(80, entity.Tinyint.Value);
            Assert.True(entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            //            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            //            Assert.Equal(new DateTime(2015, 1, 2), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            //            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            //            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal("C", entity.Varchar2Max);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal("don't", entity.Nvarchar2Max);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            //            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.Image);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.Dec);
            Assert.Equal(103m, entity.Numeric);
        }

        private static MappedNullableDataTypesWithIdentity CreateMappedNullableDataTypesWithIdentity(int id)
            => new MappedNullableDataTypesWithIdentity
            {
                Int = id,
                Bigint = 78L,
                Smallint = 79,
                Tinyint = 80,
                Bit = true,
                Money = 81.1m,
                //                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                //                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                //                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                //                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                Varchar2Max = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                Nvarchar2Max = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                //                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='' (DbType = Int64)
:p1='' (DbType = Int32)
:p2='' (Size = 2000)
:p3='' (Size = 2000)
:p4='' (DbType = DateTime)
:p5='' (DbType = DateTime)
:p6=''
:p7=''
:p8=''
:p9=''
:p10='' (Size = 8000) (DbType = Binary)
:p11='78' (Nullable = true)
:p12=''
:p13='' (Size = 2000)
:p14='' (Size = 2000)
:p15='' (Size = 2000)
:p16=''
:p17='' (Size = 2000)
:p18=''
:p19='' (DbType = Int16)
:p20='' (Size = 2000)
:p21=''
:p22='' (DbType = Byte)
:p23='' (Size = 2000) (DbType = Binary)
:p24='' (Size = 2000) (DbType = AnsiString)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 78), 78);
            }
        }

        private static void AssertNullMappedNullableDataTypesWithIdentity(
            MappedNullableDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.Bigint);
            Assert.Null(entity.Smallint);
            Assert.Null(entity.Tinyint);
            Assert.Null(entity.Bit);
            Assert.Null(entity.Money);
            //            Assert.Null(entity.Smallmoney);
            Assert.Null(entity.Float);
            Assert.Null(entity.Real);
            Assert.Null(entity.Double_precision);
            //            Assert.Null(entity.Date);
            Assert.Null(entity.Datetimeoffset);
            //            Assert.Null(entity.Datetime2);
            //            Assert.Null(entity.Smalldatetime);
            Assert.Null(entity.Datetime);
            Assert.Null(entity.Time);
            Assert.Null(entity.Varchar2Max);
            Assert.Null(entity.Char_varyingMax);
            Assert.Null(entity.Character_varyingMax);
            Assert.Null(entity.Nvarchar2Max);
            Assert.Null(entity.National_char_varyingMax);
            Assert.Null(entity.National_character_varyingMax);
            Assert.Null(entity.Text);
            Assert.Null(entity.Ntext);
            Assert.Null(entity.VarbinaryMax);
            //            Assert.Null(entity.Binary_varyingMax);
            Assert.Null(entity.Image);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.Dec);
            Assert.Null(entity.Numeric);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='0x0A0B0C' (Size = 3)
:p1='Wor' (Size = 3) (DbType = AnsiString)
:p2='Thr' (Size = 3)
:p3='Lon' (Size = 3)
:p4='Let' (Size = 3)
:p5='77'
:p6='The' (Size = 3)
:p7='Squ' (Size = 3)
:p8='Col' (Size = 3)
:p9='Won' (Size = 3)
:p10='Int' (Size = 3)
:p11='0x0B0C0D' (Size = 3)
:p12='Tha' (Size = 3) (DbType = AnsiString)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal("Wor", entity.Char);
            Assert.Equal("Lon", entity.Character);
            Assert.Equal("Tha", entity.Varchar2);
            Assert.Equal("Thr", entity.Char_varying);
            Assert.Equal("Let", entity.Character_varying);
            Assert.Equal("Won", entity.Nchar);
            Assert.Equal("Squ", entity.National_character);
            Assert.Equal("Int", entity.Nvarchar2);
            Assert.Equal("The", entity.National_char_varying);
            Assert.Equal("Col", entity.National_character_varying);
            Assert.Equal(new byte[] { 10, 11, 12 }, entity.Binary);
            Assert.Equal(new byte[] { 11, 12, 13 }, entity.Varbinary);
            //            Assert.Equal(new byte[] { 12, 13, 14 }, entity.Binary_varying);
        }

        private static MappedSizedDataTypesWithIdentity CreateMappedSizedDataTypesWithIdentity(int id)
            => new MappedSizedDataTypesWithIdentity
            {
                Int = id,
                Char = "Wor",
                Character = "Lon",
                Varchar2 = "Tha",
                Char_varying = "Thr",
                Character_varying = "Let",
                Nchar = "Won",
                National_character = "Squ",
                Nvarchar2 = "Int",
                National_char_varying = "The",
                National_character_varying = "Col",
                Binary = new byte[] { 10, 11, 12 },
                Varbinary = new byte[] { 11, 12, 13 }
                //                Binary_varying = new byte[] { 12, 13, 14 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='' (Size = 3) (DbType = Binary)
:p1='' (Size = 3) (DbType = AnsiString)
:p2='' (Size = 3)
:p3='' (Size = 3)
:p4='' (Size = 3)
:p5='78'
:p6='' (Size = 3)
:p7='' (Size = 3)
:p8='' (Size = 3)
:p9='' (Size = 3)
:p10='' (Size = 3)
:p11='' (Size = 3) (DbType = Binary)
:p12='' (Size = 3) (DbType = AnsiString)
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 78), 78);
            }
        }

        private static void AssertNullMappedSizedDataTypesWithIdentity(MappedSizedDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.Char);
            Assert.Null(entity.Character);
            Assert.Null(entity.Varchar2);
            Assert.Null(entity.Char_varying);
            Assert.Null(entity.Character_varying);
            Assert.Null(entity.Nchar);
            Assert.Null(entity.National_character);
            Assert.Null(entity.Nvarchar2);
            Assert.Null(entity.National_char_varying);
            Assert.Null(entity.National_character_varying);
            Assert.Null(entity.Binary);
            Assert.Null(entity.Varbinary);
            //            Assert.Null(entity.Binary_varying);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='2017-01-02T12:11:12' (DbType = DateTime)
:p1='2016-01-02T11:11:12' (DbType = DateTime)
:p2='102.2'
:p3='101.1'
:p4='83.3'
:p5='77'
:p6='103.3'
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypesWithIdentity(MappedScaledDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(83.3f, entity.Float);
            //            Assert.Equal(85.5f, entity.Double_precision);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(101m, entity.Decimal);
            Assert.Equal(102m, entity.Dec);
            Assert.Equal(103m, entity.Numeric);
        }

        private static MappedScaledDataTypesWithIdentity CreateMappedScaledDataTypesWithIdentity(int id)
            => new MappedScaledDataTypesWithIdentity
            {
                Int = id,
                Float = 83.3f,
                //                Double_precision = 85.5f,
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    CreateMappedPrecisionAndScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @":p0='102.2'
:p1='101.1'
:p2='77'
:p3='103.3'
cur1='' (Nullable = false) (Direction = Output) (DbType = Object)",
                parameters,
                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedPrecisionAndScaledDataTypesWithIdentity(MappedPrecisionAndScaledDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(101.1m, entity.Decimal);
            Assert.Equal(102.2m, entity.Dec);
            Assert.Equal(103.3m, entity.Numeric);
        }

        private static MappedPrecisionAndScaledDataTypesWithIdentity CreateMappedPrecisionAndScaledDataTypesWithIdentity(int id)
            => new MappedPrecisionAndScaledDataTypesWithIdentity
            {
                Int = id,
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(177));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(178));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 177), 177);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 178), 178);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(177));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(178));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 177), 177);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 178), 178);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 278 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 279 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 278), 278);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 279), 279);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_in_batch()
        {
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
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(177));
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(178));
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 177), 177);
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 178), 178);
                AssertMappedPrecisionAndScaledDataTypes(context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(177));
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(178));
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedDataTypesWithIdentity(context.Set<MappedDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(177));
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(178));
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 278 });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 279 });
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 278), 278);
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 279), 279);
                AssertNullMappedNullableDataTypesWithIdentity(context.Set<MappedNullableDataTypesWithIdentity>().Single(e => e.Int == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(177));
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(178));
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 278 });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 279 });
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 280 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 278), 278);
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 279), 279);
                AssertNullMappedSizedDataTypesWithIdentity(context.Set<MappedSizedDataTypesWithIdentity>().Single(e => e.Int == 280), 280);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(177));
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(178));
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity_in_batch()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(177));
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(178));
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(CreateMappedPrecisionAndScaledDataTypesWithIdentity(179));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 177), 177);
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 178), 178);
                AssertMappedPrecisionAndScaledDataTypesWithIdentity(
                    context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Single(e => e.Int == 179), 179);
            }
        }

        [ConditionalFact]
        public virtual void Columns_have_expected_data_types()
        {
            const string query
                = @"SELECT
                      TABLE_NAME,
                      COLUMN_NAME,
                      DATA_TYPE,
                      NULLABLE,
                      DATA_LENGTH,
                      DATA_SCALE,
                      DATA_PRECISION
                    FROM user_tab_columns";

            var columns = new List<ColumnInfo>();

            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                var command = connection.CreateCommand();
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnInfo = new ColumnInfo
                        {
                            TableName = reader.GetString(0),
                            ColumnName = reader.GetString(1),
                            DataType = reader.GetString(2),
                            IsNullable = reader.IsDBNull(3) ? null : (bool?)(reader.GetString(3) == "YES"),
                            MaxLength = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                            NumericPrecision = reader.IsDBNull(6) ? null : (int?)reader.GetByte(6),
                            NumericScale = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                            DateTimePrecision = reader.IsDBNull(6) ? null : (int?)reader.GetInt16(6)
                        };

                        columns.Add(columnInfo);
                    }
                }
            }

            var builder = new StringBuilder();

            foreach (var column in columns.OrderBy(e => e.TableName).ThenBy(e => e.ColumnName))
            {
                builder.Append(column.TableName);
                builder.Append(".");
                builder.Append(column.ColumnName);
                builder.Append(" ---> [");

                if (column.IsNullable == true)
                {
                    builder.Append("nullable ");
                }

                builder.Append(column.DataType);
                builder.Append("]");

                if (column.MaxLength.HasValue)
                {
                    builder.Append(" [MaxLength = ");
                    builder.Append(column.MaxLength);
                    builder.Append("]");
                }

                if (column.NumericPrecision.HasValue)
                {
                    builder.Append(" [Precision = ");
                    builder.Append(column.NumericPrecision);
                }

                if (column.DateTimePrecision.HasValue)
                {
                    builder.Append(" [Precision = ");
                    builder.Append(column.DateTimePrecision);
                }

                if (column.NumericScale.HasValue)
                {
                    builder.Append(" Scale = ");
                    builder.Append(column.NumericScale);
                }

                if (column.NumericPrecision.HasValue
                    || column.DateTimePrecision.HasValue
                    || column.NumericScale.HasValue)
                {
                    builder.Append("]");
                }

                builder.AppendLine();
            }

            var actual = builder.ToString();

            const string expected = @"BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [RAW] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BinaryKeyDataType.Id ---> [RAW] [MaxLength = 900]
BuiltInDataTypes.Enum16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypes.Enum32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.Enum64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInDataTypes.EnumS8 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypes.EnumU16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.EnumU32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypes.EnumU64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.TestByte ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInDataTypes.TestDateTime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
BuiltInDataTypes.TestDateTimeOffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
BuiltInDataTypes.TestDecimal ---> [NUMBER] [MaxLength = 22] [Precision = 18 [Precision = 18 Scale = 2]
BuiltInDataTypes.TestDouble ---> [FLOAT] [MaxLength = 22] [Precision = 49 [Precision = 49]
BuiltInDataTypes.TestInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSignedByte ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypes.TestSingle ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
BuiltInDataTypes.TestTimeSpan ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
BuiltInDataTypes.TestUnsignedInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypes.TestUnsignedInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypes.TestUnsignedInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Enum16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypesShadow.Enum32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.Enum64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.Enum8 ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInDataTypesShadow.EnumS8 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypesShadow.EnumU16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.EnumU32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.EnumU64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInDataTypesShadow.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.PartitionId ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestBoolean ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestByte ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInDataTypesShadow.TestCharacter ---> [NVARCHAR2] [MaxLength = 2]
BuiltInDataTypesShadow.TestDateTime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
BuiltInDataTypesShadow.TestDateTimeOffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
BuiltInDataTypesShadow.TestDecimal ---> [NUMBER] [MaxLength = 22] [Precision = 29 [Precision = 29 Scale = 4]
BuiltInDataTypesShadow.TestDouble ---> [FLOAT] [MaxLength = 22] [Precision = 49 [Precision = 49]
BuiltInDataTypesShadow.TestInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypesShadow.TestInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestSignedByte ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInDataTypesShadow.TestSingle ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
BuiltInDataTypesShadow.TestTimeSpan ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Enum16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.EnumS8 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypes.EnumU16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.EnumU32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.EnumU64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [BLOB] [MaxLength = 4000]
BuiltInNullableDataTypes.TestNullableBoolean ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableByte ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateTime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
BuiltInNullableDataTypes.TestNullableDecimal ---> [NUMBER] [MaxLength = 22] [Precision = 29 [Precision = 29 Scale = 4]
BuiltInNullableDataTypes.TestNullableDouble ---> [FLOAT] [MaxLength = 22] [Precision = 49 [Precision = 49]
BuiltInNullableDataTypes.TestNullableInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypes.TestNullableSingle ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInNullableDataTypes.TestString ---> [NVARCHAR2] [MaxLength = 4000]
BuiltInNullableDataTypesShadow.Enum16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypesShadow.Enum32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.Enum64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.Enum8 ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInNullableDataTypesShadow.EnumS8 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.EnumU64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.PartitionId ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestByteArray ---> [BLOB] [MaxLength = 4000]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [NVARCHAR2] [MaxLength = 2]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [NUMBER] [MaxLength = 22] [Precision = 29 [Precision = 29 Scale = 4]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [FLOAT] [MaxLength = 22] [Precision = 49 [Precision = 49]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [NUMBER] [MaxLength = 22] [Precision = 6 [Precision = 6 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [NUMBER] [MaxLength = 22] [Precision = 20 [Precision = 20 Scale = 0]
BuiltInNullableDataTypesShadow.TestString ---> [NVARCHAR2] [MaxLength = 4000]
EmailTemplate.Id ---> [RAW] [MaxLength = 16]
EmailTemplate.TemplateType ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedDataTypes.Bigint ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
MappedDataTypes.Bit ---> [NUMBER] [MaxLength = 22] [Precision = 1 [Precision = 1 Scale = 0]
MappedDataTypes.Char_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedDataTypes.Character_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedDataTypes.Date ---> [DATE] [MaxLength = 7]
MappedDataTypes.Datetime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
MappedDataTypes.Datetimeoffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
MappedDataTypes.Dec ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypes.Decimal ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypes.Double_precision ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedDataTypes.Float ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedDataTypes.Image ---> [BLOB] [MaxLength = 4000]
MappedDataTypes.Int ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypes.Money ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 4]
MappedDataTypes.National_char_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedDataTypes.National_character_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedDataTypes.Ntext ---> [NCLOB] [MaxLength = 4000]
MappedDataTypes.Numeric ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypes.Nvarchar2Max ---> [NVARCHAR2] [MaxLength = 4000]
MappedDataTypes.Real ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
MappedDataTypes.Smallint ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypes.Text ---> [CLOB] [MaxLength = 4000]
MappedDataTypes.Time ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
MappedDataTypes.Tinyint ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedDataTypes.VarbinaryMax ---> [RAW] [MaxLength = 2000]
MappedDataTypes.Varchar2Max ---> [VARCHAR2] [MaxLength = 2000]
MappedDataTypesWithIdentity.Bigint ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.Bit ---> [NUMBER] [MaxLength = 22] [Precision = 1 [Precision = 1 Scale = 0]
MappedDataTypesWithIdentity.Char_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedDataTypesWithIdentity.Character_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedDataTypesWithIdentity.Date ---> [DATE] [MaxLength = 7]
MappedDataTypesWithIdentity.Datetime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
MappedDataTypesWithIdentity.Datetimeoffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
MappedDataTypesWithIdentity.Dec ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypesWithIdentity.Decimal ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypesWithIdentity.Double_precision ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedDataTypesWithIdentity.Float ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedDataTypesWithIdentity.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.Image ---> [BLOB] [MaxLength = 4000]
MappedDataTypesWithIdentity.Int ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypesWithIdentity.Money ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 4]
MappedDataTypesWithIdentity.National_char_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedDataTypesWithIdentity.National_character_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedDataTypesWithIdentity.Ntext ---> [NCLOB] [MaxLength = 4000]
MappedDataTypesWithIdentity.Numeric ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypesWithIdentity.Nvarchar2Max ---> [NVARCHAR2] [MaxLength = 4000]
MappedDataTypesWithIdentity.Real ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
MappedDataTypesWithIdentity.Smallint ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedDataTypesWithIdentity.Text ---> [CLOB] [MaxLength = 4000]
MappedDataTypesWithIdentity.Time ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
MappedDataTypesWithIdentity.Tinyint ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedDataTypesWithIdentity.VarbinaryMax ---> [RAW] [MaxLength = 2000]
MappedDataTypesWithIdentity.Varchar2Max ---> [VARCHAR2] [MaxLength = 2000]
MappedNullableDataTypes.Bigint ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
MappedNullableDataTypes.Bit ---> [NUMBER] [MaxLength = 22] [Precision = 1 [Precision = 1 Scale = 0]
MappedNullableDataTypes.Char_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedNullableDataTypes.Character_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedNullableDataTypes.Datetime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
MappedNullableDataTypes.Datetimeoffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
MappedNullableDataTypes.Dec ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypes.Decimal ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypes.Double_precision ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedNullableDataTypes.Float ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedNullableDataTypes.Image ---> [BLOB] [MaxLength = 4000]
MappedNullableDataTypes.Int ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypes.Money ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 4]
MappedNullableDataTypes.National_char_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedNullableDataTypes.National_character_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedNullableDataTypes.Ntext ---> [NCLOB] [MaxLength = 4000]
MappedNullableDataTypes.Numeric ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypes.Nvarchar2Max ---> [NVARCHAR2] [MaxLength = 4000]
MappedNullableDataTypes.Real ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
MappedNullableDataTypes.Smallint ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypes.Text ---> [CLOB] [MaxLength = 4000]
MappedNullableDataTypes.Time ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
MappedNullableDataTypes.Tinyint ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedNullableDataTypes.VarbinaryMax ---> [RAW] [MaxLength = 2000]
MappedNullableDataTypes.Varchar2Max ---> [VARCHAR2] [MaxLength = 2000]
MappedNullableDataTypesWithIdentity.Bigint ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.Bit ---> [NUMBER] [MaxLength = 22] [Precision = 1 [Precision = 1 Scale = 0]
MappedNullableDataTypesWithIdentity.Char_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedNullableDataTypesWithIdentity.Character_varyingMax ---> [VARCHAR2] [MaxLength = 2000]
MappedNullableDataTypesWithIdentity.Datetime ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
MappedNullableDataTypesWithIdentity.Datetimeoffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
MappedNullableDataTypesWithIdentity.Dec ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypesWithIdentity.Decimal ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypesWithIdentity.Double_precision ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedNullableDataTypesWithIdentity.Float ---> [FLOAT] [MaxLength = 22] [Precision = 126 [Precision = 126]
MappedNullableDataTypesWithIdentity.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.Image ---> [BLOB] [MaxLength = 4000]
MappedNullableDataTypesWithIdentity.Int ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypesWithIdentity.Money ---> [NUMBER] [MaxLength = 22] [Precision = 19 [Precision = 19 Scale = 4]
MappedNullableDataTypesWithIdentity.National_char_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedNullableDataTypesWithIdentity.National_character_varyingMax ---> [NVARCHAR2] [MaxLength = 4000]
MappedNullableDataTypesWithIdentity.Ntext ---> [NCLOB] [MaxLength = 4000]
MappedNullableDataTypesWithIdentity.Numeric ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypesWithIdentity.Nvarchar2Max ---> [NVARCHAR2] [MaxLength = 4000]
MappedNullableDataTypesWithIdentity.Real ---> [FLOAT] [MaxLength = 22] [Precision = 63 [Precision = 63]
MappedNullableDataTypesWithIdentity.Smallint ---> [NUMBER] [MaxLength = 22] Scale = 0]
MappedNullableDataTypesWithIdentity.Text ---> [CLOB] [MaxLength = 4000]
MappedNullableDataTypesWithIdentity.Time ---> [INTERVAL DAY(2) TO SECOND(6)] [MaxLength = 11] [Precision = 2 [Precision = 2 Scale = 6]
MappedNullableDataTypesWithIdentity.Tinyint ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedNullableDataTypesWithIdentity.VarbinaryMax ---> [RAW] [MaxLength = 2000]
MappedNullableDataTypesWithIdentity.Varchar2Max ---> [VARCHAR2] [MaxLength = 2000]
MappedPrecisionAndScaledDataTypes.Dec ---> [NUMBER] [MaxLength = 22] [Precision = 5 [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Decimal ---> [NUMBER] [MaxLength = 22] [Precision = 5 [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypes.Numeric ---> [NUMBER] [MaxLength = 22] [Precision = 5 [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Dec ---> [NUMBER] [MaxLength = 22] [Precision = 5 [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Decimal ---> [NUMBER] [MaxLength = 22] [Precision = 5 [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.Int ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.Numeric ---> [NUMBER] [MaxLength = 22] [Precision = 5 [Precision = 5 Scale = 2]
MappedScaledDataTypes.Datetime2 ---> [TIMESTAMP(6)] [MaxLength = 11] Scale = 6]
MappedScaledDataTypes.Datetimeoffset ---> [TIMESTAMP(6) WITH TIME ZONE] [MaxLength = 13] Scale = 6]
MappedScaledDataTypes.Dec ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedScaledDataTypes.Decimal ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedScaledDataTypes.Float ---> [FLOAT] [MaxLength = 22] [Precision = 10 [Precision = 10]
MappedScaledDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedScaledDataTypes.Numeric ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.Datetime2 ---> [TIMESTAMP(3)] [MaxLength = 11] Scale = 3]
MappedScaledDataTypesWithIdentity.Datetimeoffset ---> [TIMESTAMP(3) WITH TIME ZONE] [MaxLength = 13] Scale = 3]
MappedScaledDataTypesWithIdentity.Dec ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.Decimal ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.Float ---> [FLOAT] [MaxLength = 22] [Precision = 10 [Precision = 10]
MappedScaledDataTypesWithIdentity.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.Int ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.Numeric ---> [NUMBER] [MaxLength = 22] [Precision = 3 [Precision = 3 Scale = 0]
MappedSizedDataTypes.Binary ---> [RAW] [MaxLength = 3]
MappedSizedDataTypes.Char ---> [CHAR] [MaxLength = 3]
MappedSizedDataTypes.Char_varying ---> [VARCHAR2] [MaxLength = 3]
MappedSizedDataTypes.Character ---> [CHAR] [MaxLength = 3]
MappedSizedDataTypes.Character_varying ---> [VARCHAR2] [MaxLength = 3]
MappedSizedDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedSizedDataTypes.National_char_varying ---> [NVARCHAR2] [MaxLength = 6]
MappedSizedDataTypes.National_character ---> [NCHAR] [MaxLength = 6]
MappedSizedDataTypes.National_character_varying ---> [NVARCHAR2] [MaxLength = 6]
MappedSizedDataTypes.Nchar ---> [NCHAR] [MaxLength = 6]
MappedSizedDataTypes.Nvarchar2 ---> [NVARCHAR2] [MaxLength = 6]
MappedSizedDataTypes.Varbinary ---> [RAW] [MaxLength = 3]
MappedSizedDataTypes.Varchar2 ---> [VARCHAR2] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Binary ---> [RAW] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Char ---> [CHAR] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Char_varying ---> [VARCHAR2] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Character ---> [CHAR] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Character_varying ---> [VARCHAR2] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.Int ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.National_char_varying ---> [NVARCHAR2] [MaxLength = 6]
MappedSizedDataTypesWithIdentity.National_character ---> [NCHAR] [MaxLength = 6]
MappedSizedDataTypesWithIdentity.National_character_varying ---> [NVARCHAR2] [MaxLength = 6]
MappedSizedDataTypesWithIdentity.Nchar ---> [NCHAR] [MaxLength = 6]
MappedSizedDataTypesWithIdentity.Nvarchar2 ---> [NVARCHAR2] [MaxLength = 6]
MappedSizedDataTypesWithIdentity.Varbinary ---> [RAW] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Varchar2 ---> [VARCHAR2] [MaxLength = 3]
MaxLengthDataTypes.ByteArray5 ---> [RAW] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [BLOB] [MaxLength = 4000]
MaxLengthDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [NVARCHAR2] [MaxLength = 6]
MaxLengthDataTypes.String9000 ---> [NCLOB] [MaxLength = 4000]
StringForeignKeyDataType.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [NVARCHAR2] [MaxLength = 900]
StringKeyDataType.Id ---> [NVARCHAR2] [MaxLength = 900]
UnicodeDataTypes.Id ---> [NUMBER] [MaxLength = 22] [Precision = 10 [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [VARCHAR2] [MaxLength = 4000]
UnicodeDataTypes.StringAnsi3 ---> [VARCHAR2] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [CLOB] [MaxLength = 4000]
UnicodeDataTypes.StringDefault ---> [NVARCHAR2] [MaxLength = 4000]
UnicodeDataTypes.StringUnicode ---> [NVARCHAR2] [MaxLength = 4000]
";

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
        }

        private static readonly string _eol = Environment.NewLine;

        [Fact]
        public void Can_get_column_types_from_built_model()
        {
            using (var context = CreateContext())
            {
                var mappingSource = context.GetService<IRelationalTypeMappingSource>();

                foreach (var property in context.Model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
                {
                    var columnType = property.Relational().ColumnType;
                    Assert.NotNull(columnType);

                    if (property[RelationalAnnotationNames.ColumnType] == null)
                    {
                        Assert.Equal(
                            columnType.ToLowerInvariant(),
                            mappingSource.FindMapping(property).StoreType.ToLowerInvariant());
                    }
                }
            }
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class BuiltInDataTypesOracleFixture : BuiltInDataTypesFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => true;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => false;

            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            //public override int LongStringLength { get; } = 4000;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                MakeRequired<MappedDataTypes>(modelBuilder);

                modelBuilder.Entity<BuiltInDataTypes>(
                    b =>
                    {
                        b.Ignore(dt => dt.TestCharacter);
                        b.Property(dt => dt.TestDecimal).HasColumnType("DECIMAL(18,2)");
                    });

                modelBuilder.Entity<BuiltInNullableDataTypes>()
                    .Ignore(dt => dt.TestNullableCharacter);

                modelBuilder.Entity<MappedDataTypes>(
                    b =>
                    {
                        b.HasKey(e => e.Int);
                        b.Property(e => e.Int).ValueGeneratedNever();
                    });

                modelBuilder.Entity<MappedNullableDataTypes>(
                    b =>
                    {
                        b.HasKey(e => e.Int);
                        b.Property(e => e.Int).ValueGeneratedNever();
                    });

                modelBuilder.Entity<MappedSizedDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                MapColumnTypes<MappedDataTypes>(modelBuilder);

                modelBuilder.Entity<MappedDataTypes>(
                    e
                        =>
                    {
                        e.Property(m => m.Bigint).HasColumnType("NUMBER(19)");
                        e.Property(m => m.Tinyint).HasColumnType("NUMBER(3)");
                        e.Property(m => m.Bit).HasColumnType("NUMBER(1)");
                        e.Property(m => m.Money).HasColumnType("DECIMAL(19,4)");
                        e.Property(m => m.Datetimeoffset).HasColumnType("TIMESTAMP WITH TIME ZONE");
                        e.Property(m => m.Datetime).HasColumnType("TIMESTAMP");
                        e.Property(m => m.Time).HasColumnType("INTERVAL DAY TO SECOND");
                        e.Property(m => m.Text).HasColumnType("CLOB");
                        e.Property(m => m.Ntext).HasColumnType("NCLOB");
                        e.Property(m => m.Image).HasColumnType("BLOB");
                        e.Property(m => m.VarbinaryMax).HasColumnType("RAW(2000)");
                    });

                MapColumnTypes<MappedNullableDataTypes>(modelBuilder);

                modelBuilder.Entity<MappedNullableDataTypes>(
                    e
                        =>
                    {
                        e.Property(m => m.Bigint).HasColumnType("NUMBER(19)");
                        e.Property(m => m.Tinyint).HasColumnType("NUMBER(3)");
                        e.Property(m => m.Bit).HasColumnType("NUMBER(1)");
                        e.Property(m => m.Money).HasColumnType("DECIMAL(19,4)");
                        e.Property(m => m.Datetimeoffset).HasColumnType("TIMESTAMP WITH TIME ZONE");
                        e.Property(m => m.Datetime).HasColumnType("TIMESTAMP");
                        e.Property(m => m.Time).HasColumnType("INTERVAL DAY TO SECOND");
                        e.Property(m => m.Text).HasColumnType("CLOB");
                        e.Property(m => m.Ntext).HasColumnType("NCLOB");
                        e.Property(m => m.Image).HasColumnType("BLOB");
                        e.Property(m => m.VarbinaryMax).HasColumnType("RAW(2000)");
                    });

                MapSizedColumnTypes<MappedSizedDataTypes>(modelBuilder);

                modelBuilder.Entity<MappedSizedDataTypes>(
                    e
                        =>
                    {
                        e.Property(m => m.Binary).HasColumnType("RAW(3)");
                        e.Property(m => m.Varbinary).HasColumnType("RAW(3)");
                    });

                MapSizedColumnTypes<MappedScaledDataTypes>(modelBuilder);

                modelBuilder.Entity<MappedScaledDataTypes>(
                    e
                        =>
                    {
                        e.Property(m => m.Datetimeoffset).HasColumnType("TIMESTAMP WITH TIME ZONE");
                        e.Property(m => m.Datetime2).HasColumnType("TIMESTAMP");
                        e.Property(m => m.Float).HasColumnType("FLOAT(10)");
                    });

                MapPreciseColumnTypes<MappedPrecisionAndScaledDataTypes>(modelBuilder);

                modelBuilder.Entity<MappedDataTypesWithIdentity>(b => b.HasKey(e => e.Id));

                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>(b => b.HasKey(e => e.Id));

                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>()
                    .Property(e => e.Id);

                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>()
                    .Property(e => e.Id);

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>()
                    .Property(e => e.Id);

                MapColumnTypes<MappedDataTypesWithIdentity>(modelBuilder);

                modelBuilder.Entity<MappedDataTypesWithIdentity>(
                    e
                        =>
                    {
                        e.Property(m => m.Bigint).HasColumnType("NUMBER(19)");
                        e.Property(m => m.Tinyint).HasColumnType("NUMBER(3)");
                        e.Property(m => m.Bit).HasColumnType("NUMBER(1)");
                        e.Property(m => m.Money).HasColumnType("DECIMAL(19,4)");
                        e.Property(m => m.Datetimeoffset).HasColumnType("TIMESTAMP WITH TIME ZONE");
                        e.Property(m => m.Datetime).HasColumnType("TIMESTAMP");
                        e.Property(m => m.Time).HasColumnType("INTERVAL DAY TO SECOND");
                        e.Property(m => m.Text).HasColumnType("CLOB");
                        e.Property(m => m.Ntext).HasColumnType("NCLOB");
                        e.Property(m => m.Image).HasColumnType("BLOB");
                        e.Property(m => m.VarbinaryMax).HasColumnType("RAW(2000)");
                    });

                MapColumnTypes<MappedNullableDataTypesWithIdentity>(modelBuilder);

                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>(
                    e
                        =>
                    {
                        e.Property(m => m.Bigint).HasColumnType("NUMBER(19)");
                        e.Property(m => m.Tinyint).HasColumnType("NUMBER(3)");
                        e.Property(m => m.Bit).HasColumnType("NUMBER(1)");
                        e.Property(m => m.Money).HasColumnType("DECIMAL(19,4)");
                        e.Property(m => m.Datetimeoffset).HasColumnType("TIMESTAMP WITH TIME ZONE");
                        e.Property(m => m.Datetime).HasColumnType("TIMESTAMP");
                        e.Property(m => m.Time).HasColumnType("INTERVAL DAY TO SECOND");
                        e.Property(m => m.Text).HasColumnType("CLOB");
                        e.Property(m => m.Ntext).HasColumnType("NCLOB");
                        e.Property(m => m.Image).HasColumnType("BLOB");
                        e.Property(m => m.VarbinaryMax).HasColumnType("RAW(2000)");
                    });

                MapSizedColumnTypes<MappedSizedDataTypesWithIdentity>(modelBuilder);

                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>(
                    e
                        =>
                    {
                        e.Property(m => m.Binary).HasColumnType("RAW(3)");
                        e.Property(m => m.Varbinary).HasColumnType("RAW(3)");
                    });

                MapSizedColumnTypes<MappedScaledDataTypesWithIdentity>(modelBuilder);

                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>(
                    e
                        =>
                    {
                        e.Property(m => m.Datetimeoffset).HasColumnType("TIMESTAMP(3) WITH TIME ZONE");
                        e.Property(m => m.Datetime2).HasColumnType("TIMESTAMP(3)");
                        e.Property(m => m.Float).HasColumnType("FLOAT(10)");
                    });

                MapPreciseColumnTypes<MappedPrecisionAndScaledDataTypesWithIdentity>(modelBuilder);
            }

            private static void MapColumnTypes<TEntity>(ModelBuilder modelBuilder)
                where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties.Where(p => p.Name != "Id"))
                {
                    var typeName = propertyInfo.Name;

                    if (typeName.EndsWith("Max"))
                    {
                        typeName = typeName.Substring(0, typeName.IndexOf("Max")) + "(2000)";
                    }

                    typeName = typeName.Replace('_', ' ');

                    entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = typeName;
                }
            }

            private static void MapSizedColumnTypes<TEntity>(ModelBuilder modelBuilder)
                where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties
                    .Where(p => p.Name != "Id" && p.Name != "Int"))
                {
                    entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = propertyInfo.Name.Replace('_', ' ') + "(3)";
                }
            }

            private static void MapPreciseColumnTypes<TEntity>(ModelBuilder modelBuilder)
                where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties
                    .Where(p => p.Name != "Id" && p.Name != "Int"))
                {
                    entityType.GetOrAddProperty(propertyInfo).Relational().ColumnType = propertyInfo.Name.Replace('_', ' ') + "(5, 2)";
                }
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning));

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }

        protected class MappedDataTypes
        {
            public int Int { get; set; }
            public long Bigint { get; set; }
            public short Smallint { get; set; }
            public byte Tinyint { get; set; }
            public bool Bit { get; set; }

            public decimal Money { get; set; }

            //public decimal Smallmoney { get; set; }
            public double Float { get; set; }

            public float Real { get; set; }
            public double Double_precision { get; set; }
            public DateTime Date { get; set; }

            public DateTimeOffset Datetimeoffset { get; set; }

            //public DateTime Datetime2 { get; set; }
            //public DateTime Smalldatetime { get; set; }
            public DateTime Datetime { get; set; }

            public TimeSpan Time { get; set; }
            public string Varchar2Max { get; set; }
            public string Char_varyingMax { get; set; }
            public string Character_varyingMax { get; set; }
            public string Nvarchar2Max { get; set; }
            public string National_char_varyingMax { get; set; }
            public string National_character_varyingMax { get; set; }
            public string Text { get; set; }
            public string Ntext { get; set; }

            public byte[] VarbinaryMax { get; set; }

            //public byte[] Binary_varyingMax { get; set; }
            public byte[] Image { get; set; }

            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
        }

        protected class MappedSizedDataTypes
        {
            public int Id { get; set; }
            public string Char { get; set; }
            public string Character { get; set; }
            public string Varchar2 { get; set; }
            public string Char_varying { get; set; }
            public string Character_varying { get; set; }
            public string Nchar { get; set; }
            public string National_character { get; set; }
            public string Nvarchar2 { get; set; }
            public string National_char_varying { get; set; }
            public string National_character_varying { get; set; }
            public byte[] Binary { get; set; }

            public byte[] Varbinary { get; set; }
            //public byte[] Binary_varying { get; set; }
        }

        protected class MappedScaledDataTypes
        {
            public int Id { get; set; }

            public float Float { get; set; }

            //public float Double_precision { get; set; }
            public DateTimeOffset Datetimeoffset { get; set; }

            public DateTime Datetime2 { get; set; }
            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypes
        {
            public int Id { get; set; }
            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
        }

        protected class MappedNullableDataTypes
        {
            public int? Int { get; set; }
            public long? Bigint { get; set; }
            public short? Smallint { get; set; }
            public byte? Tinyint { get; set; }
            public bool? Bit { get; set; }

            public decimal? Money { get; set; }

            //public decimal? Smallmoney { get; set; }
            public double? Float { get; set; }

            public float? Real { get; set; }

            public double? Double_precision { get; set; }

            //public DateTime? Date { get; set; }
            public DateTimeOffset? Datetimeoffset { get; set; }

            //public DateTime? Datetime2 { get; set; }
            //public DateTime? Smalldatetime { get; set; }
            public DateTime? Datetime { get; set; }

            public TimeSpan? Time { get; set; }
            public string Varchar2Max { get; set; }
            public string Char_varyingMax { get; set; }
            public string Character_varyingMax { get; set; }
            public string Nvarchar2Max { get; set; }
            public string National_char_varyingMax { get; set; }
            public string National_character_varyingMax { get; set; }
            public string Text { get; set; }
            public string Ntext { get; set; }

            public byte[] VarbinaryMax { get; set; }

            //public byte[] Binary_varyingMax { get; set; }
            public byte[] Image { get; set; }

            public decimal? Decimal { get; set; }
            public decimal? Dec { get; set; }
            public decimal? Numeric { get; set; }
        }

        protected class MappedDataTypesWithIdentity
        {
            public int Id { get; set; }

            public int Int { get; set; }
            public long Bigint { get; set; }
            public short Smallint { get; set; }
            public byte Tinyint { get; set; }
            public bool Bit { get; set; }

            public decimal Money { get; set; }

            //public decimal Smallmoney { get; set; }
            public double Float { get; set; }

            public float Real { get; set; }
            public double Double_precision { get; set; }
            public DateTime Date { get; set; }

            public DateTimeOffset Datetimeoffset { get; set; }

            //public DateTime Datetime2 { get; set; }
            //public DateTime Smalldatetime { get; set; }
            public DateTime Datetime { get; set; }

            public TimeSpan Time { get; set; }
            public string Varchar2Max { get; set; }
            public string Char_varyingMax { get; set; }
            public string Character_varyingMax { get; set; }
            public string Nvarchar2Max { get; set; }
            public string National_char_varyingMax { get; set; }
            public string National_character_varyingMax { get; set; }
            public string Text { get; set; }
            public string Ntext { get; set; }

            public byte[] VarbinaryMax { get; set; }

            //public byte[] Binary_varyingMax { get; set; }
            public byte[] Image { get; set; }

            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
        }

        protected class MappedSizedDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            public string Char { get; set; }
            public string Character { get; set; }
            public string Varchar2 { get; set; }
            public string Char_varying { get; set; }
            public string Character_varying { get; set; }
            public string Nchar { get; set; }
            public string National_character { get; set; }
            public string Nvarchar2 { get; set; }
            public string National_char_varying { get; set; }
            public string National_character_varying { get; set; }
            public byte[] Binary { get; set; }

            public byte[] Varbinary { get; set; }
            //public byte[] Binary_varying { get; set; }
        }

        protected class MappedScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            public float Float { get; set; }

            //public float Double_precision { get; set; }
            public DateTimeOffset Datetimeoffset { get; set; }

            public DateTime Datetime2 { get; set; }
            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            public decimal Decimal { get; set; }
            public decimal Dec { get; set; }
            public decimal Numeric { get; set; }
        }

        protected class MappedNullableDataTypesWithIdentity
        {
            public int Id { get; set; }

            public int? Int { get; set; }
            public long? Bigint { get; set; }
            public short? Smallint { get; set; }
            public byte? Tinyint { get; set; }
            public bool? Bit { get; set; }

            public decimal? Money { get; set; }

            //public decimal? Smallmoney { get; set; }
            public double? Float { get; set; }

            public float? Real { get; set; }

            public double? Double_precision { get; set; }

            //public DateTime? Date { get; set; }
            public DateTimeOffset? Datetimeoffset { get; set; }

            //public DateTime? Datetime2 { get; set; }
            //public DateTime? Smalldatetime { get; set; }
            public DateTime? Datetime { get; set; }

            public TimeSpan? Time { get; set; }
            public string Varchar2Max { get; set; }
            public string Char_varyingMax { get; set; }
            public string Character_varyingMax { get; set; }
            public string Nvarchar2Max { get; set; }
            public string National_char_varyingMax { get; set; }
            public string National_character_varyingMax { get; set; }
            public string Text { get; set; }
            public string Ntext { get; set; }

            public byte[] VarbinaryMax { get; set; }

            //public byte[] Binary_varyingMax { get; set; }
            public byte[] Image { get; set; }

            public decimal? Decimal { get; set; }
            public decimal? Dec { get; set; }
            public decimal? Numeric { get; set; }
        }

        protected class ColumnInfo
        {
            public string TableName { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public bool? IsNullable { get; set; }
            public int? MaxLength { get; set; }
            public int? NumericPrecision { get; set; }
            public int? NumericScale { get; set; }
            public int? DateTimePrecision { get; set; }
        }
    }
}
