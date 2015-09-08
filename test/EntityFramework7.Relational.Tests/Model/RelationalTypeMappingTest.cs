// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Model
{
    public class RelationalTypeMappingTest
    {
        [Fact]
        public void Can_create_simple_parameter()
        {
            var parameter = new RelationalTypeMapping("int")
                .CreateParameter(new TestCommand(), "Name", 17, isNullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DbType.AnsiString, parameter.DbType);
            Assert.False(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_nullable_parameter()
        {
            var parameter = new RelationalTypeMapping("int")
                .CreateParameter(new TestCommand(), "Name", 17, isNullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DbType.AnsiString, parameter.DbType);
            Assert.True(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_parameter_with_DbType()
        {
            var parameter = new RelationalTypeMapping("int", DbType.Int32)
                .CreateParameter(new TestCommand(), "Name", 17, isNullable: false);

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
                .CreateParameter(new TestCommand(), "Name", 17, isNullable: true);

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
                .CreateParameter(new TestCommand(), "Name", "Value", isNullable: false);

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
                .CreateParameter(new TestCommand(), "Name", "Value", isNullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal("Value", parameter.Value);
            Assert.Equal(DbType.String, parameter.DbType);
            Assert.True(parameter.IsNullable);
            Assert.Equal(23, parameter.Size);
        }

        private class TestParameter : DbParameter
        {
            public override void ResetDbType()
            {
            }

            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override string SourceColumn { get; set; }
            public override DataRowVersion SourceVersion { get; set; }
            public override object Value { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override int Size { get; set; }
        }

        private class TestCommand : DbCommand
        {
            public override void Prepare()
            {
            }

            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection { get; }
            protected override DbTransaction DbTransaction { get; set; }
            public override bool DesignTimeVisible { get; set; }

            public override void Cancel()
            {
            }

            protected override DbParameter CreateDbParameter()
            {
                return new TestParameter();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                throw new NotImplementedException();
            }

            public override int ExecuteNonQuery()
            {
                throw new NotImplementedException();
            }

            public override object ExecuteScalar()
            {
                throw new NotImplementedException();
            }
        }
    }
}
