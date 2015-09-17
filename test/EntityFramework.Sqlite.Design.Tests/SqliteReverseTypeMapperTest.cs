// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Sqlite.Design;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Design
{
    public class SqliteReverseTypeMapperTest
    {
        [Theory]
        [InlineData("TEXT", typeof(string))]
        [InlineData("Integer", typeof(long))]
        [InlineData("Blob", typeof(byte[]))]
        [InlineData("real", typeof(double))]
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
        [InlineData(null, typeof(string))]
        public void It_maps_not_null_types(string typeName, Type clrType)
        {
            Assert.Equal(clrType, new SqliteReverseTypeMapper().GetClrType(typeName, nullable: false));
        }

        [Theory]
        [InlineData("TEXT", typeof(string))]
        [InlineData("Integer", typeof(long?))]
        [InlineData("Blob", typeof(byte[]))]
        [InlineData("real", typeof(double?))]
        [InlineData("numeric", typeof(string))]
        [InlineData(null, typeof(string))]
        public void It_maps_nullable_types(string typeName, Type clrType)
        {
            Assert.Equal(clrType, new SqliteReverseTypeMapper().GetClrType(typeName, nullable: true));
        }
    }
}
