// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQuerySqliteTest : SimpleQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public SimpleQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Contains(
                @"SELECT ""t"".*" + EOL +
                @"FROM (" + EOL +
                @"    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"    FROM ""Customers"" AS ""c""" + EOL +
                @"    ORDER BY ""c"".""ContactName""" + EOL +
                @"    LIMIT @__p_0" + EOL +
                @") AS ""t""" + EOL +
                @"ORDER BY ""t"".""ContactName""" + EOL +
                @"LIMIT -1 OFFSET @__p_1",
                Sql);
        }

        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE ""c"".""Region"" IS NULL OR (trim(""c"".""Region"") = '')",
                Sql);
        }

        public override void TrimStart_without_arguments_in_predicate()
        {
            base.TrimStart_without_arguments_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE ltrim(""c"".""ContactTitle"") = 'Owner'",
                Sql);
        }

        public override void TrimStart_with_char_argument_in_predicate()
        {
            base.TrimStart_with_char_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE ltrim(""c"".""ContactTitle"", 'O') = 'wner'",
                Sql);
        }

        public override void TrimStart_with_char_array_argument_in_predicate()
        {
            base.TrimStart_with_char_array_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE ltrim(""c"".""ContactTitle"", 'Ow') = 'ner'",
                Sql);
        }

        public override void TrimEnd_without_arguments_in_predicate()
        {
            base.TrimEnd_without_arguments_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE rtrim(""c"".""ContactTitle"") = 'Owner'",
                Sql);
        }

        public override void TrimEnd_with_char_argument_in_predicate()
        {
            base.TrimEnd_with_char_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE rtrim(""c"".""ContactTitle"", 'r') = 'Owne'",
                Sql);
        }

        public override void TrimEnd_with_char_array_argument_in_predicate()
        {
            base.TrimEnd_with_char_array_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE rtrim(""c"".""ContactTitle"", 'er') = 'Own'",
                Sql);
        }

        public override void Trim_without_argument_in_predicate()
        {
            base.Trim_without_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE trim(""c"".""ContactTitle"") = 'Owner'",
                Sql);
        }

        public override void Trim_with_char_argument_in_predicate()
        {
            base.Trim_with_char_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE trim(""c"".""ContactTitle"", 'O') = 'wner'",
                Sql);
        }

        public override void Trim_with_char_array_argument_in_predicate()
        {
            base.Trim_with_char_array_argument_in_predicate();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""" + EOL +
                @"FROM ""Customers"" AS ""c""" + EOL +
                @"WHERE trim(""c"".""ContactTitle"", 'Or') = 'wne'",
                Sql);
        }

        public override void Sum_with_coalesce()
        {
            base.Sum_with_coalesce();

            Assert.Contains(
                @"SELECT SUM(COALESCE(""p"".""UnitPrice"", 0.0))" + EOL +
                @"FROM ""Products"" AS ""p""" + EOL +
                @"WHERE ""p"".""ProductID"" < 40",
                Sql);
        }

        private static readonly string EOL = Environment.NewLine;

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
