// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class RawSqlStringTest
    {
        [Fact]
        public void Trailing_semicolon_gets_removed()
        {
            var sql = "SELECT * FROM Customers;";

            var sut = new RawSqlString(sql);

            Assert.Equal("SELECT * FROM Customers", sut.Format);
        }

        [Fact]
        public void Trailing_semicolon_followed_by_space_gets_removed()
        {
            var sql = "SELECT * FROM Customers; ";

            var sut = new RawSqlString(sql);

            Assert.Equal("SELECT * FROM Customers", sut.Format);
        }

        [Fact]
        public void Trailing_semicolon_separated_by_space_gets_removed()
        {
            var sql = "SELECT * FROM Customers ;";

            var sut = new RawSqlString(sql);

            Assert.Equal("SELECT * FROM Customers", sut.Format);
        }

        [Fact]
        public void Multiple_trailing_semicolons_are_removed()
        {
            var sql = "SELECT * FROM Customers;;";

            var sut = new RawSqlString(sql);

            Assert.Equal("SELECT * FROM Customers", sut.Format);
        }

        [Fact]
        public void Multiple_trailing_semicolons_separated_by_space_are_removed()
        {
            var sql = "SELECT * FROM Customers; ;";

            var sut = new RawSqlString(sql);

            Assert.Equal("SELECT * FROM Customers", sut.Format);
        }

        [Fact]
        public void Semicolons_not_at_the_end_are_kept()
        {
            var text = "abc;def;xyz;";

            var sut = new RawSqlString(text);

            Assert.Equal("abc;def;xyz", sut.Format);
        }
    }
}
