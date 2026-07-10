// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class SqlServerSequenceValueGeneratorTest
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_int_values(bool async)
        => await Generates_sequential_values<int>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_long_values(bool async)
        => await Generates_sequential_values<long>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_short_values(bool async)
        => await Generates_sequential_values<short>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_byte_values(bool async)
        => await Generates_sequential_values<byte>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_uint_values(bool async)
        => await Generates_sequential_values<uint>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_ulong_values(bool async)
        => await Generates_sequential_values<ulong>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_ushort_values(bool async)
        => await Generates_sequential_values<ushort>(async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Generates_sequential_sbyte_values(bool async)
        => await Generates_sequential_values<sbyte>(async);

    private async Task Generates_sequential_values<TValue>(bool async)
    {
        const int blockSize = 4;

        var sequence = ((IMutableModel)new Model()).AddSequence("Foo");
        sequence.IncrementBy = blockSize;
        var state = new SqlServerSequenceValueGeneratorState((ISequence)sequence);

        var generator = new SqlServerSequenceHiLoValueGenerator<TValue>(
            new FakeRawSqlCommandBuilder(blockSize),
            new SqlServerUpdateSqlGenerator(
                new UpdateSqlGeneratorDependencies(
                    new SqlServerSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))),
            state,
            CreateConnection(),
            new FakeRelationalCommandDiagnosticsLogger());

        for (var i = 1; i <= 27; i++)
        {
            var value = async
                ? await generator.NextAsync(null)
                : generator.Next(null);

            Assert.Equal(i, (int)Convert.ChangeType(value, typeof(int), CultureInfo.InvariantCulture));
        }
    }

    [ConditionalFact]
    public async Task Multiple_threads_can_use_the_same_generator_state()
    {
        const int threadCount = 50;
        const int valueCount = 35;

        var generatedValues = await GenerateValuesInMultipleThreads(threadCount, valueCount);

        // Check that each value was generated once and only once
        var checks = new bool[threadCount * valueCount];
        foreach (var values in generatedValues)
        {
            Assert.Equal(valueCount, values.Count);
            foreach (var value in values)
            {
                checks[value - 1] = true;
            }
        }

        Assert.True(checks.All(c => c));
    }

    private async Task<IEnumerable<List<long>>> GenerateValuesInMultipleThreads(int threadCount, int valueCount)
    {
        const int blockSize = 10;

        var serviceProvider = SqlServerTestHelpers.Instance.CreateServiceProvider();

        var sequence = ((IMutableModel)new Model()).AddSequence("Foo");
        sequence.IncrementBy = blockSize;
        var state = new SqlServerSequenceValueGeneratorState((ISequence)sequence);

        var executor = new FakeRawSqlCommandBuilder(blockSize);
        var sqlGenerator = new SqlServerUpdateSqlGenerator(
            new UpdateSqlGeneratorDependencies(
                new SqlServerSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new SqlServerTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));

        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var tests = new Func<Task>[threadCount];
        var generatedValues = new List<long>[threadCount];
        for (var i = 0; i < tests.Length; i++)
        {
            var testNumber = i;
            generatedValues[testNumber] = [];
            tests[testNumber] = async () =>
            {
                for (var j = 0; j < valueCount; j++)
                {
                    var connection = CreateConnection(serviceProvider);
                    var generator = new SqlServerSequenceHiLoValueGenerator<long>(executor, sqlGenerator, state, connection, logger);

                    var value = j % 2 == 0
                        ? await generator.NextAsync(null)
                        : generator.Next(null);

                    generatedValues[testNumber].Add(value);
                }
            };
        }

        var tasks = tests.Select(Task.Run).ToArray();

        foreach (var t in tasks)
        {
            await t;
        }

        return generatedValues;
    }

    [ConditionalFact]
    public void Does_not_generate_temp_values()
    {
        var sequence = ((IMutableModel)new Model()).AddSequence("Foo");
        sequence.IncrementBy = 4;
        var state = new SqlServerSequenceValueGeneratorState((ISequence)sequence);

        var generator = new SqlServerSequenceHiLoValueGenerator<int>(
            new FakeRawSqlCommandBuilder(4),
            new SqlServerUpdateSqlGenerator(
                new UpdateSqlGeneratorDependencies(
                    new SqlServerSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()))),
            state,
            CreateConnection(),
            new FakeRelationalCommandDiagnosticsLogger());

        Assert.False(generator.GeneratesTemporaryValues);
    }

    private static ISqlServerConnection CreateConnection(IServiceProvider serviceProvider = null)
    {
        serviceProvider ??= SqlServerTestHelpers.Instance.CreateServiceProvider();

        return SqlServerTestHelpers.Instance.CreateContextServices(serviceProvider).GetRequiredService<ISqlServerConnection>();
    }

    private class FakeRawSqlCommandBuilder(int blockSize) : IRawSqlCommandBuilder
    {
        private readonly int _blockSize = blockSize;
        private long _current = -blockSize + 1;

        public IRelationalCommand Build(string sql)
            => new FakeRelationalCommand(this);

        public RawSqlCommand Build(string sql, IEnumerable<object> parameters)
            => throw new NotImplementedException();

        public RawSqlCommand Build(
            string sql,
            IEnumerable<object> parameters,
            IModel model)
            => new(new FakeRelationalCommand(this), new Dictionary<string, object>());

        private class FakeRelationalCommand(FakeRawSqlCommandBuilder commandBuilder) : IRelationalCommand
        {
            private readonly FakeRawSqlCommandBuilder _commandBuilder = commandBuilder;

            public string CommandText
                => throw new NotImplementedException();

            public IReadOnlyList<IRelationalParameter> Parameters
                => throw new NotImplementedException();

            public IReadOnlyDictionary<string, object> ParameterValues
                => throw new NotImplementedException();

            public int ExecuteNonQuery(RelationalCommandParameterObject parameterObject)
                => throw new NotImplementedException();

            public Task<int> ExecuteNonQueryAsync(
                RelationalCommandParameterObject parameterObject,
                CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public object ExecuteScalar(RelationalCommandParameterObject parameterObject)
                => Interlocked.Add(ref _commandBuilder._current, _commandBuilder._blockSize);

            public Task<object> ExecuteScalarAsync(
                RelationalCommandParameterObject parameterObject,
                CancellationToken cancellationToken = default)
                => Task.FromResult<object>(Interlocked.Add(ref _commandBuilder._current, _commandBuilder._blockSize));

            public RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
                => throw new NotImplementedException();

            public Task<RelationalDataReader> ExecuteReaderAsync(
                RelationalCommandParameterObject parameterObject,
                CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public DbCommand CreateDbCommand(
                RelationalCommandParameterObject parameterObject,
                Guid commandId,
                DbCommandMethod commandMethod)
                => throw new NotImplementedException();

            public void PopulateFrom(IRelationalCommandTemplate commandTemplate)
                => throw new NotImplementedException();
        }
    }
}
