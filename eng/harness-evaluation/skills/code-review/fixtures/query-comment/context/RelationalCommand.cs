using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Storage;

public partial class RelationalCommand
{
    private DbCommand CreateDbCommand(RelationalCommandParameterObject parameterObject)
    {
        var command = parameterObject.Connection.DbConnection.CreateCommand();
        command.CommandText = CommandText;
        foreach (var parameter in Parameters)
        {
            parameter.AddDbParameter(command, parameterObject.ParameterValues);
        }

        return command;
    }

    public virtual RelationalDataReader ExecuteReader(RelationalCommandParameterObject parameterObject)
    {
        var command = CreateDbCommand(parameterObject);
        var reader = command.ExecuteReader();
        return new RelationalDataReader(parameterObject.Connection, command, reader);
    }

    public virtual async Task<RelationalDataReader> ExecuteReaderAsync(
        RelationalCommandParameterObject parameterObject,
        CancellationToken cancellationToken = default)
    {
        var command = CreateDbCommand(parameterObject);
        var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return new RelationalDataReader(parameterObject.Connection, command, reader);
    }
}