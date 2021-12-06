// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CSharpHelper : ICSharpHelper
{
    private readonly ITypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpHelper(ITypeMappingSource typeMappingSource)
    {
        _typeMappingSource = typeMappingSource;
    }

    private static readonly IReadOnlyCollection<string> _keywords = new[]
    {
        "__arglist",
        "__makeref",
        "__reftype",
        "__refvalue",
        "abstract",
        "as",
        "base",
        "bool",
        "break",
        "byte",
        "case",
        "catch",
        "char",
        "checked",
        "class",
        "const",
        "continue",
        "decimal",
        "default",
        "delegate",
        "do",
        "double",
        "else",
        "enum",
        "event",
        "explicit",
        "extern",
        "false",
        "finally",
        "fixed",
        "float",
        "for",
        "foreach",
        "goto",
        "if",
        "implicit",
        "in",
        "int",
        "interface",
        "internal",
        "is",
        "lock",
        "long",
        "namespace",
        "new",
        "null",
        "object",
        "operator",
        "out",
        "override",
        "params",
        "private",
        "protected",
        "public",
        "readonly",
        "ref",
        "return",
        "sbyte",
        "sealed",
        "short",
        "sizeof",
        "stackalloc",
        "static",
        "string",
        "struct",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "uint",
        "ulong",
        "unchecked",
        "unsafe",
        "ushort",
        "using",
        "virtual",
        "void",
        "volatile",
        "while"
    };

    private static readonly IReadOnlyDictionary<Type, Func<CSharpHelper, object, string>> _literalFuncs =
        new Dictionary<Type, Func<CSharpHelper, object, string>>
        {
            { typeof(bool), (c, v) => c.Literal((bool)v) },
            { typeof(byte), (c, v) => c.Literal((byte)v) },
            { typeof(byte[]), (c, v) => c.Literal((byte[])v) },
            { typeof(char), (c, v) => c.Literal((char)v) },
            { typeof(DateOnly), (c, v) => c.Literal((DateOnly)v) },
            { typeof(DateTime), (c, v) => c.Literal((DateTime)v) },
            { typeof(DateTimeOffset), (c, v) => c.Literal((DateTimeOffset)v) },
            { typeof(decimal), (c, v) => c.Literal((decimal)v) },
            { typeof(double), (c, v) => c.Literal((double)v) },
            { typeof(float), (c, v) => c.Literal((float)v) },
            { typeof(Guid), (c, v) => c.Literal((Guid)v) },
            { typeof(int), (c, v) => c.Literal((int)v) },
            { typeof(long), (c, v) => c.Literal((long)v) },
            { typeof(NestedClosureCodeFragment), (c, v) => c.Fragment((NestedClosureCodeFragment)v, 0) },
            { typeof(object[]), (c, v) => c.Literal((object[])v) },
            { typeof(object[,]), (c, v) => c.Literal((object[,])v) },
            { typeof(sbyte), (c, v) => c.Literal((sbyte)v) },
            { typeof(short), (c, v) => c.Literal((short)v) },
            { typeof(string), (c, v) => c.Literal((string)v) },
            { typeof(TimeOnly), (c, v) => c.Literal((TimeOnly)v) },
            { typeof(TimeSpan), (c, v) => c.Literal((TimeSpan)v) },
            { typeof(uint), (c, v) => c.Literal((uint)v) },
            { typeof(ulong), (c, v) => c.Literal((ulong)v) },
            { typeof(ushort), (c, v) => c.Literal((ushort)v) },
            { typeof(BigInteger), (c, v) => c.Literal((BigInteger)v) },
            { typeof(Type), (c, v) => c.Literal((Type)v) }
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Lambda(IReadOnlyList<string> properties, string? lambdaIdentifier)
    {
        lambdaIdentifier ??= "x";
        var builder = new StringBuilder();
        builder.Append(lambdaIdentifier);
        builder.Append(" => ");

        if (properties.Count == 1)
        {
            builder
                .Append(lambdaIdentifier)
                .Append('.')
                .Append(properties[0]);
        }
        else
        {
            builder.Append("new { ");
            builder.AppendJoin(", ", properties.Select(p => $"{lambdaIdentifier}.{p}"));
            builder.Append(" }");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Reference(Type type, bool? fullName = null)
    {
        fullName ??= type.IsNested ? ShouldUseFullName(type.DeclaringType!) : ShouldUseFullName(type);

        return type.DisplayName(fullName.Value, compilable: true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldUseFullName(Type type)
        => ShouldUseFullName(type.Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldUseFullName(string shortTypeName)
        => false;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Identifier(string name, ICollection<string>? scope = null, bool? capitalize = null)
    {
        var builder = new StringBuilder();
        var partStart = 0;

        for (var i = 0; i < name.Length; i++)
        {
            if (!IsIdentifierPartCharacter(name[i]))
            {
                if (partStart != i)
                {
                    builder.Append(name, partStart, i - partStart);
                }

                partStart = i + 1;
            }
        }

        if (partStart != name.Length)
        {
            builder.Append(name.Substring(partStart));
        }

        if (builder.Length == 0
            || !IsIdentifierStartCharacter(builder[0]))
        {
            builder.Insert(0, '_');
        }

        if (capitalize != null)
        {
            ChangeFirstLetterCase(builder, capitalize.Value);
        }

        var identifier = builder.ToString();
        if (scope != null)
        {
            var uniqueIdentifier = identifier;
            var qualifier = 0;
            while (scope.Contains(uniqueIdentifier))
            {
                uniqueIdentifier = identifier + qualifier++;
            }

            scope.Add(uniqueIdentifier);
            identifier = uniqueIdentifier;
        }

        return _keywords.Contains(identifier) ? "@" + identifier : identifier;
    }

    private static StringBuilder ChangeFirstLetterCase(StringBuilder builder, bool capitalize)
    {
        if (builder.Length == 0)
        {
            return builder;
        }

        var first = builder[index: 0];
        if (char.IsUpper(first) == capitalize)
        {
            return builder;
        }

        builder.Remove(startIndex: 0, length: 1)
            .Insert(index: 0, value: capitalize ? char.ToUpperInvariant(first) : char.ToLowerInvariant(first));

        return builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Namespace(params string[] name)
    {
        var @namespace = new StringBuilder();
        foreach (var piece in name.Where(p => !string.IsNullOrEmpty(p))
                     .SelectMany(p => p.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)))
        {
            var identifier = Identifier(piece);
            if (!string.IsNullOrEmpty(identifier))
            {
                @namespace.Append(identifier)
                    .Append('.');
            }
        }

        return @namespace.Length > 0 ? @namespace.Remove(@namespace.Length - 1, 1).ToString() : "_";
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(string value)
        // do not use @"" syntax as in Migrations this can get indented at a newline and so add spaces to the literal
        => "\"" + value.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(bool value)
        => value ? "true" : "false";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(byte value)
        => "(byte)" + value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(char value)
        => "\'" + (value == '\'' ? "\\'" : value.ToString()) + "\'";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(DateOnly value)
        => string.Format(
            CultureInfo.InvariantCulture,
            "new DateOnly({0}, {1}, {2})",
            value.Year,
            value.Month,
            value.Day);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(DateTime value)
        => string.Format(
                CultureInfo.InvariantCulture,
                "new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.{7})",
                value.Year,
                value.Month,
                value.Day,
                value.Hour,
                value.Minute,
                value.Second,
                value.Millisecond,
                value.Kind)
            + (value.Ticks % 10000 == 0
                ? ""
                : string.Format(
                    CultureInfo.InvariantCulture,
                    ".AddTicks({0})",
                    value.Ticks % 10000));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(DateTimeOffset value)
        => "new DateTimeOffset(" + Literal(value.DateTime) + ", " + Literal(value.Offset) + ")";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(decimal value)
        => value.ToString(CultureInfo.InvariantCulture) + "m";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(double value)
        => EnsureDecimalPlaces(value);

    private static string EnsureDecimalPlaces(double number)
    {
        var literal = number.ToString("G17", CultureInfo.InvariantCulture);

        if (double.IsNaN(number))
        {
            return $"double.{nameof(double.NaN)}";
        }

        if (double.IsNegativeInfinity(number))
        {
            return $"double.{nameof(double.NegativeInfinity)}";
        }

        if (double.IsPositiveInfinity(number))
        {
            return $"double.{nameof(double.PositiveInfinity)}";
        }

        return !literal.Contains('E')
            && !literal.Contains('e')
            && !literal.Contains('.')
                ? literal + ".0"
                : literal;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(float value)
        => value.ToString(CultureInfo.InvariantCulture) + "f";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(Guid value)
        => "new Guid(\"" + value + "\")";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(int value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(long value)
        => value.ToString(CultureInfo.InvariantCulture) + "L";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(sbyte value)
        => "(sbyte)" + value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(short value)
        => "(short)" + value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(TimeOnly value)
    {
        var result = value.Millisecond == 0
            ? string.Format(
                CultureInfo.InvariantCulture, "new TimeOnly({0}, {1}, {2})", value.Hour, value.Minute, value.Second)
            : string.Format(
                CultureInfo.InvariantCulture, "new TimeOnly({0}, {1}, {2}, {3})", value.Hour, value.Minute, value.Second,
                value.Millisecond);

        if (value.Ticks % 10000 > 0)
        {
            result += string.Format(
                CultureInfo.InvariantCulture,
                ".Add(TimeSpan.FromTicks({0}))",
                value.Ticks % 10000);
        }

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(TimeSpan value)
        => value.Ticks % 10000 == 0
            ? string.Format(
                CultureInfo.InvariantCulture,
                "new TimeSpan({0}, {1}, {2}, {3}, {4})",
                value.Days,
                value.Hours,
                value.Minutes,
                value.Seconds,
                value.Milliseconds)
            : string.Format(
                CultureInfo.InvariantCulture,
                "new TimeSpan({0})",
                value.Ticks);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(uint value)
        => value + "u";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(ulong value)
        => value + "ul";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(ushort value)
        => "(ushort)" + value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(BigInteger value)
        => $"BigInteger.Parse(\"{value.ToString(NumberFormatInfo.InvariantInfo)}\", NumberFormatInfo.InvariantInfo)";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(Type value, bool? useFullName = null)
        => $"typeof({Reference(value, useFullName)})";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal<T>(T? value)
        where T : struct
        => UnknownLiteral(value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal<T>(T[] values, bool vertical = false)
        => Array(typeof(T), values, vertical);

    private string Array(Type type, IEnumerable values, bool vertical = false)
    {
        var builder = new IndentedStringBuilder();

        builder.Append("new");

        var valuesList = values.Cast<object?>().ToList();

        if (valuesList.Count == 0)
        {
            builder
                .Append(" ")
                .Append(Reference(type))
                .Append("[0]");
        }
        else
        {
            var byteArray = type == typeof(byte);
            if (byteArray)
            {
                builder.Append(" byte");
            }
            else if (type == typeof(object))
            {
                builder.Append(" object");
            }

            if (vertical)
            {
                builder.AppendLine("[]");
            }
            else
            {
                builder.Append("[] ");
            }

            builder.Append("{");

            if (vertical)
            {
                builder.AppendLine();
                builder.IncrementIndent();
            }
            else
            {
                builder.Append(" ");
            }

            var first = true;
            foreach (var value in valuesList)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(",");

                    if (vertical)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.Append(" ");
                    }
                }

                builder.Append(
                    byteArray
                        ? Literal((int)(byte)value!)
                        : UnknownLiteral(value));
            }

            if (vertical)
            {
                builder.AppendLine();
                builder.DecrementIndent();
            }
            else
            {
                builder.Append(" ");
            }

            builder.Append("}");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(object?[,] values)
    {
        var builder = new IndentedStringBuilder();

        builder
            .AppendLine("new object[,]")
            .AppendLine("{");

        using (builder.Indent())
        {
            var rowCount = values.GetLength(0);
            var valueCount = values.GetLength(1);
            for (var i = 0; i < rowCount; i++)
            {
                if (i != 0)
                {
                    builder.AppendLine(",");
                }

                builder.Append("{ ");

                for (var j = 0; j < valueCount; j++)
                {
                    if (j != 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(UnknownLiteral(values[i, j]));
                }

                builder.Append(" }");
            }
        }

        builder
            .AppendLine()
            .Append("}");

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal(Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);

        return name == null
            ? GetCompositeEnumValue(type, value)
            : GetSimpleEnumValue(type, name);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetSimpleEnumValue(Type type, string name)
        => Reference(type) + "." + name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetCompositeEnumValue(Type type, Enum flags)
    {
        var allValues = new HashSet<Enum>(GetFlags(flags));
        foreach (var currentValue in allValues.ToList())
        {
            var decomposedValues = GetFlags(currentValue);
            if (decomposedValues.Count > 1)
            {
                allValues.ExceptWith(decomposedValues.Where(v => !Equals(v, currentValue)));
            }
        }

        return allValues.Aggregate(
            (string?)null,
            (previous, current) =>
                previous == null
                    ? GetSimpleEnumValue(type, Enum.GetName(type, current)!)
                    : previous + " | " + GetSimpleEnumValue(type, Enum.GetName(type, current)!))!;
    }

    internal static IReadOnlyCollection<Enum> GetFlags(Enum flags)
    {
        var values = new List<Enum>();
        var type = flags.GetType();
        var defaultValue = Enum.ToObject(type, value: 0);
        foreach (Enum currValue in Enum.GetValues(type))
        {
            if (currValue.Equals(defaultValue))
            {
                continue;
            }

            if (flags.HasFlag(currValue))
            {
                values.Add(currValue);
            }
        }

        return values;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string UnknownLiteral(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        var literalType = value.GetType();

        if (_literalFuncs.TryGetValue(literalType.UnwrapNullableType(), out var literalFunc))
        {
            return literalFunc(this, value);
        }

        if (value is Enum enumValue)
        {
            return Literal(enumValue);
        }

        if (value is Type type)
        {
            return Literal(type);
        }

        if (value is Array array)
        {
            return Array(literalType.GetElementType()!, array);
        }

        var mapping = _typeMappingSource.FindMapping(literalType);
        if (mapping != null)
        {
            var builder = new StringBuilder();
            var expression = mapping.GenerateCodeLiteral(value);
            var handled = HandleExpression(expression, builder);

            if (!handled)
            {
                throw new NotSupportedException(
                    DesignStrings.LiteralExpressionNotSupported(
                        expression.ToString(),
                        literalType.ShortDisplayName()));
            }

            return builder.ToString();
        }

        throw new InvalidOperationException(DesignStrings.UnknownLiteral(literalType));
    }

    private bool HandleExpression(Expression expression, StringBuilder builder, bool simple = false)
    {
        // Only handle trivially simple cases for `new` and factory methods
        switch (expression.NodeType)
        {
            case ExpressionType.NewArrayInit:
                builder
                    .Append("new ")
                    .Append(Reference(expression.Type.GetElementType()!))
                    .Append("[] { ");

                HandleList(((NewArrayExpression)expression).Expressions, builder, simple: true);

                builder
                    .Append(" }");

                return true;
            case ExpressionType.Convert:
                builder
                    .Append('(')
                    .Append(Reference(expression.Type, fullName: true))
                    .Append(')');

                return HandleExpression(((UnaryExpression)expression).Operand, builder);
            case ExpressionType.New:
                builder
                    .Append("new ")
                    .Append(Reference(expression.Type, fullName: true));

                return HandleArguments(((NewExpression)expression).Arguments, builder);
            case ExpressionType.Call:
            {
                var callExpression = (MethodCallExpression)expression;
                if (callExpression.Method.IsStatic)
                {
                    builder
                        .Append(Reference(callExpression.Method.DeclaringType!, fullName: true));
                }
                else
                {
                    if (!HandleExpression(callExpression.Object!, builder))
                    {
                        return false;
                    }
                }

                builder
                    .Append('.')
                    .Append(callExpression.Method.Name);

                return HandleArguments(callExpression.Arguments, builder);
            }
            case ExpressionType.Constant:
            {
                var value = ((ConstantExpression)expression).Value;

                builder
                    .Append(
                        simple
                        && value?.GetType()?.IsNumeric() == true
                            ? value
                            : UnknownLiteral(value));
                return true;
            }
            case ExpressionType.MemberAccess:
            {
                var memberExpression = (MemberExpression)expression;
                if (memberExpression.Expression == null)
                {
                    builder
                        .Append(Reference(memberExpression.Member.DeclaringType!, fullName: true));
                }
                else
                {
                    if (!HandleExpression(memberExpression.Expression, builder))
                    {
                        return false;
                    }
                }

                builder
                    .Append('.')
                    .Append(memberExpression.Member.Name);

                return true;
            }
            case ExpressionType.Add:
            {
                var binaryExpression = (BinaryExpression)expression;
                if (!HandleExpression(binaryExpression.Left, builder))
                {
                    return false;
                }

                builder.Append(" + ");

                return HandleExpression(binaryExpression.Right, builder);
            }
        }

        return false;
    }

    private bool HandleArguments(IEnumerable<Expression> argumentExpressions, StringBuilder builder)
    {
        builder.Append('(');

        if (!HandleList(argumentExpressions, builder))
        {
            return false;
        }

        builder.Append(')');

        return true;
    }

    private bool HandleList(IEnumerable<Expression> argumentExpressions, StringBuilder builder, bool simple = false)
    {
        var separator = string.Empty;
        foreach (var expression in argumentExpressions)
        {
            builder.Append(separator);

            if (!HandleExpression(expression, builder, simple))
            {
                return false;
            }

            separator = ", ";
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Fragment(MethodCallCodeFragment fragment, string? instanceIdentifier = null, bool typeQualified = false)
        => Fragment(fragment, typeQualified, instanceIdentifier, indent: 0);

    private string Fragment(MethodCallCodeFragment fragment, bool typeQualified, string? instanceIdentifier, int indent)
    {
        var builder = new IndentedStringBuilder();
        var current = fragment;

        if (typeQualified)
        {
            if (instanceIdentifier is null || fragment.MethodInfo is null || fragment.ChainedCall is not null)
            {
                throw new ArgumentException(DesignStrings.CannotGenerateTypeQualifiedMethodCall);
            }

            builder
                .Append(fragment.DeclaringType!)
                .Append('.')
                .Append(fragment.Method)
                .Append('(')
                .Append(instanceIdentifier);

            for (var i = 0; i < fragment.Arguments.Count; i++)
            {
                builder.Append(", ");
                Argument(fragment.Arguments[i]);
            }

            builder.Append(')');

            return builder.ToString();
        }

        // Non-type-qualified fragment

        if (instanceIdentifier is not null)
        {
            builder.Append(instanceIdentifier);

            if (current.ChainedCall is not null)
            {
                builder
                    .AppendLine()
                    .IncrementIndent();
            }
        }

        while (true)
        {
            builder
                .Append('.')
                .Append(current.Method)
                .Append('(');

            for (var i = 0; i < current.Arguments.Count; i++)
            {
                if (i != 0)
                {
                    builder.Append(", ");
                }

                Argument(current.Arguments[i]);
            }

            builder.Append(')');

            if (current.ChainedCall is null)
            {
                break;
            }

            builder.AppendLine();
            current = current.ChainedCall;
        }

        return builder.ToString();

        void Argument(object? argument)
        {
            if (argument is NestedClosureCodeFragment nestedFragment)
            {
                builder.Append(Fragment(nestedFragment, indent));
            }
            else
            {
                builder.Append(UnknownLiteral(argument));
            }
        }
    }

    private string Fragment(NestedClosureCodeFragment fragment, int indent)
    {
        if (fragment.MethodCalls.Count == 1)
        {
            return fragment.Parameter + " => " + Fragment(fragment.MethodCalls[0], typeQualified: false, fragment.Parameter, indent);
        }

        var builder = new IndentedStringBuilder();
        builder.AppendLine(fragment.Parameter + " =>");
        for (var i = -1; i < indent; i++)
        {
            builder.IncrementIndent();
        }

        builder.AppendLine("{");
        using (builder.Indent())
        {
            foreach (var methodCall in fragment.MethodCalls)
            {
                builder.AppendLines(Fragment(methodCall, typeQualified: false, fragment.Parameter, indent + 1), skipFinalNewline: true);
                builder.AppendLine(";");
            }
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    private static bool IsIdentifierStartCharacter(char ch)
    {
        if (ch < 'a')
        {
            return ch < 'A'
                ? false
                : ch <= 'Z'
                || ch == '_';
        }

        if (ch <= 'z')
        {
            return true;
        }

        return ch <= '\u007F' ? false : IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
    }

    private static bool IsIdentifierPartCharacter(char ch)
    {
        if (ch < 'a')
        {
            return ch < 'A'
                ? ch >= '0'
                && ch <= '9'
                : ch <= 'Z'
                || ch == '_';
        }

        if (ch <= 'z')
        {
            return true;
        }

        if (ch <= '\u007F')
        {
            return false;
        }

        var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
        if (IsLetterChar(cat))
        {
            return true;
        }

        switch (cat)
        {
            case UnicodeCategory.DecimalDigitNumber:
            case UnicodeCategory.ConnectorPunctuation:
            case UnicodeCategory.NonSpacingMark:
            case UnicodeCategory.SpacingCombiningMark:
            case UnicodeCategory.Format:
                return true;
        }

        return false;
    }

    private static bool IsLetterChar(UnicodeCategory cat)
    {
        switch (cat)
        {
            case UnicodeCategory.UppercaseLetter:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.TitlecaseLetter:
            case UnicodeCategory.ModifierLetter:
            case UnicodeCategory.OtherLetter:
            case UnicodeCategory.LetterNumber:
                return true;
        }

        return false;
    }
}
