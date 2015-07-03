// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class BuiltInDataTypesSqlServerTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqlServerFixture>
    {
        public BuiltInDataTypesSqlServerTest(BuiltInDataTypesSqlServerFixture fixture)
            : base(fixture)
        {
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
                        Char = "A",
                        Character = "B",
                        Varchar = "I",
                        Char_varying = "J",
                        Character_varying = "K",
                        VarcharMax = "C",
                        Char_varyingMax = "Your",
                        Character_varyingMax = "strong",
                        Nchar = "D",
                        National_character = "E",
                        Nvarchar = "F",
                        National_char_varying = "G",
                        National_character_varying = "H",
                        NvarcharMax = "don't",
                        National_char_varyingMax = "help",
                        National_character_varyingMax = "anyone!",
                        Text = "Gumball Rules!",
                        Ntext = "Gumball Rules OK!",
                        Binary = new byte[] { 86 },
                        Varbinary = new byte[] { 87 },
                        Binary_varying = new byte[] { 88 },
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

                var param14 = "A";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Char == param14));

                var param15 = "B";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Character== param15));

                var param16 = "I";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Varchar == param16));

                var param17 = "J";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Char_varying == param17));

                var param18 = "K";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Character_varying == param18));

                var param19 = "C";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.VarcharMax == param19));

                var param20 = "Your";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Char_varyingMax == param20));

                var param21 = "strong";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Character_varyingMax == param21));

                var param22 = "D";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Nchar == param22));

                var param23 = "E";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_character == param23));

                var param24 = "F";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Nvarchar == param24));

                var param25 = "G";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_char_varying == param25));

                var param26 = "H";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_character_varying == param26));

                var param27 = "don't";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.NvarcharMax == param27));

                var param28 = "help";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_char_varyingMax == param28));

                var param29 = "anyone!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.National_character_varyingMax == param29));

                var param32 = new byte[] { 86 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Binary == param32));

                var param33 = new byte[] { 87 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Varbinary == param33));

                var param34 = new byte[] { 88 };
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 999 && e.Binary_varying == param34));

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
                        Int = 911,
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
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && ((long?)((int?)e.Smallint)) == param2));

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

                string param14 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Char == param14));

                string param15 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Character == param15));

                string param16 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Varchar == param16));

                string param17 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Char_varying == param17));

                string param18 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Character_varying == param18));

                string param19 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.VarcharMax == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Char_varyingMax == param20));

                string param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Character_varyingMax == param21));

                string param22 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Nchar == param22));

                string param23 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.National_character == param23));

                string param24 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Nvarchar == param24));

                string param25 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.National_char_varying == param25));

                string param26 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.National_character_varying == param26));

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

                byte[] param32 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Binary == param32));

                byte[] param33 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Varbinary == param33));

                byte[] param34 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.Binary_varying == param34));

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
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(
                    new MappedDataTypes
                        {
                            Int = 77,
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
                            Char = "A",
                            Character = "B",
                            Varchar = "I",
                            Char_varying = "J",
                            Character_varying = "K",
                            VarcharMax = "C",
                            Char_varyingMax = "Your",
                            Character_varyingMax = "strong",
                            Nchar = "D",
                            National_character = "E",
                            Nvarchar = "F",
                            National_char_varying = "G",
                            National_character_varying = "H",
                            NvarcharMax = "don't",
                            National_char_varyingMax = "help",
                            National_character_varyingMax = "anyone!",
                            Text = "Gumball Rules!",
                            Ntext = "Gumball Rules OK!",
                            Binary = new byte[] { 86 },
                            Varbinary = new byte[] { 87 },
                            Binary_varying = new byte[] { 88 },
                            VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                            Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                            Image = new byte[] { 97, 98, 99, 100 },
                            Decimal = 101.1m,
                            Dec = 102.2m,
                            Numeric = 103.3m
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedDataTypes>().Single(e => e.Int == 77);

                Assert.Equal(77, entity.Int);
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
                Assert.Equal("A", entity.Char);
                Assert.Equal("B", entity.Character);
                Assert.Equal("I", entity.Varchar);
                Assert.Equal("J", entity.Char_varying);
                Assert.Equal("K", entity.Character_varying);
                Assert.Equal("C", entity.VarcharMax);
                Assert.Equal("Your", entity.Char_varyingMax);
                Assert.Equal("strong", entity.Character_varyingMax);
                Assert.Equal("D", entity.Nchar);
                Assert.Equal("E", entity.National_character);
                Assert.Equal("F", entity.Nvarchar);
                Assert.Equal("G", entity.National_char_varying);
                Assert.Equal("H", entity.National_character_varying);
                Assert.Equal("don't", entity.NvarcharMax);
                Assert.Equal("help", entity.National_char_varyingMax);
                Assert.Equal("anyone!", entity.National_character_varyingMax);
                Assert.Equal("Gumball Rules!", entity.Text);
                Assert.Equal("Gumball Rules OK!", entity.Ntext);
                Assert.Equal(new byte[] { 86 }, entity.Binary);
                Assert.Equal(new byte[] { 87 }, entity.Varbinary);
                Assert.Equal(new byte[] { 88 }, entity.Binary_varying);
                Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
                Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
                Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.Image);
                Assert.Equal(101m, entity.Decimal);
                Assert.Equal(102m, entity.Dec);
                Assert.Equal(103m, entity.Numeric);
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
                            Int = 77,
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
                            Char = "A",
                            Character = "B",
                            Varchar = "I",
                            Char_varying = "J",
                            Character_varying = "K",
                            VarcharMax = "C",
                            Char_varyingMax = "Your",
                            Character_varyingMax = "strong",
                            Nchar = "D",
                            National_character = "E",
                            Nvarchar = "F",
                            National_char_varying = "G",
                            National_character_varying = "H",
                            NvarcharMax = "don't",
                            National_char_varyingMax = "help",
                            National_character_varyingMax = "anyone!",
                            Text = "Gumball Rules!",
                            Ntext = "Gumball Rules OK!",
                            Binary = new byte[] { 86 },
                            Varbinary = new byte[] { 87 },
                            Binary_varying = new byte[] { 88 },
                            VarbinaryMax = new byte[] { 89, 90, 91, 92 },
                            Binary_varyingMax = new byte[] { 93, 94, 95, 96 },
                            Image = new byte[] { 97, 98, 99, 100 },
                            Decimal = 101.1m,
                            Dec = 102.2m,
                            Numeric = 103.3m
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 77);

                Assert.Equal(77, entity.Int);
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
                Assert.Equal("A", entity.Char);
                Assert.Equal("B", entity.Character);
                Assert.Equal("I", entity.Varchar);
                Assert.Equal("J", entity.Char_varying);
                Assert.Equal("K", entity.Character_varying);
                Assert.Equal("C", entity.VarcharMax);
                Assert.Equal("Your", entity.Char_varyingMax);
                Assert.Equal("strong", entity.Character_varyingMax);
                Assert.Equal("D", entity.Nchar);
                Assert.Equal("E", entity.National_character);
                Assert.Equal("F", entity.Nvarchar);
                Assert.Equal("G", entity.National_char_varying);
                Assert.Equal("H", entity.National_character_varying);
                Assert.Equal("don't", entity.NvarcharMax);
                Assert.Equal("help", entity.National_char_varyingMax);
                Assert.Equal("anyone!", entity.National_character_varyingMax);
                Assert.Equal("Gumball Rules!", entity.Text);
                Assert.Equal("Gumball Rules OK!", entity.Ntext);
                Assert.Equal(new byte[] { 86 }, entity.Binary);
                Assert.Equal(new byte[] { 87 }, entity.Varbinary);
                Assert.Equal(new byte[] { 88 }, entity.Binary_varying);
                Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.VarbinaryMax);
                Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.Binary_varyingMax);
                Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.Image);
                Assert.Equal(101m, entity.Decimal);
                Assert.Equal(102m, entity.Dec);
                Assert.Equal(103m, entity.Numeric);
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
                            Int = 78
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 78);

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
                Assert.Null(entity.Char);
                Assert.Null(entity.Character);
                Assert.Null(entity.VarcharMax);
                Assert.Null(entity.Char_varyingMax);
                Assert.Null(entity.Character_varyingMax);
                Assert.Null(entity.Nchar);
                Assert.Null(entity.National_character);
                Assert.Null(entity.Nvarchar);
                Assert.Null(entity.National_char_varying);
                Assert.Null(entity.National_character_varying);
                Assert.Null(entity.NvarcharMax);
                Assert.Null(entity.National_char_varyingMax);
                Assert.Null(entity.National_character_varyingMax);
                Assert.Null(entity.Text);
                Assert.Null(entity.Ntext);
                Assert.Null(entity.Binary);
                Assert.Null(entity.Varbinary);
                Assert.Null(entity.Binary_varying);
                Assert.Null(entity.VarbinaryMax);
                Assert.Null(entity.Binary_varyingMax);
                Assert.Null(entity.Image);
                Assert.Null(entity.Decimal);
                Assert.Null(entity.Dec);
                Assert.Null(entity.Numeric);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_sized_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedSizedDataTypes>().Add(
                    new MappedSizedDataTypes
                        {
                            Id = 77,
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
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedSizedDataTypes>().Single(e => e.Id == 77);

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
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_scale()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedScaledDataTypes>().Add(
                    new MappedScaledDataTypes
                        {
                            Id = 77,
                            Float = 83.3f,
                            Double_precision = 85.5f,
                            Datetimeoffset = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                            Datetime2 = new DateTime(2017, 1, 2, 12, 11, 12),
                            Decimal = 101.1m,
                            Dec = 102.2m,
                            Numeric = 103.3m
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedScaledDataTypes>().Single(e => e.Id == 77);

                Assert.Equal(83.3f, entity.Float);
                Assert.Equal(85.5f, entity.Double_precision);
                Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.Datetimeoffset);
                Assert.Equal(new DateTime(2017, 1, 2, 12, 11, 12), entity.Datetime2);
                Assert.Equal(101m, entity.Decimal);
                Assert.Equal(102m, entity.Dec);
                Assert.Equal(103m, entity.Numeric);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_with_precision_and_scale()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedPrecisionAndScaledDataTypes>().Add(
                    new MappedPrecisionAndScaledDataTypes
                        {
                            Id = 77,
                            Decimal = 101.1m,
                            Dec = 102.2m,
                            Numeric = 103.3m
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedPrecisionAndScaledDataTypes>().Single(e => e.Id == 77);

                Assert.Equal(101.1m, entity.Decimal);
                Assert.Equal(102.2m, entity.Dec);
                Assert.Equal(103.3m, entity.Numeric);
            }
        }

        [Fact]
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

            var actual = builder.ToString();

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
MappedDataTypes.Binary ---> [binary] [MaxLength = 1]
MappedDataTypes.Binary_varying ---> [varbinary] [MaxLength = 1]
MappedDataTypes.Binary_varyingMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.Bit ---> [bit]
MappedDataTypes.Char ---> [char] [MaxLength = 1]
MappedDataTypes.Char_varying ---> [varchar] [MaxLength = 1]
MappedDataTypes.Char_varyingMax ---> [varchar] [MaxLength = -1]
MappedDataTypes.Character ---> [char] [MaxLength = 1]
MappedDataTypes.Character_varying ---> [varchar] [MaxLength = 1]
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
MappedDataTypes.National_char_varying ---> [nvarchar] [MaxLength = 1]
MappedDataTypes.National_char_varyingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.National_character ---> [nchar] [MaxLength = 1]
MappedDataTypes.National_character_varying ---> [nvarchar] [MaxLength = 1]
MappedDataTypes.National_character_varyingMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.Nchar ---> [nchar] [MaxLength = 1]
MappedDataTypes.Ntext ---> [ntext] [MaxLength = 1073741823]
MappedDataTypes.Numeric ---> [numeric] [Precision = 18 Scale = 0]
MappedDataTypes.Nvarchar ---> [nvarchar] [MaxLength = 1]
MappedDataTypes.NvarcharMax ---> [nvarchar] [MaxLength = -1]
MappedDataTypes.Real ---> [real] [Precision = 24]
MappedDataTypes.Smalldatetime ---> [smalldatetime] [Precision = 0]
MappedDataTypes.Smallint ---> [smallint] [Precision = 5 Scale = 0]
MappedDataTypes.Smallmoney ---> [smallmoney] [Precision = 10 Scale = 4]
MappedDataTypes.Text ---> [text] [MaxLength = 2147483647]
MappedDataTypes.Time ---> [time] [Precision = 7]
MappedDataTypes.Tinyint ---> [tinyint] [Precision = 3 Scale = 0]
MappedDataTypes.Varbinary ---> [varbinary] [MaxLength = 1]
MappedDataTypes.VarbinaryMax ---> [varbinary] [MaxLength = -1]
MappedDataTypes.Varchar ---> [varchar] [MaxLength = 1]
MappedDataTypes.VarcharMax ---> [varchar] [MaxLength = -1]
MappedNullableDataTypes.Bigint ---> [nullable bigint] [Precision = 19 Scale = 0]
MappedNullableDataTypes.Binary ---> [nullable binary] [MaxLength = 1]
MappedNullableDataTypes.Binary_varying ---> [nullable varbinary] [MaxLength = 1]
MappedNullableDataTypes.Binary_varyingMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.Bit ---> [nullable bit]
MappedNullableDataTypes.Char ---> [nullable char] [MaxLength = 1]
MappedNullableDataTypes.Char_varying ---> [nullable varchar] [MaxLength = 1]
MappedNullableDataTypes.Char_varyingMax ---> [nullable varchar] [MaxLength = -1]
MappedNullableDataTypes.Character ---> [nullable char] [MaxLength = 1]
MappedNullableDataTypes.Character_varying ---> [nullable varchar] [MaxLength = 1]
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
MappedNullableDataTypes.National_char_varying ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypes.National_char_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.National_character ---> [nullable nchar] [MaxLength = 1]
MappedNullableDataTypes.National_character_varying ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypes.National_character_varyingMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.Nchar ---> [nullable nchar] [MaxLength = 1]
MappedNullableDataTypes.Ntext ---> [nullable ntext] [MaxLength = 1073741823]
MappedNullableDataTypes.Numeric ---> [nullable numeric] [Precision = 18 Scale = 0]
MappedNullableDataTypes.Nvarchar ---> [nullable nvarchar] [MaxLength = 1]
MappedNullableDataTypes.NvarcharMax ---> [nullable nvarchar] [MaxLength = -1]
MappedNullableDataTypes.Real ---> [nullable real] [Precision = 24]
MappedNullableDataTypes.Smalldatetime ---> [nullable smalldatetime] [Precision = 0]
MappedNullableDataTypes.Smallint ---> [nullable smallint] [Precision = 5 Scale = 0]
MappedNullableDataTypes.Smallmoney ---> [nullable smallmoney] [Precision = 10 Scale = 4]
MappedNullableDataTypes.Text ---> [nullable text] [MaxLength = 2147483647]
MappedNullableDataTypes.Time ---> [nullable time] [Precision = 7]
MappedNullableDataTypes.Tinyint ---> [nullable tinyint] [Precision = 3 Scale = 0]
MappedNullableDataTypes.Varbinary ---> [nullable varbinary] [MaxLength = 1]
MappedNullableDataTypes.VarbinaryMax ---> [nullable varbinary] [MaxLength = -1]
MappedNullableDataTypes.Varchar ---> [nullable varchar] [MaxLength = 1]
MappedNullableDataTypes.VarcharMax ---> [nullable varchar] [MaxLength = -1]
MappedPrecisionAndScaledDataTypes.Dec ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Decimal ---> [decimal] [Precision = 5 Scale = 2]
MappedPrecisionAndScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedPrecisionAndScaledDataTypes.Numeric ---> [numeric] [Precision = 5 Scale = 2]
MappedScaledDataTypes.Datetime2 ---> [datetime2] [Precision = 3]
MappedScaledDataTypes.Datetimeoffset ---> [datetimeoffset] [Precision = 3]
MappedScaledDataTypes.Dec ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.Decimal ---> [decimal] [Precision = 3 Scale = 0]
MappedScaledDataTypes.Double_precision ---> [real] [Precision = 24]
MappedScaledDataTypes.Float ---> [real] [Precision = 24]
MappedScaledDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MappedScaledDataTypes.Numeric ---> [numeric] [Precision = 3 Scale = 0]
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
MaxLengthDataTypes.ByteArray5 ---> [nullable varbinary] [MaxLength = 5]
MaxLengthDataTypes.ByteArray9000 ---> [nullable varbinary] [MaxLength = -1]
MaxLengthDataTypes.Id ---> [int] [Precision = 10 Scale = 0]
MaxLengthDataTypes.String3 ---> [nullable nvarchar] [MaxLength = 3]
MaxLengthDataTypes.String9000 ---> [nullable nvarchar] [MaxLength = -1]
StringForeignKeyDataType.Id ---> [int] [Precision = 10 Scale = 0]
StringForeignKeyDataType.StringKeyDataTypeId ---> [nullable nvarchar] [MaxLength = 450]
StringKeyDataType.Id ---> [nvarchar] [MaxLength = 450]
";

            Assert.Equal(expected, actual);
        }

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
