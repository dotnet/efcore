// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

internal sealed class SqliteRelationalCommandBuilderFactory(RelationalCommandBuilderDependencies dependencies)
    : RelationalCommandBuilderFactory(dependencies)
{
    public override IRelationalCommandBuilder Create()
        => new Builder(Dependencies);

    internal sealed class Builder(RelationalCommandBuilderDependencies dependencies) : RelationalCommandBuilder(dependencies)
    {
        internal IRelationalCommand? IndexProbe { get; set; }
        internal IRelationalCommand? ScanQuery { get; set; }

        public override IRelationalCommand Build()
        {
            // An empty builder creates the command which a connection rents for executing query templates.
            if (CommandTextLength != 0 && IndexProbe == null)
            {
                return base.Build();
            }

            var command = base.Build();
            return new Command(Dependencies, command, IndexProbe, ScanQuery);
        }
    }

    private sealed class Command(
        RelationalCommandBuilderDependencies dependencies,
        IRelationalCommandTemplate template,
        IRelationalCommand? indexProbe,
        IRelationalCommand? scanQuery)
        : RelationalCommand(dependencies, template.CommandText, template.LogCommandText, template.Parameters)
    {
        private IRelationalCommand? _indexProbe = indexProbe;
        private IRelationalCommand? _scanQuery = scanQuery;

        public override void PopulateFrom(IRelationalCommandTemplate commandTemplate)
        {
            base.PopulateFrom(commandTemplate);
            _indexProbe = (commandTemplate as Command)?._indexProbe;
            _scanQuery = (commandTemplate as Command)?._scanQuery;
        }

        public override RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
        {
            if (_indexProbe == null)
            {
                return base.ExecuteReader(parameterObject);
            }

            var connection = parameterObject.Connection;
            connection.Open();
            try
            {
                return _indexProbe.ExecuteScalar(parameterObject) == null
                    ? CreateScanCommand().ExecuteReader(parameterObject)
                    : base.ExecuteReader(parameterObject);
            }
            finally
            {
                connection.Close();
            }
        }

        public override Task<RelationalDataReader> ExecuteReaderAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken = default)
            => _indexProbe == null
                ? base.ExecuteReaderAsync(parameterObject, cancellationToken)
                : ExecuteWithIndexProbeAsync(parameterObject, cancellationToken);

        // The template is shared between contexts. Each active reader needs its own command.
        private RelationalCommand CreateScanCommand()
            => new(Dependencies, _scanQuery!.CommandText, _scanQuery.LogCommandText, _scanQuery.Parameters);

        private async Task<RelationalDataReader> ExecuteWithIndexProbeAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken)
        {
            var connection = parameterObject.Connection;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var probe = await _indexProbe!.ExecuteScalarAsync(parameterObject, cancellationToken).ConfigureAwait(false);
                return probe == null
                    ? await CreateScanCommand().ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false)
                    : await base.ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
