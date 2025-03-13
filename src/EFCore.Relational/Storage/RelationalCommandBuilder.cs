// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <inheritdoc />
public class RelationalCommandBuilder : IRelationalCommandBuilder
{
    private readonly List<IRelationalParameter> _parameters = [];
    private readonly IndentedStringBuilder _commandTextBuilder = new();
    private IndentedStringBuilder? _logCommandTextBuilder;

    /// <summary>
    ///     <para>
    ///         Constructs a new <see cref="RelationalCommand" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public RelationalCommandBuilder(
        RelationalCommandBuilderDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalCommandBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    [Obsolete("Code trying to add parameter should add type mapped parameter using TypeMappingSource directly.")]
    public virtual IRelationalTypeMappingSource TypeMappingSource
        => Dependencies.TypeMappingSource;

    /// <inheritdoc />
    public virtual IRelationalCommand Build()
    {
        var commandText = _commandTextBuilder.ToString();
        var logCommandText = _logCommandTextBuilder?.ToString() ?? commandText;
        return new RelationalCommand(Dependencies, commandText, logCommandText, Parameters);
    }

    /// <summary>
    ///     Gets the command text.
    /// </summary>
    public override string ToString()
        => _commandTextBuilder.ToString();

    /// <inheritdoc />
    public virtual IReadOnlyList<IRelationalParameter> Parameters
        => _parameters;

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder AddParameter(IRelationalParameter parameter)
    {
        _parameters.Add(parameter);

        return this;
    }

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder RemoveParameterAt(int index)
    {
        _parameters.RemoveAt(index);

        return this;
    }

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder Append(string value, bool redact = false)
    {
        InitializeLogCommandTextBuilderIfNeeded(redact);
        _commandTextBuilder.Append(value);
        _logCommandTextBuilder?.Append(redact ? "?" : value);

        return this;
    }

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder Append(FormattableString value, bool redact = false)
    {
        InitializeLogCommandTextBuilderIfNeeded(redact);
        _commandTextBuilder.Append(value);
        _logCommandTextBuilder?.Append(redact ? $"?" : value);

        return this;
    }

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder AppendLine()
    {
        _commandTextBuilder.AppendLine();
        _logCommandTextBuilder?.AppendLine();

        return this;
    }

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder IncrementIndent()
    {
        _commandTextBuilder.IncrementIndent();
        _logCommandTextBuilder?.IncrementIndent();

        return this;
    }

    /// <inheritdoc />
    public virtual IRelationalCommandBuilder DecrementIndent()
    {
        _commandTextBuilder.DecrementIndent();
        _logCommandTextBuilder?.DecrementIndent();

        return this;
    }

    /// <inheritdoc />
    public virtual int CommandTextLength
        => _commandTextBuilder.Length;

    private void InitializeLogCommandTextBuilderIfNeeded(bool redact)
    {
        if (redact
            && _logCommandTextBuilder is null
            && !Dependencies.LoggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _logCommandTextBuilder = _commandTextBuilder.Clone();
        }
    }
}
