// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Xunit;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class RelationalTypeMappingTest
    {
        [Fact]
        public void Can_create_simple_parameter()
        {
            var parameter = new RelationalTypeMapping("int")
                .CreateParameter(CreateTestCommand(), "Name", 17, isNullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DefaultParameterType, parameter.DbType);
            Assert.False(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_nullable_parameter()
        {
            var parameter = new RelationalTypeMapping("int")
                .CreateParameter(CreateTestCommand(), "Name", 17, isNullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DefaultParameterType, parameter.DbType);
            Assert.True(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_parameter_with_DbType()
        {
            var parameter = new RelationalTypeMapping("int", DbType.Int32)
                .CreateParameter(CreateTestCommand(), "Name", 17, isNullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DbType.Int32, parameter.DbType);
            Assert.False(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_nullable_parameter_with_DbType()
        {
            var parameter = new RelationalTypeMapping("int", DbType.Int32)
                .CreateParameter(CreateTestCommand(), "Name", 17, isNullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DbType.Int32, parameter.DbType);
            Assert.True(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_required_string_parameter()
        {
            var parameter = new RelationalSizedTypeMapping("nvarchar(23)", DbType.String, 23)
                .CreateParameter(CreateTestCommand(), "Name", "Value", isNullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal("Value", parameter.Value);
            Assert.Equal(DbType.String, parameter.DbType);
            Assert.False(parameter.IsNullable);
            Assert.Equal(23, parameter.Size);
        }

        [Fact]
        public void Can_create_string_parameter()
        {
            var parameter = new RelationalSizedTypeMapping("nvarchar(23)", DbType.String, 23)
                .CreateParameter(CreateTestCommand(), "Name", "Value", isNullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal("Value", parameter.Value);
            Assert.Equal(DbType.String, parameter.DbType);
            Assert.True(parameter.IsNullable);
            Assert.Equal(23, parameter.Size);
        }

        protected abstract DbCommand CreateTestCommand();

        protected abstract DbType DefaultParameterType { get; }
    }
}
