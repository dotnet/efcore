// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class BadDataSqliteTest : IClassFixture<NorthwindQuerySqliteFixture>
    {
        private readonly NorthwindQuerySqliteFixture _fixture;

        public BadDataSqliteTest(NorthwindQuerySqliteFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast_key()
        {
            using (var context = CreateContext("bad int"))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueInvalidCast(typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(() =>
                            context.Set<Product>().ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null_key()
        {
            using (var context = CreateContext(null, true))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueNullReference(typeof(int)),
                    Assert.Throws<InvalidOperationException>(() =>
                            context.Set<Product>().ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast()
        {
            using (var context = CreateContext(1, true, 1))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueInvalidCast(typeof(string), typeof(int)),
                    Assert.Throws<InvalidOperationException>(() =>
                            context.Set<Product>().ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast_projection()
        {
            using (var context = CreateContext(1))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueInvalidCast(typeof(string), typeof(int)),
                    Assert.Throws<InvalidOperationException>(() =>
                        context.Set<Product>()
                            .Select(p => p.ProductName)
                            .ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_invalid_cast_no_tracking()
        {
            using (var context = CreateContext("bad int"))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueInvalidCast(typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(() =>
                        context.Set<Product>()
                            .AsNoTracking()
                            .ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null()
        {
            using (var context = CreateContext(1, null))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueNullReference(typeof(bool)),
                    Assert.Throws<InvalidOperationException>(() =>
                            context.Set<Product>().ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null_projection()
        {
            using (var context = CreateContext(new object[] { null }))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueNullReference(typeof(bool)),
                    Assert.Throws<InvalidOperationException>(() =>
                        context.Set<Product>()
                            .Select(p => p.Discontinued)
                            .ToList()).Message);
            }
        }

        [Fact]
        public void Bad_data_error_handling_null_no_tracking()
        {
            using (var context = CreateContext(null, true))
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingValueNullReference(typeof(int)),
                    Assert.Throws<InvalidOperationException>(() =>
                        context.Set<Product>()
                            .AsNoTracking()
                            .ToList()).Message);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class BadDataCommandBuilderFactory : RelationalCommandBuilderFactory
        {
            public BadDataCommandBuilderFactory(
                ISensitiveDataLogger<IRelationalCommandBuilderFactory> logger,
                DiagnosticSource diagnosticSource,
                IRelationalTypeMapper typeMapper)
                : base(logger, diagnosticSource, typeMapper)
            {
            }

            public object[] Values { private get; set; }

            protected override IRelationalCommandBuilder CreateCore(
                    ISensitiveDataLogger sensitiveDataLogger,
                    DiagnosticSource diagnosticSource,
                    IRelationalTypeMapper relationalTypeMapper)
                => new BadDataRelationalCommandBuilder(
                    sensitiveDataLogger, diagnosticSource, relationalTypeMapper, Values);

            private class BadDataRelationalCommandBuilder : RelationalCommandBuilder
            {
                private readonly object[] _values;

                public BadDataRelationalCommandBuilder(
                    ISensitiveDataLogger logger,
                    DiagnosticSource diagnosticSource,
                    IRelationalTypeMapper typeMapper,
                    object[] values)
                    : base(logger, diagnosticSource, typeMapper)
                {
                    _values = values;
                }

                protected override IRelationalCommand BuildCore(
                        ISensitiveDataLogger sensitiveDataLogger,
                        DiagnosticSource diagnosticSource,
                        string commandText,
                        IReadOnlyList<IRelationalParameter> parameters)
                    => new BadDataRelationalCommand(sensitiveDataLogger, diagnosticSource, commandText, parameters, _values);

                private class BadDataRelationalCommand : RelationalCommand
                {
                    private readonly object[] _values;

                    public BadDataRelationalCommand(
                        ISensitiveDataLogger logger,
                        DiagnosticSource diagnosticSource,
                        string commandText,
                        IReadOnlyList<IRelationalParameter> parameters,
                        object[] values)
                        : base(logger, diagnosticSource, commandText, parameters)
                    {
                        _values = values;
                    }

                    public override RelationalDataReader ExecuteReader(
                            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues)
                        => new BadDataRelationalDataReader(base.ExecuteReader(connection, parameterValues), _values);

                    private class BadDataRelationalDataReader : RelationalDataReader
                    {
                        private readonly RelationalDataReader _relationalDataReader;
                        private readonly BadDataDataReader _dataReader;

                        public BadDataRelationalDataReader(RelationalDataReader relationalDataReader, object[] values)
                        {
                            _relationalDataReader = relationalDataReader;
                            _dataReader = new BadDataDataReader(values);
                        }

                        private class BadDataDataReader : DbDataReader
                        {
                            private readonly object[] _values;

                            public BadDataDataReader(object[] values)
                            {
                                _values = values;
                            }

                            public override bool Read() => true;

                            public override bool IsDBNull(int ordinal)
                                => false;

                            public override int GetInt32(int ordinal) => (int)GetValue(ordinal);

                            public override short GetInt16(int ordinal) => (short)GetValue(ordinal);

                            public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);

                            public override string GetString(int ordinal) => (string)GetValue(ordinal);

                            public override object GetValue(int ordinal) => _values[ordinal];

                            #region NotImplemented members

                            public override string GetName(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override int GetValues(object[] values)
                            {
                                throw new NotImplementedException();
                            }

                            public override int FieldCount
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override object this[int ordinal]
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override object this[string name]
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override bool HasRows
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override bool IsClosed
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override int RecordsAffected
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override bool NextResult()
                            {
                                throw new NotImplementedException();
                            }

                            public override int Depth
                            {
                                get { throw new NotImplementedException(); }
                            }

                            public override int GetOrdinal(string name)
                            {
                                throw new NotImplementedException();
                            }

                            public override byte GetByte(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
                            {
                                throw new NotImplementedException();
                            }

                            public override char GetChar(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
                            {
                                throw new NotImplementedException();
                            }

                            public override Guid GetGuid(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override long GetInt64(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override DateTime GetDateTime(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override decimal GetDecimal(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override double GetDouble(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override float GetFloat(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override string GetDataTypeName(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override Type GetFieldType(int ordinal)
                            {
                                throw new NotImplementedException();
                            }

                            public override IEnumerator GetEnumerator()
                            {
                                throw new NotImplementedException();
                            }

                            #endregion
                        }

                        public override DbDataReader DbDataReader => _dataReader;

                        public override void Dispose() => _relationalDataReader.Dispose();
                    }
                }
            }
        }

        private NorthwindContext CreateContext(params object[] values)
        {
            var context = new NorthwindContext(
                _fixture.BuildOptions(
                    new ServiceCollection()
                        .AddScoped<IRelationalCommandBuilderFactory, BadDataCommandBuilderFactory>()));

            var badDataCommandBuilderFactory
                = (BadDataCommandBuilderFactory)context.GetService<IRelationalCommandBuilderFactory>();

            badDataCommandBuilderFactory.Values = values;

            return context;
        }
    }
}
