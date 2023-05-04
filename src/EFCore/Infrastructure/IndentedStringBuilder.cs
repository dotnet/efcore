// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         A thin wrapper over <see cref="StringBuilder" /> that adds indentation to each line built.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class IndentedStringBuilder
{
    private const byte IndentSize = 4;
    private int _indent;
    private bool _indentPending = true;

    private readonly StringBuilder _stringBuilder = new();

    /// <summary>
    ///     Gets the current indent level.
    /// </summary>
    /// <value>The current indent level.</value>
    public virtual int IndentCount
        => _indent;

    /// <summary>
    ///     The current length of the built string.
    /// </summary>
    public virtual int Length
        => _stringBuilder.Length;

    /// <summary>
    ///     Appends the current indent and then the given string to the string being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder Append(string value)
    {
        DoIndent();

        _stringBuilder.Append(value);

        return this;
    }

    /// <summary>
    ///     Appends the current indent and then the given string to the string being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder Append(FormattableString value)
    {
        DoIndent();

        _stringBuilder.Append(value);

        return this;
    }

    /// <summary>
    ///     Appends the current indent and then the given char to the string being built.
    /// </summary>
    /// <param name="value">The char to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder Append(char value)
    {
        DoIndent();

        _stringBuilder.Append(value);

        return this;
    }

    /// <summary>
    ///     Appends the current indent and then the given strings to the string being built.
    /// </summary>
    /// <param name="value">The strings to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder Append(IEnumerable<string> value)
    {
        DoIndent();

        foreach (var str in value)
        {
            _stringBuilder.Append(str);
        }

        return this;
    }

    /// <summary>
    ///     Appends the current indent and then the given chars to the string being built.
    /// </summary>
    /// <param name="value">The chars to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder Append(IEnumerable<char> value)
    {
        DoIndent();

        foreach (var chr in value)
        {
            _stringBuilder.Append(chr);
        }

        return this;
    }

    /// <summary>
    ///     Appends a new line to the string being built.
    /// </summary>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder AppendLine()
    {
        AppendLine(string.Empty);

        return this;
    }

    /// <summary>
    ///     Appends the current indent, the given string, and a new line to the string being built.
    /// </summary>
    /// <remarks>
    ///     If the given string itself contains a new line, the part of the string after that new line will not be indented.
    /// </remarks>
    /// <param name="value">The string to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder AppendLine(string value)
    {
        if (value.Length != 0)
        {
            DoIndent();
        }

        _stringBuilder.AppendLine(value);

        _indentPending = true;

        return this;
    }

    /// <summary>
    ///     Appends the current indent, the given string, and a new line to the string being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder AppendLine(FormattableString value)
    {
        DoIndent();

        _stringBuilder.Append(value);

        _indentPending = true;

        return this;
    }

    /// <summary>
    ///     Separates the given string into lines, and then appends each line, prefixed
    ///     by the current indent and followed by a new line, to the string being built.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <param name="skipFinalNewline">If <see langword="true" />, then the terminating new line is not added after the last line.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder AppendLines(string value, bool skipFinalNewline = false)
    {
        using (var reader = new StringReader(value))
        {
            var first = true;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    AppendLine();
                }

                if (line.Length != 0)
                {
                    Append(line);
                }
            }
        }

        if (!skipFinalNewline)
        {
            AppendLine();
        }

        return this;
    }

    /// <summary>
    ///     Concatenates the members of the given collection, using the specified separator between each member,
    ///     and then appends the resulting string,
    /// </summary>
    /// <param name="values">The values to concatenate.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder AppendJoin(
        IEnumerable<string> values,
        string separator = ", ")
    {
        DoIndent();

        _stringBuilder.AppendJoin(values, separator);

        return this;
    }

    /// <summary>
    ///     Concatenates the members of the given collection, using the specified separator between each member,
    ///     and then appends the resulting string,
    /// </summary>
    /// <param name="values">The values to concatenate.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder AppendJoin(
        string separator,
        params string[] values)
    {
        DoIndent();

        _stringBuilder.AppendJoin(separator, values);

        return this;
    }

    /// <summary>
    ///     Resets this builder ready to build a new string.
    /// </summary>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder Clear()
    {
        _stringBuilder.Clear();
        _indent = 0;

        return this;
    }

    /// <summary>
    ///     Increments the indent.
    /// </summary>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder IncrementIndent()
    {
        _indent++;

        return this;
    }

    /// <summary>
    ///     Decrements the indent.
    /// </summary>
    /// <returns>This builder so that additional calls can be chained.</returns>
    public virtual IndentedStringBuilder DecrementIndent()
    {
        if (_indent > 0)
        {
            _indent--;
        }

        return this;
    }

    /// <summary>
    ///     Creates a scoped indenter that will increment the indent, then decrement it when disposed.
    /// </summary>
    /// <returns>An indenter.</returns>
    public virtual IDisposable Indent()
        => new Indenter(this);

    /// <summary>
    ///     Temporarily disables all indentation. Restores the original indentation when the returned object is disposed.
    /// </summary>
    /// <returns>An object that restores the original indentation when disposed.</returns>
    public virtual IDisposable SuspendIndent()
        => new IndentSuspender(this);

    /// <summary>
    ///     Returns the built string.
    /// </summary>
    /// <returns>The built string.</returns>
    public override string ToString()
        => _stringBuilder.ToString();

    private void DoIndent()
    {
        if (_indentPending && _indent > 0)
        {
            _stringBuilder.Append(' ', _indent * IndentSize);
        }

        _indentPending = false;
    }

    private sealed class Indenter : IDisposable
    {
        private readonly IndentedStringBuilder _stringBuilder;

        public Indenter(IndentedStringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;

            _stringBuilder.IncrementIndent();
        }

        public void Dispose()
            => _stringBuilder.DecrementIndent();
    }

    private sealed class IndentSuspender : IDisposable
    {
        private readonly IndentedStringBuilder _stringBuilder;
        private readonly int _indent;

        public IndentSuspender(IndentedStringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
            _indent = _stringBuilder._indent;
            _stringBuilder._indent = 0;
        }

        public void Dispose()
            => _stringBuilder._indent = _indent;
    }
}
