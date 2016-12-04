// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

// ReSharper disable UnusedParameter.Local
// ReSharper disable PossibleInvalidOperationException
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public class BuiltInDataTypesSqlServerTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqlServerFixture>
    {
        public BuiltInDataTypesSqlServerTest(BuiltInDataTypesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public virtual void Can_perform_query_with_ansi_strings()
        {
            Can_perform_query_with_ansi_strings(supportsAnsi: true);
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_constant()
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
                    @"SELECT [e].[Int]
FROM [MappedNullableDataTypes] AS [e]
WHERE [e].[Time] = '00:01:02'",
                    Sql);
            }
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_parameter()
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
                    @"@__timeSpan_0: 02:01:00

SELECT [e].[Int]
FROM [MappedNullableDataTypes] AS [e]
WHERE [e].[Time] = @__timeSpan_0",
                    Sql);
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
                        Smallmoney = 82.2m,
                        Float = 83.3,
                        Real = 84.4f,
                        Double_precision = 85.5,
                        Date = new DateTime(2015, 1, 2, 10, 11, 12),
                        Datetimeoffset = new DateTimeOffset(new DateTime(), TimeSpan.Zero),
                        Datetime2 = new DateTime(),
                        Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                        Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                        Time = new TimeSpan(11, 15, 12),
                        VarcharMax = "C",
                        Char_varyingMax = "Your",
                        Character_varyingMax = "strong",
                        NvarcharMax = "don't",
                        National_char_varyingMax = "help",
                        National_character_varyingMax = "anyone!",
                        Text = "Gumball Rules!",
                        Ntext = "Gumball Rules OK!",
                        VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                        Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
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

                decimal? param6 = 82.2m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Smallmoney == param6));

                double? param7a = 83.3;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Float == param7a));

                float? param7b = 84.4f;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Real == param7b));

                double? param7c = 85.5;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Double_precision == param7c));

                DateTime? param8 = new DateTime(2015, 1, 2);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Date == param8));

                DateTimeOffset? param9 = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Datetimeoffset == param9));

                DateTime? param10 = new DateTime();
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Datetime2 == param10));

                DateTime? param11 = new DateTime(2019, 1, 2, 14, 11, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Datetime == param11));

                DateTime? param12 = new DateTime(2018, 1, 2, 13, 11, 0);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Smalldatetime == param12));

                TimeSpan? param13 = new TimeSpan(11, 15, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Time == param13));

                var param19 = "C";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.VarcharMax == param19));

                var param20 = "Your";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Char_varyingMax == param20));

                var param21 = "strong";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Character_varyingMax == param21));

                var param27 = "don't";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.NvarcharMax == param27));

                var param28 = "help";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_char_varyingMax == param28));

                var param29 = "anyone!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_character_varyingMax == param29));

                var param35 = new byte[] { 89, 90, 91, 92 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.VarbinaryMax == param35));

                var param36 = new byte[] { 93, 94, 95, 96 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Binary_varyingMax == param36));

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

                decimal? param6 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Smallmoney == param6));

                double? param7a = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Float == param7a));

                float? param7b = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Real == param7b));

                double? param7c = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Double_precision == param7c));

                DateTime? param8 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Date == param8));

                DateTimeOffset? param9 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Datetimeoffset == param9));

                DateTime? param10 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Datetime2 == param10));

                DateTime? param11 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Datetime == param11));

                DateTime? param12 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Smalldatetime == param12));

                TimeSpan? param13 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Time == param13));

                string param19 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.VarcharMax == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Char_varyingMax == param20));

                string param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Character_varyingMax == param21));

                string param27 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.NvarcharMax == param27));

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

                byte[] param36 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Binary_varyingMax == param36));

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
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 77
@p1: 78
@p2: 0x5D5E5F60 (Nullable = false) (Size = 8000)
@p3: True
@p4: Your (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p5: strong (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p6: 01/02/2015 10:11:12 (DbType = DateTime)
@p7: 01/02/2019 14:11:12 (DbType = DateTime)
@p8: 01/02/2017 12:11:12
@p9: 01/02/2016 11:11:12 +00:00
@p10: 102.2
@p11: 101.1
@p12: 85.5
@p13: 83.3
@p14: 0x61626364 (Nullable = false) (Size = 8000)
@p15: 81.1
@p16: help (Nullable = false) (Size = 4000)
@p17: anyone! (Nullable = false) (Size = 4000)
@p18: Gumball Rules OK! (Nullable = false) (Size = 4000)
@p19: 103.3
@p20: don't (Nullable = false) (Size = 4000)
@p21: 84.4
@p22: 01/02/2018 13:11:12 (DbType = DateTime)
@p23: 79
@p24: 82.2
@p25: Gumball Rules! (Nullable = false) (Size = 8000) (DbType = AnsiString)
@p26: 11:15:12
@p27: 80 (Size = 1)
@p28: 0x595A5B5C (Nullable = false) (Size = 8000)
@p29: C (Nullable = false) (Size = 8000) (DbType = AnsiString)",
                parameters);

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == 77), 77);
            }
        }

        private static string DumpParameters()
            => string.Join(
                FileLineEnding,
                TestSqlLoggerFactory.CommandLogData.Single().Parameters
                    .Select(p => p.Name + ": " + p.FormatParameter(quoteValues: false)));

        private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.Bigint);
            Assert.Equal(79, entity.Smallint);
            Assert.Equal(80, entity.Tinyint);
            Assert.Equal(true, entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal("C", entity.VarcharMax);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal("don't", entity.NvarcharMax);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
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
                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                VarcharMax = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                NvarcharMax = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 77
@p1: 78 (Nullable = true)
@p2: 0x5D5E5F60 (Size = 8000)
@p3: True (Nullable = true)
@p4: Your (Size = 8000) (DbType = AnsiString)
@p5: strong (Size = 8000) (DbType = AnsiString)
@p6: 01/02/2015 10:11:12 (Nullable = true) (DbType = DateTime)
@p7: 01/02/2019 14:11:12 (Nullable = true) (DbType = DateTime)
@p8: 01/02/2017 12:11:12 (Nullable = true)
@p9: 01/02/2016 11:11:12 +00:00 (Nullable = true)
@p10: 102.2 (Nullable = true)
@p11: 101.1 (Nullable = true)
@p12: 85.5 (Nullable = true)
@p13: 83.3 (Nullable = true)
@p14: 0x61626364 (Size = 8000)
@p15: 81.1 (Nullable = true)
@p16: help (Size = 4000)
@p17: anyone! (Size = 4000)
@p18: Gumball Rules OK! (Size = 4000)
@p19: 103.3 (Nullable = true)
@p20: don't (Size = 4000)
@p21: 84.4 (Nullable = true)
@p22: 01/02/2018 13:11:12 (Nullable = true) (DbType = DateTime)
@p23: 79 (Nullable = true)
@p24: 82.2 (Nullable = true)
@p25: Gumball Rules! (Size = 8000) (DbType = AnsiString)
@p26: 11:15:12 (Nullable = true)
@p27: 80 (Nullable = true) (Size = 1)
@p28: 0x595A5B5C (Size = 8000)
@p29: C (Size = 8000) (DbType = AnsiString)",
                parameters);

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
            Assert.Equal(true, entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal("C", entity.VarcharMax);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal("don't", entity.NvarcharMax);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
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
                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                VarcharMax = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                NvarcharMax = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 78
@p1:  (DbType = Int64)
@p2:  (Size = 8000) (DbType = Binary)
@p3:  (DbType = String)
@p4:  (Size = 8000)
@p5:  (Size = 8000)
@p6:  (DbType = DateTime)
@p7:  (DbType = DateTime)
@p8:  (DbType = DateTime2)
@p9:  (DbType = String)
@p10:  (DbType = String)
@p11:  (DbType = String)
@p12:  (DbType = String)
@p13:  (DbType = String)
@p14:  (Size = 8000) (DbType = Binary)
@p15:  (DbType = String)
@p16:  (Size = 4000) (DbType = String)
@p17:  (Size = 4000) (DbType = String)
@p18:  (Size = 4000) (DbType = String)
@p19:  (DbType = String)
@p20:  (Size = 4000) (DbType = String)
@p21:  (DbType = String)
@p22:  (DbType = DateTime)
@p23:  (DbType = Int16)
@p24:  (DbType = String)
@p25:  (Size = 8000)
@p26:  (DbType = String)
@p27:  (DbType = Byte)
@p28:  (Size = 8000) (DbType = Binary)
@p29:  (Size = 8000)",
                parameters);

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
            Assert.Null(entity.Smallmoney);
            Assert.Null(entity.Float);
            Assert.Null(entity.Real);
            Assert.Null(entity.Double_precision);
            Assert.Null(entity.Date);
            Assert.Null(entity.Datetimeoffset);
            Assert.Null(entity.Datetime2);
            Assert.Null(entity.Smalldatetime);
            Assert.Null(entity.Datetime);
            Assert.Null(entity.Time);
            Assert.Null(entity.VarcharMax);
            Assert.Null(entity.Char_varyingMax);
            Assert.Null(entity.Character_varyingMax);
            Assert.Null(entity.NvarcharMax);
            Assert.Null(entity.National_char_varyingMax);
            Assert.Null(entity.National_character_varyingMax);
            Assert.Null(entity.Text);
            Assert.Null(entity.Ntext);
            Assert.Null(entity.VarbinaryMax);
            Assert.Null(entity.Binary_varyingMax);
            Assert.Null(entity.Image);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.Dec);
            Assert.Null(entity.Numeric);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(CreateMappedSizedDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 77
@p1: 0x0A0B0C (Size = 3)
@p2: 0x0C0D0E (Size = 3)
@p3: Wor (Size = 3) (DbType = AnsiStringFixedLength)
@p4: Thr (Size = 3) (DbType = AnsiString)
@p5: Lon (Size = 3) (DbType = AnsiStringFixedLength)
@p6: Let (Size = 3) (DbType = AnsiString)
@p7: The (Size = 3)
@p8: Squ (Size = 3) (DbType = StringFixedLength)
@p9: Col (Size = 3)
@p10: Won (Size = 3) (DbType = StringFixedLength)
@p11: Int (Size = 3)
@p12: 0x0B0C0D (Size = 3)
@p13: Tha (Size = 3) (DbType = AnsiString)",
                parameters);

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
            Assert.Equal("Tha", entity.Varchar);
            Assert.Equal("Thr", entity.Char_varying);
            Assert.Equal("Let", entity.Character_varying);
            Assert.Equal("Won", entity.Nchar);
            Assert.Equal("Squ", entity.National_character);
            Assert.Equal("Int", entity.Nvarchar);
            Assert.Equal("The", entity.National_char_varying);
            Assert.Equal("Col", entity.National_character_varying);
            Assert.Equal(new byte[] { 10, 11, 12 }, entity.Binary);
            Assert.Equal(new byte[] { 11, 12, 13 }, entity.Varbinary);
            Assert.Equal(new byte[] { 12, 13, 14 }, entity.Binary_varying);
        }

        private static MappedSizedDataTypes CreateMappedSizedDataTypes(int id)
            => new MappedSizedDataTypes
            {
                Id = id,
                Char = "Wor",
                Character = "Lon",
                Varchar = "Tha",
                Char_varying = "Thr",
                Character_varying = "Let",
                Nchar = "Won",
                National_character = "Squ",
                Nvarchar = "Int",
                National_char_varying = "The",
                National_character_varying = "Col",
                Binary = new byte[] { 10, 11, 12 },
                Varbinary = new byte[] { 11, 12, 13 },
                Binary_varying = new byte[] { 12, 13, 14 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(new MappedSizedDataTypes { Id = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 78
@p1:  (Size = 3) (DbType = Binary)
@p2:  (Size = 3) (DbType = Binary)
@p3:  (Size = 3) (DbType = AnsiStringFixedLength)
@p4:  (Size = 3)
@p5:  (Size = 3) (DbType = AnsiStringFixedLength)
@p6:  (Size = 3)
@p7:  (Size = 3) (DbType = String)
@p8:  (Size = 3) (DbType = StringFixedLength)
@p9:  (Size = 3) (DbType = String)
@p10:  (Size = 3) (DbType = StringFixedLength)
@p11:  (Size = 3) (DbType = String)
@p12:  (Size = 3) (DbType = Binary)
@p13:  (Size = 3)",
                parameters);

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
            Assert.Null(entity.Varchar);
            Assert.Null(entity.Char_varying);
            Assert.Null(entity.Character_varying);
            Assert.Null(entity.Nchar);
            Assert.Null(entity.National_character);
            Assert.Null(entity.Nvarchar);
            Assert.Null(entity.National_char_varying);
            Assert.Null(entity.National_character_varying);
            Assert.Null(entity.Binary);
            Assert.Null(entity.Varbinary);
            Assert.Null(entity.Binary_varying);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(CreateMappedScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 77
@p1: 01/02/2017 12:11:12
@p2: 01/02/2016 11:11:12 +00:00
@p3: 102.2
@p4: 101.1
@p5: 85.5
@p6: 83.3
@p7: 103.3",
                parameters);

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypes(context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypes(MappedScaledDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Id);
            Assert.Equal(83.3f, entity.Float);
            Assert.Equal(85.5f, entity.Double_precision);
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
                Double_precision = 85.5f,
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(CreateMappedPrecisionAndScaledDataTypes(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 77
@p1: 102.2
@p2: 101.1
@p3: 103.3",
                parameters);

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
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypesWithIdentity>().Add(CreateMappedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 78
@p1: 0x5D5E5F60 (Size = 8000)
@p2: True
@p3: Your (Size = 8000) (DbType = AnsiString)
@p4: strong (Size = 8000) (DbType = AnsiString)
@p5: 01/02/2015 10:11:12 (DbType = DateTime)
@p6: 01/02/2019 14:11:12 (DbType = DateTime)
@p7: 01/02/2017 12:11:12
@p8: 01/02/2016 11:11:12 +00:00
@p9: 102.2
@p10: 101.1
@p11: 85.5
@p12: 83.3
@p13: 0x61626364 (Size = 8000)
@p14: 77
@p15: 81.1
@p16: help (Size = 4000)
@p17: anyone! (Size = 4000)
@p18: Gumball Rules OK! (Size = 4000)
@p19: 103.3
@p20: don't (Size = 4000)
@p21: 84.4
@p22: 01/02/2018 13:11:12 (DbType = DateTime)
@p23: 79
@p24: 82.2
@p25: Gumball Rules! (Size = 8000) (DbType = AnsiString)
@p26: 11:15:12
@p27: 80 (Size = 1)
@p28: 0x595A5B5C (Size = 8000)
@p29: C (Size = 8000) (DbType = AnsiString)",
                parameters);

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
            Assert.Equal(true, entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal("C", entity.VarcharMax);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal("don't", entity.NvarcharMax);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
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
                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                VarcharMax = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                NvarcharMax = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_with_identity()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(CreateMappedNullableDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 78 (Nullable = true)
@p1: 0x5D5E5F60 (Size = 8000)
@p2: True (Nullable = true)
@p3: Your (Size = 8000) (DbType = AnsiString)
@p4: strong (Size = 8000) (DbType = AnsiString)
@p5: 01/02/2015 10:11:12 (Nullable = true) (DbType = DateTime)
@p6: 01/02/2019 14:11:12 (Nullable = true) (DbType = DateTime)
@p7: 01/02/2017 12:11:12 (Nullable = true)
@p8: 01/02/2016 11:11:12 +00:00 (Nullable = true)
@p9: 102.2 (Nullable = true)
@p10: 101.1 (Nullable = true)
@p11: 85.5 (Nullable = true)
@p12: 83.3 (Nullable = true)
@p13: 0x61626364 (Size = 8000)
@p14: 77 (Nullable = true)
@p15: 81.1 (Nullable = true)
@p16: help (Size = 4000)
@p17: anyone! (Size = 4000)
@p18: Gumball Rules OK! (Size = 4000)
@p19: 103.3 (Nullable = true)
@p20: don't (Size = 4000)
@p21: 84.4 (Nullable = true)
@p22: 01/02/2018 13:11:12 (Nullable = true) (DbType = DateTime)
@p23: 79 (Nullable = true)
@p24: 82.2 (Nullable = true)
@p25: Gumball Rules! (Size = 8000) (DbType = AnsiString)
@p26: 11:15:12 (Nullable = true)
@p27: 80 (Nullable = true) (Size = 1)
@p28: 0x595A5B5C (Size = 8000)
@p29: C (Size = 8000) (DbType = AnsiString)",
                parameters);

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
            Assert.Equal(true, entity.Bit);
            Assert.Equal(81.1m, entity.Money);
            Assert.Equal(82.2m, entity.Smallmoney);
            Assert.Equal(83.3, entity.Float);
            Assert.Equal(84.4f, entity.Real);
            Assert.Equal(85.5, entity.Double_precision);
            Assert.Equal(new DateTime(2015, 1, 2), entity.Date);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
            Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
            Assert.Equal(new DateTime(2018, 1, 2, 13, 11, 00), entity.Smalldatetime);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.Datetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.Time);
            Assert.Equal("C", entity.VarcharMax);
            Assert.Equal("Your", entity.Char_varyingMax);
            Assert.Equal("strong", entity.Character_varyingMax);
            Assert.Equal("don't", entity.NvarcharMax);
            Assert.Equal("help", entity.National_char_varyingMax);
            Assert.Equal("anyone!", entity.National_character_varyingMax);
            Assert.Equal("Gumball Rules!", entity.Text);
            Assert.Equal("Gumball Rules OK!", entity.Ntext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
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
                Smallmoney = 82.2m,
                Float = 83.3,
                Real = 84.4f,
                Double_precision = 85.5,
                Date = new DateTime(2015, 1, 2, 10, 11, 12),
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Smalldatetime = new DateTime(2018, 1, 2, 13, 11, 12),
                Datetime = new DateTime(2019, 1, 2, 14, 11, 12),
                Time = new TimeSpan(11, 15, 12),
                VarcharMax = "C",
                Char_varyingMax = "Your",
                Character_varyingMax = "strong",
                NvarcharMax = "don't",
                National_char_varyingMax = "help",
                National_character_varyingMax = "anyone!",
                Text = "Gumball Rules!",
                Ntext = "Gumball Rules OK!",
                VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                Image = new byte[] { 97, 98, 99, 100 },
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_with_identity()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypesWithIdentity>().Add(new MappedNullableDataTypesWithIdentity { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0:  (DbType = Int64)
@p1:  (Size = 8000) (DbType = Binary)
@p2:  (DbType = String)
@p3:  (Size = 8000)
@p4:  (Size = 8000)
@p5:  (DbType = DateTime)
@p6:  (DbType = DateTime)
@p7:  (DbType = DateTime2)
@p8:  (DbType = String)
@p9:  (DbType = String)
@p10:  (DbType = String)
@p11:  (DbType = String)
@p12:  (DbType = String)
@p13:  (Size = 8000) (DbType = Binary)
@p14: 78 (Nullable = true)
@p15:  (DbType = String)
@p16:  (Size = 4000) (DbType = String)
@p17:  (Size = 4000) (DbType = String)
@p18:  (Size = 4000) (DbType = String)
@p19:  (DbType = String)
@p20:  (Size = 4000) (DbType = String)
@p21:  (DbType = String)
@p22:  (DbType = DateTime)
@p23:  (DbType = Int16)
@p24:  (DbType = String)
@p25:  (Size = 8000)
@p26:  (DbType = String)
@p27:  (DbType = Byte)
@p28:  (Size = 8000) (DbType = Binary)
@p29:  (Size = 8000)",
                parameters);

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
            Assert.Null(entity.Smallmoney);
            Assert.Null(entity.Float);
            Assert.Null(entity.Real);
            Assert.Null(entity.Double_precision);
            Assert.Null(entity.Date);
            Assert.Null(entity.Datetimeoffset);
            Assert.Null(entity.Datetime2);
            Assert.Null(entity.Smalldatetime);
            Assert.Null(entity.Datetime);
            Assert.Null(entity.Time);
            Assert.Null(entity.VarcharMax);
            Assert.Null(entity.Char_varyingMax);
            Assert.Null(entity.Character_varyingMax);
            Assert.Null(entity.NvarcharMax);
            Assert.Null(entity.National_char_varyingMax);
            Assert.Null(entity.National_character_varyingMax);
            Assert.Null(entity.Text);
            Assert.Null(entity.Ntext);
            Assert.Null(entity.VarbinaryMax);
            Assert.Null(entity.Binary_varyingMax);
            Assert.Null(entity.Image);
            Assert.Null(entity.Decimal);
            Assert.Null(entity.Dec);
            Assert.Null(entity.Numeric);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types_with_identity()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(CreateMappedSizedDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 0x0A0B0C (Size = 3)
@p1: 0x0C0D0E (Size = 3)
@p2: Wor (Size = 3) (DbType = AnsiStringFixedLength)
@p3: Thr (Size = 3) (DbType = AnsiString)
@p4: Lon (Size = 3) (DbType = AnsiStringFixedLength)
@p5: Let (Size = 3) (DbType = AnsiString)
@p6: 77
@p7: The (Size = 3)
@p8: Squ (Size = 3) (DbType = StringFixedLength)
@p9: Col (Size = 3)
@p10: Won (Size = 3) (DbType = StringFixedLength)
@p11: Int (Size = 3)
@p12: 0x0B0C0D (Size = 3)
@p13: Tha (Size = 3) (DbType = AnsiString)",
                parameters);

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
            Assert.Equal("Tha", entity.Varchar);
            Assert.Equal("Thr", entity.Char_varying);
            Assert.Equal("Let", entity.Character_varying);
            Assert.Equal("Won", entity.Nchar);
            Assert.Equal("Squ", entity.National_character);
            Assert.Equal("Int", entity.Nvarchar);
            Assert.Equal("The", entity.National_char_varying);
            Assert.Equal("Col", entity.National_character_varying);
            Assert.Equal(new byte[] { 10, 11, 12 }, entity.Binary);
            Assert.Equal(new byte[] { 11, 12, 13 }, entity.Varbinary);
            Assert.Equal(new byte[] { 12, 13, 14 }, entity.Binary_varying);
        }

        private static MappedSizedDataTypesWithIdentity CreateMappedSizedDataTypesWithIdentity(int id)
            => new MappedSizedDataTypesWithIdentity
            {
                Int = id,
                Char = "Wor",
                Character = "Lon",
                Varchar = "Tha",
                Char_varying = "Thr",
                Character_varying = "Let",
                Nchar = "Won",
                National_character = "Squ",
                Nvarchar = "Int",
                National_char_varying = "The",
                National_character_varying = "Col",
                Binary = new byte[] { 10, 11, 12 },
                Varbinary = new byte[] { 11, 12, 13 },
                Binary_varying = new byte[] { 12, 13, 14 }
            };

        [Fact]
        public virtual void Can_insert_and_read_back_nulls_for_all_mapped_sized_data_types_with_identity()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypesWithIdentity>().Add(new MappedSizedDataTypesWithIdentity { Int = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0:  (Size = 3) (DbType = Binary)
@p1:  (Size = 3) (DbType = Binary)
@p2:  (Size = 3) (DbType = AnsiStringFixedLength)
@p3:  (Size = 3)
@p4:  (Size = 3) (DbType = AnsiStringFixedLength)
@p5:  (Size = 3)
@p6: 78
@p7:  (Size = 3) (DbType = String)
@p8:  (Size = 3) (DbType = StringFixedLength)
@p9:  (Size = 3) (DbType = String)
@p10:  (Size = 3) (DbType = StringFixedLength)
@p11:  (Size = 3) (DbType = String)
@p12:  (Size = 3) (DbType = Binary)
@p13:  (Size = 3)",
                parameters);

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
            Assert.Null(entity.Varchar);
            Assert.Null(entity.Char_varying);
            Assert.Null(entity.Character_varying);
            Assert.Null(entity.Nchar);
            Assert.Null(entity.National_character);
            Assert.Null(entity.Nvarchar);
            Assert.Null(entity.National_char_varying);
            Assert.Null(entity.National_character_varying);
            Assert.Null(entity.Binary);
            Assert.Null(entity.Varbinary);
            Assert.Null(entity.Binary_varying);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale_with_identity()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypesWithIdentity>().Add(CreateMappedScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 01/02/2017 12:11:12
@p1: 01/02/2016 11:11:12 +00:00
@p2: 102.2
@p3: 101.1
@p4: 85.5
@p5: 83.3
@p6: 77
@p7: 103.3",
                parameters);

            using (var context = CreateContext())
            {
                AssertMappedScaledDataTypesWithIdentity(context.Set<MappedScaledDataTypesWithIdentity>().Single(e => e.Int == 77), 77);
            }
        }

        private static void AssertMappedScaledDataTypesWithIdentity(MappedScaledDataTypesWithIdentity entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(83.3f, entity.Float);
            Assert.Equal(85.5f, entity.Double_precision);
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
                Double_precision = 85.5f,
                Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                Decimal = 101.1m,
                Dec = 102.2m,
                Numeric = 103.3m
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale_with_identity()
        {
            TestSqlLoggerFactory.Reset();
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypesWithIdentity>().Add(
                    CreateMappedPrecisionAndScaledDataTypesWithIdentity(77));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
            Assert.Equal(
                @"@p0: 102.2
@p1: 101.1
@p2: 77
@p3: 103.3",
                parameters);

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
                        IS_NULLABLE,
                        CHARACTER_MAXIMUM_LENGTH,
                        NUMERIC_PRECISION,
                        NUMERIC_SCALE,
                        DATETIME_PRECISION
                    FROM INFORMATION_SCHEMA.COLUMNS";

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
                            NumericPrecision = reader.IsDBNull(5) ? null : (int?)reader.GetByte(5),
                            NumericScale = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            DateTimePrecision = reader.IsDBNull(7) ? null : (int?)reader.GetInt16(7)
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

            var actual = builder.ToString().Replace(Environment.NewLine, FileLineEnding);

            const string expected = @"BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [nullable varbinary] [MaxLength = 900]
BinaryForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
BinaryKeyDataType.Id ---> [varbinary] [MaxLength = 900]
BuiltInDataTypes.Enum16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.Enum32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.Enum64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.Enum8 ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestBoolean ---> [bit]
BuiltInDataTypes.TestByte ---> [tinyint] [Precision = 3 Scale = 0]
BuiltInDataTypes.TestDateTime ---> [datetime2] [Precision = 7]
BuiltInDataTypes.TestDateTimeOffset ---> [datetimeoffset] [Precision = 7]
BuiltInDataTypes.TestDecimal ---> [decimal] [Precision = 18 Scale = 2]
BuiltInDataTypes.TestDouble ---> [float] [Precision = 53]
BuiltInDataTypes.TestInt16 ---> [smallint] [Precision = 5 Scale = 0]
BuiltInDataTypes.TestInt32 ---> [int] [Precision = 10 Scale = 0]
BuiltInDataTypes.TestInt64 ---> [bigint] [Precision = 19 Scale = 0]
BuiltInDataTypes.TestSingle ---> [real] [Precision = 24]
BuiltInDataTypes.TestTimeSpan ---> [time] [Precision = 7]
BuiltInNullableDataTypes.Enum16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.Enum32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.Enum64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.Enum8 ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.PartitionId ---> [int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestByteArray ---> [nullable varbinary] [MaxLength = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [nullable bit]
BuiltInNullableDataTypes.TestNullableByte ---> [nullable tinyint] [Precision = 3 Scale = 0]
BuiltInNullableDataTypes.TestNullableDateTime ---> [nullable datetime2] [Precision = 7]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [nullable datetimeoffset] [Precision = 7]
BuiltInNullableDataTypes.TestNullableDecimal ---> [nullable decimal] [Precision = 18 Scale = 2]
BuiltInNullableDataTypes.TestNullableDouble ---> [nullable float] [Precision = 53]
BuiltInNullableDataTypes.TestNullableInt16 ---> [nullable smallint] [Precision = 5 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt32 ---> [nullable int] [Precision = 10 Scale = 0]
BuiltInNullableDataTypes.TestNullableInt64 ---> [nullable bigint] [Precision = 19 Scale = 0]
BuiltInNullableDataTypes.TestNullableSingle ---> [nullable real] [Precision = 24]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [nullable time] [Precision = 7]
BuiltInNullableDataTypes.TestString ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypes.Bigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypes.Binary_varyingMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.Bit ---> [bit]
MappedDataTypes.Char_varyingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.Character_varyingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.Date ---> [date] [Precision = 0]
MappedDataTypes.Datetime ---> [datetime] [Precision = 3]
MappedDataTypes.Datetime2 ---> [datetime2] [Precision = 7]
MappedDataTypes.Datetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedDataTypes.Dec ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypes.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypes.Double_precision ---> [float] [Precision = 53]
MappedDataTypes.Float ---> [float] [Precision = 53]
MappedDataTypes.Image ---> [image] [MaxLength = 2147483647]
MappedDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedDataTypes.Money ---> [money] [Precision = 19 Scale = 4]
MappedDataTypes.National_char_varyingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.National_character_varyingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.Ntext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypes.Numeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypes.NvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.Real ---> [real] [Precision = 24]
MappedDataTypes.Smalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypes.Smallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypes.Smallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypes.Text ---> [text] [MaxLength = 2147483647]
MappedDataTypes.Time ---> [time] [Precision = 7]
MappedDataTypes.Tinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypes.VarbinaryMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.VarcharMax ---> [varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.Bigint ---> [bigint] [Precision = 19 Scale = 0]
MappedDataTypesWithIdentity.Binary_varyingMax ---> [nullable varbinary] [MaxLength = -1]
MappedDataTypesWithIdentity.Bit ---> [bit]
MappedDataTypesWithIdentity.Char_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.Character_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedDataTypesWithIdentity.Date ---> [date] [Precision = 0]
MappedDataTypesWithIdentity.Datetime ---> [datetime] [Precision = 3]
MappedDataTypesWithIdentity.Datetime2 ---> [datetime2] [Precision = 7]
MappedDataTypesWithIdentity.Datetimeoffset ---> [datetimeoffset] [Precision = 7]
MappedDataTypesWithIdentity.Dec ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.Decimal ---> [decimal] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.Double_precision ---> [float] [Precision = 53]
MappedDataTypesWithIdentity.Float ---> [float] [Precision = 53]
MappedDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.Image ---> [nullable image] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedDataTypesWithIdentity.Money ---> [money] [Precision = 19 Scale = 4]
MappedDataTypesWithIdentity.National_char_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.National_character_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.Ntext ---> [nullable ntext] [MaxLength = 1073741823]
MappedDataTypesWithIdentity.Numeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypesWithIdentity.NvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedDataTypesWithIdentity.Real ---> [real] [Precision = 24]
MappedDataTypesWithIdentity.Smalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypesWithIdentity.Smallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypesWithIdentity.Smallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypesWithIdentity.Text ---> [nullable text] [MaxLength = 2147483647]
MappedDataTypesWithIdentity.Time ---> [time] [Precision = 7]
MappedDataTypesWithIdentity.Tinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypesWithIdentity.VarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedDataTypesWithIdentity.VarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.Bigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.Binary_varyingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.Bit ---> [nullable bit]
MappedNullableDataTypes.Char_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.Character_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.Date ---> [nullable date] [Precision = 0]
MappedNullableDataTypes.Datetime ---> [nullable datetime] [Precision = 3]
MappedNullableDataTypes.Datetime2 ---> [nullable datetime2] [Precision = 7]
MappedNullableDataTypes.Datetimeoffset ---> [nullable datetimeoffset] [Precision = 7]
MappedNullableDataTypes.Dec ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypes.Decimal ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypes.Double_precision ---> [nullable float] [Precision = 53]
MappedNullableDataTypes.Float ---> [nullable float] [Precision = 53]
MappedNullableDataTypes.Image ---> [nullable image] [MaxLength = 2147483647]
MappedNullableDataTypes.Int ---> [int] [Precision = 10 Scale = 0]
MappedNullableDataTypes.Money ---> [nullable money] [Precision = 19 Scale = 4]
MappedNullableDataTypes.National_char_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.National_character_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.Ntext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypes.Numeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypes.NvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.Real ---> [nullable real] [Precision = 24]
MappedNullableDataTypes.Smalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypes.Smallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.Smallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypes.Text ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypes.Time ---> [nullable time] [Precision = 7]
MappedNullableDataTypes.Tinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypes.VarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.VarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.Bigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypesWithIdentity.Binary_varyingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.Bit ---> [nullable bit]
MappedNullableDataTypesWithIdentity.Char_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.Character_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.Date ---> [nullable date] [Precision = 0]
MappedNullableDataTypesWithIdentity.Datetime ---> [nullable datetime] [Precision = 3]
MappedNullableDataTypesWithIdentity.Datetime2 ---> [nullable datetime2] [Precision = 7]
MappedNullableDataTypesWithIdentity.Datetimeoffset ---> [nullable datetimeoffset] [Precision = 7]
MappedNullableDataTypesWithIdentity.Dec ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.Decimal ---> [nullable decimal] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.Double_precision ---> [nullable float] [Precision = 53]
MappedNullableDataTypesWithIdentity.Float ---> [nullable float] [Precision = 53]
MappedNullableDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.Image ---> [nullable image] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.Int ---> [nullable int] [Precision = 10 Scale = 0]
MappedNullableDataTypesWithIdentity.Money ---> [nullable money] [Precision = 19 Scale = 4]
MappedNullableDataTypesWithIdentity.National_char_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.National_character_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.Ntext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypesWithIdentity.Numeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypesWithIdentity.NvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.Real ---> [nullable real] [Precision = 24]
MappedNullableDataTypesWithIdentity.Smalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypesWithIdentity.Smallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypesWithIdentity.Smallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypesWithIdentity.Text ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypesWithIdentity.Time ---> [nullable time] [Precision = 7]
MappedNullableDataTypesWithIdentity.Tinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypesWithIdentity.VarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypesWithIdentity.VarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedPrecisionAndScaledDataTypes.Dec ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Decimal ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypes.Numeric ---> [numeric] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Dec ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Decimal ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypesWithIdentity.Numeric ---> [numeric] [Precision = 5 Scale = 2]
MappedScaledDataTypes.Datetime2 ---> [datetime2] [Precision = 3]
MappedScaledDataTypes.Datetimeoffset ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypes.Dec ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.Decimal ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.Double_precision ---> [real] [Precision = 24]
MappedScaledDataTypes.Float ---> [real] [Precision = 24]
MappedScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypes.Numeric ---> [numeric] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.Datetime2 ---> [datetime2] [Precision = 3]
MappedScaledDataTypesWithIdentity.Datetimeoffset ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypesWithIdentity.Dec ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.Decimal ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypesWithIdentity.Double_precision ---> [real] [Precision = 24]
MappedScaledDataTypesWithIdentity.Float ---> [real] [Precision = 24]
MappedScaledDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypesWithIdentity.Numeric ---> [numeric] [Precision = 3 Scale = 0]
MappedSizedDataTypes.Binary ---> [nullable binary] [MaxLength = 3]
MappedSizedDataTypes.Binary_varying ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypes.Char ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.Char_varying ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.Character ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypes.Character_varying ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypes.National_char_varying ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.National_character ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypes.National_character_varying ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.Nchar ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypes.Nvarchar ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypes.Varbinary ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypes.Varchar ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Binary ---> [nullable binary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Binary_varying ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Char ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Char_varying ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Character ---> [nullable char] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Character_varying ---> [nullable varchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Id ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.Int ---> [int] [Precision = 10 Scale = 0]
MappedSizedDataTypesWithIdentity.National_char_varying ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.National_character ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.National_character_varying ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Nchar ---> [nullable nchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Nvarchar ---> [nullable nvarchar] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Varbinary ---> [nullable varbinary] [MaxLength = 3]
MappedSizedDataTypesWithIdentity.Varchar ---> [nullable varchar] [MaxLength = 3]
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
UnicodeDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
UnicodeDataTypes.StringAnsi ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringAnsi3 ---> [nullable varchar] [MaxLength = 3]
UnicodeDataTypes.StringAnsi9000 ---> [nullable varchar] [MaxLength = -1]
UnicodeDataTypes.StringDefault ---> [nullable nvarchar] [MaxLength = -1]
UnicodeDataTypes.StringUnicode ---> [nullable nvarchar] [MaxLength = -1]
";

            Assert.Equal(expected, actual);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private class ColumnInfo
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
