// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Relational.Tests.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.Tests.Storage
{
    public class SqliteTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new SqliteCommand();

        protected override DbType DefaultParameterType
            => DbType.String;

        [Theory]
        [InlineData("TEXT", typeof(string))]
        [InlineData("Integer", typeof(long))]
        [InlineData("Blob", typeof(byte[]))]
        [InlineData("numeric", typeof(string))]
        [InlineData("real", typeof(double))]
        [InlineData("doub", typeof(double))]
        [InlineData("int", typeof(long))]
        [InlineData("SMALLINT", typeof(long))]
        [InlineData("UNSIGNED BIG INT", typeof(long))]
        [InlineData("VARCHAR(255)", typeof(string))]
        [InlineData("nchar(55)", typeof(string))]
        [InlineData("datetime", typeof(string))]
        [InlineData("decimal(10,4)", typeof(string))]
        [InlineData("boolean", typeof(string))]
        [InlineData("unknown_type", typeof(string))]
        [InlineData("", typeof(string))]
        public void It_maps_strings_to_not_null_types(string typeName, Type clrType)
        {
            Assert.Equal(clrType, new SqliteTypeMapper().GetMapping(typeName).ClrType);
        }
    }
}
