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
        internal IRelationalCommand? SizeProbe { get; set; }
        internal IRelationalCommand? SmallTableQuery { get; set; }

        public override IRelationalCommand Build()
        {
            // An empty builder creates the command which a connection rents for executing query templates.
            if (CommandTextLength != 0 && SizeProbe == null)
            {
                return base.Build();
            }

            var command = base.Build();
            return new Command(Dependencies, command, SizeProbe, SmallTableQuery);
        }
    }

    private sealed class Command(
        RelationalCommandBuilderDependencies dependencies,
        IRelationalCommandTemplate template,
        IRelationalCommand? sizeProbe,
        IRelationalCommand? smallTableQuery)
        : RelationalCommand(dependencies, template.CommandText, template.LogCommandText, template.Parameters)
    {
        private IRelationalCommand? _sizeProbe = sizeProbe;
        private IRelationalCommand? _smallTableQuery = smallTableQuery;

        public override void PopulateFrom(IRelationalCommandTemplate commandTemplate)
        {
            base.PopulateFrom(commandTemplate);
            _sizeProbe = (commandTemplate as Command)?._sizeProbe;
            _smallTableQuery = (commandTemplate as Command)?._smallTableQuery;
        }

        public override RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
        {
            if (_sizeProbe == null)
            {
                return base.ExecuteReader(parameterObject);
            }

            var connection = parameterObject.Connection;
            connection.Open();
            try
            {
                return _sizeProbe.ExecuteScalar(parameterObject) == null
                    ? CreateSmallTableCommand().ExecuteReader(parameterObject)
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
            => _sizeProbe == null
                ? base.ExecuteReaderAsync(parameterObject, cancellationToken)
                : ExecuteWithSizeProbeAsync(parameterObject, cancellationToken);

        // The template is shared between contexts. Each active reader needs its own command.
        private RelationalCommand CreateSmallTableCommand()
            => new(Dependencies, _smallTableQuery!.CommandText, _smallTableQuery.LogCommandText, _smallTableQuery.Parameters);

        private async Task<RelationalDataReader> ExecuteWithSizeProbeAsync(
            RelationalCommandParameterObject parameterObject,
            CancellationToken cancellationToken)
        {
            var connection = parameterObject.Connection;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var size = await _sizeProbe!.ExecuteScalarAsync(parameterObject, cancellationToken).ConfigureAwait(false);
                return size == null
                    ? await CreateSmallTableCommand().ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false)
                    : await base.ExecuteReaderAsync(parameterObject, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
