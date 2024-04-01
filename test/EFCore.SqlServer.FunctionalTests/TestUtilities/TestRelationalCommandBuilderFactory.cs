// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestRelationalCommandBuilderFactory(
    RelationalCommandBuilderDependencies dependencies) : IRelationalCommandBuilderFactory
{
    public RelationalCommandBuilderDependencies Dependencies { get; } = dependencies;

    public virtual IRelationalCommandBuilder Create()
        => new TestRelationalCommandBuilder(Dependencies);

    private class TestRelationalCommandBuilder(
        RelationalCommandBuilderDependencies dependencies) : IRelationalCommandBuilder
    {
        private readonly List<IRelationalParameter> _parameters = [];

        public IndentedStringBuilder Instance { get; } = new();

        public RelationalCommandBuilderDependencies Dependencies { get; } = dependencies;

        public IReadOnlyList<IRelationalParameter> Parameters
            => _parameters;

        public IRelationalCommandBuilder AddParameter(IRelationalParameter parameter)
        {
            _parameters.Add(parameter);

            return this;
        }

        public IRelationalCommandBuilder RemoveParameterAt(int index)
        {
            _parameters.RemoveAt(index);

            return this;
        }

        [Obsolete("Code trying to add parameter should add type mapped parameter using TypeMappingSource directly.")]
        public IRelationalTypeMappingSource TypeMappingSource
            => Dependencies.TypeMappingSource;

        public IRelationalCommand Build()
            => new TestRelationalCommand(
                Dependencies,
                Instance.ToString(),
                Parameters);

        public IRelationalCommandBuilder Append(string value)
        {
            Instance.Append(value);

            return this;
        }

        public IRelationalCommandBuilder AppendLine()
        {
            Instance.AppendLine();

            return this;
        }

        public IRelationalCommandBuilder IncrementIndent()
        {
            Instance.IncrementIndent();

            return this;
        }

        public IRelationalCommandBuilder DecrementIndent()
        {
            Instance.DecrementIndent();

            return this;
        }

        public int CommandTextLength
            => Instance.Length;
    }

    private class TestRelationalCommand(
        RelationalCommandBuilderDependencies dependencies,
        string commandText,
        IReadOnlyList<IRelationalParameter> parameters) : IRelationalCommand
    {
        private readonly RelationalCommand _realRelationalCommand = new RelationalCommand(dependencies, commandText, parameters);

        public string CommandText
            => _realRelationalCommand.CommandText;

        public IReadOnlyList<IRelationalParameter> Parameters
            => _realRelationalCommand.Parameters;

        public int ExecuteNonQuery(RelationalCommandParameterObject parameterObject)
        {
            var connection = parameterObject.Connection;
            var errorNumber = PreExecution(connection);

            var result = _realRelationalCommand.ExecuteNonQuery(parameterObject);
            if (errorNumber.HasValue)
            {
                connection.DbConnection.Close();
                throw SqlExceptionFactory.CreateSqlException(errorNumber.Value);
            }

            return result;
        }

        public Task<int> ExecuteNonQueryAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
        {
            var connection = parameterObject.Connection;
            var errorNumber = PreExecution(connection);

            var result = _realRelationalCommand.ExecuteNonQueryAsync(parameterObject, cancellationToken);
            if (errorNumber.HasValue)
            {
                connection.DbConnection.Close();
                throw SqlExceptionFactory.CreateSqlException(errorNumber.Value);
            }

            return result;
        }

        public object? ExecuteScalar(RelationalCommandParameterObject parameterObject)
        {
            var connection = parameterObject.Connection;
            var errorNumber = PreExecution(connection);

            var result = _realRelationalCommand.ExecuteScalar(parameterObject);
            if (errorNumber.HasValue)
            {
                connection.DbConnection.Close();
                throw SqlExceptionFactory.CreateSqlException(errorNumber.Value);
            }

            return result;
        }

        public async Task<object?> ExecuteScalarAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
        {
            var connection = parameterObject.Connection;
            var errorNumber = PreExecution(connection);

            var result = await _realRelationalCommand.ExecuteScalarAsync(parameterObject, cancellationToken);
            if (errorNumber.HasValue)
            {
                connection.DbConnection.Close();
                throw SqlExceptionFactory.CreateSqlException(errorNumber.Value);
            }

            return result;
        }

        public RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
        {
            var connection = parameterObject.Connection;
            var errorNumber = PreExecution(connection);

            var result = _realRelationalCommand.ExecuteReader(parameterObject);
            if (errorNumber.HasValue)
            {
                connection.DbConnection.Close();
                result.Dispose(); // Normally, in non-test case, reader is disposed by using in caller code
                throw SqlExceptionFactory.CreateSqlException(errorNumber.Value);
            }

            return result;
        }

        public async Task<RelationalDataReader> ExecuteReaderAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
        {
            var connection = parameterObject.Connection;
            var errorNumber = PreExecution(connection);

            var result = await _realRelationalCommand.ExecuteReaderAsync(parameterObject);
            if (errorNumber.HasValue)
            {
                connection.DbConnection.Close();
                result.Dispose(); // Normally, in non-test case, reader is disposed by using in caller code
                throw SqlExceptionFactory.CreateSqlException(errorNumber.Value);
            }

            return result;
        }

        public DbCommand CreateDbCommand(
            RelationalCommandParameterObject parameterObject,
            Guid commandId,
            DbCommandMethod commandMethod)
            => _realRelationalCommand.CreateDbCommand(parameterObject, commandId, commandMethod);

        public void PopulateFrom(IRelationalCommandTemplate commandTemplate)
            => _realRelationalCommand.PopulateFrom(commandTemplate);

        private int? PreExecution(IRelationalConnection connection)
        {
            int? errorNumber = null;
            var testConnection = (TestSqlServerConnection)connection;

            testConnection.ExecutionCount++;
            if (testConnection.ExecutionFailures.Count > 0)
            {
                var fail = testConnection.ExecutionFailures.Dequeue();
                if (fail.HasValue)
                {
                    if (fail.Value)
                    {
                        testConnection.DbConnection.Close();
                        throw SqlExceptionFactory.CreateSqlException(testConnection.ErrorNumber);
                    }

                    errorNumber = testConnection.ErrorNumber;
                }
            }

            return errorNumber;
        }
    }
}
