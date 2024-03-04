// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

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
    private readonly Project _project;
    private readonly RuntimeModelLinqToCSharpSyntaxTranslator _translator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpHelper(ITypeMappingSource typeMappingSource)
    {
        _typeMappingSource = typeMappingSource;

        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var versionStamp = VersionStamp.Create();
        var projectInfo = ProjectInfo.Create(projectId, versionStamp, "Proj", "Proj", LanguageNames.CSharp);
        _project = workspace.AddProject(projectInfo);
        var syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
        _translator = new RuntimeModelLinqToCSharpSyntaxTranslator(syntaxGenerator);
    }

    private static readonly IReadOnlyCollection<string> Keywords =
    [
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
    ];

    private static readonly IReadOnlyDictionary<Type, Func<CSharpHelper, object, string>> LiteralFuncs =
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
            { typeof(NestedClosureCodeFragment), (c, v) => c.Fragment((NestedClosureCodeFragment)v) },
            { typeof(PropertyAccessorCodeFragment), (c, v) => c.Fragment((PropertyAccessorCodeFragment)v) },
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
    protected virtual bool ShouldUseFullName(Type type)
        => ShouldUseFullName(type.Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool ShouldUseFullName(string shortTypeName)
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
            builder.Append(name[partStart..]);
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

        return Keywords.Contains(identifier) ? "@" + identifier : identifier;
    }

    private static void ChangeFirstLetterCase(StringBuilder builder, bool capitalize)
    {
        if (builder.Length == 0)
        {
            return;
        }

        var first = builder[index: 0];
        if (char.IsUpper(first) == capitalize)
        {
            return;
        }

        builder.Remove(startIndex: 0, length: 1)
            .Insert(index: 0, value: capitalize ? char.ToUpperInvariant(first) : char.ToLowerInvariant(first));
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
                     .SelectMany(p => p.Split('.', StringSplitOptions.RemoveEmptyEntries)))
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
    public virtual string Literal(string? value)
        // do not output @"" syntax as in Migrations this can get indented at a newline and so add spaces to the literal
        => value is not null
            ? new StringBuilder(value)
                .Replace("\\", @"\\")
                .Replace("\0", @"\0")
                .Replace("\n", @"\n")
                .Replace("\r", @"\r")
                .Replace("\"", "\\\"")
                .Insert(0, '"')
                .Append('"')
                .ToString()
            : "null";

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
        => "\'"
            + value switch
            {
                '\\' => @"\\",
                '\0' => @"\0",
                '\n' => @"\n",
                '\r' => @"\r",
                '\'' => @"\'",
                _ => value.ToString()
            }
            + "\'";

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

    private string ValueTuple(ITuple tuple)
    {
        var builder = new StringBuilder();

        Type[]? typeArguments = null;
        var i = 0;

        if (tuple.Length == 1)
        {
            builder.Append("ValueTuple.Create(");
            AppendItem(tuple[i]);
            builder.Append(')');

            return builder.ToString();
        }

        builder.Append('(');

        for (; i < tuple.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            AppendItem(tuple[i]);
        }

        builder.Append(')');

        return builder.ToString();

        void AppendItem(object? item)
        {
            if (item is null)
            {
                typeArguments ??= tuple.GetType().GenericTypeArguments;

                builder
                    .Append('(')
                    .Append(Reference(typeArguments[i]))
                    .Append(')');
            }

            builder.Append(UnknownLiteral(item));
        }
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
    public virtual string Literal<T>(List<T> values, bool vertical = false)
        => List(typeof(T), values, vertical);

    private string List(Type type, IEnumerable values, bool vertical = false)
    {
        var builder = new IndentedStringBuilder();

        builder.Append("new List<")
            .Append(Reference(type))
            .Append(">");

        return HandleEnumerable(
            builder, vertical, values, value =>
            {
                builder.Append(UnknownLiteral(value));
            });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Literal<TKey, TValue>(Dictionary<TKey, TValue> dict, bool vertical = false)
        where TKey : notnull
        => Dictionary(typeof(TKey), typeof(TValue), dict, vertical);

    private string Dictionary(Type keyType, Type valueType, IDictionary dict, bool vertical = false)
    {
        var builder = new IndentedStringBuilder();

        builder.Append("new Dictionary<")
            .Append(Reference(keyType))
            .Append(", ")
            .Append(Reference(valueType))
            .Append(">");

        return HandleEnumerable(
            builder, vertical, dict.Keys, key =>
            {
                builder.Append("[")
                    .Append(UnknownLiteral(key))
                    .Append("] = ")
                    .Append(UnknownLiteral(dict[key]));
            });
    }

    private static string HandleEnumerable(IndentedStringBuilder builder, bool vertical, IEnumerable values, Action<object> handleValue)
    {
        var first = true;
        foreach (var value in values)
        {
            if (first)
            {
                if (vertical)
                {
                    builder.AppendLine();
                }
                else
                {
                    builder.Append(" ");
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

            handleValue(value);
        }

        if (first)
        {
            builder.Append("()");
        }
        else
        {
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
    public virtual string Literal(Enum value, bool fullName = false)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);

        return name == null
            ? type.IsDefined(typeof(FlagsAttribute), false)
                ? GetCompositeEnumValue(type, value, fullName)
                : $"({Reference(type)}){UnknownLiteral(Convert.ChangeType(value, Enum.GetUnderlyingType(type)))}"
            : GetSimpleEnumValue(type, name, fullName);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetSimpleEnumValue(Type type, string name, bool fullName)
        => Reference(type, fullName) + "." + name;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual string GetCompositeEnumValue(Type type, Enum flags, bool fullName)
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
                    ? GetSimpleEnumValue(type, Enum.GetName(type, current)!, fullName)
                    : previous + " | " + GetSimpleEnumValue(type, Enum.GetName(type, current)!, fullName))
            ?? $"({Reference(type)}){UnknownLiteral(Convert.ChangeType(flags, Enum.GetUnderlyingType(type)))}";
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

        if (LiteralFuncs.TryGetValue(literalType.UnwrapNullableType(), out var literalFunc))
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

        if (value is ITuple tuple
            && value.GetType().FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true)
        {
            return ValueTuple(tuple);
        }

        var valueType = value.GetType();
        if (valueType is { IsGenericType: true, IsGenericTypeDefinition: false })
        {
            var genericArguments = valueType.GetGenericArguments();
            switch (value)
            {
                case IList list when genericArguments.Length == 1 && valueType.GetGenericTypeDefinition() == typeof(List<>):
                    return List(genericArguments[0], list);
                case IDictionary dict when genericArguments.Length == 2 && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>):
                    return Dictionary(genericArguments[0], genericArguments[1], dict);
            }
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
            {
                var unaryExpression = (UnaryExpression)expression;
                if (unaryExpression.Method?.Name != "op_Implicit")
                {
                    builder
                        .Append('(')
                        .Append(Reference(expression.Type, fullName: true))
                        .Append(')');
                }

                return HandleExpression(unaryExpression.Operand, builder);
            }
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
    public virtual string Fragment(IMethodCallCodeFragment fragment, string? instanceIdentifier, bool typeQualified)
    {
        var builder = new StringBuilder();

        if (typeQualified)
        {
            if (instanceIdentifier is null || fragment.DeclaringType is null || fragment.ChainedCall is not null)
            {
                throw new ArgumentException(DesignStrings.CannotGenerateTypeQualifiedMethodCall);
            }

            builder
                .Append(fragment.DeclaringType)
                .Append('.')
                .Append(fragment.Method)
                .Append('(')
                .Append(instanceIdentifier);

            foreach (var argument in fragment.Arguments)
            {
                builder.Append(", ");

                if (argument is NestedClosureCodeFragment nestedFragment)
                {
                    builder.Append(Fragment(nestedFragment, 1));
                }
                else
                {
                    builder.Append(UnknownLiteral(argument));
                }
            }

            builder.Append(')');

            return builder.ToString();
        }

        if (instanceIdentifier is not null)
        {
            builder.Append(instanceIdentifier);
        }

        builder.Append(Fragment(fragment, indent: 1));

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Fragment(IMethodCallCodeFragment? fragment, int indent = 0)
    {
        if (fragment is null)
        {
            return string.Empty;
        }

        var builder = new IndentedStringBuilder();

        if (fragment.ChainedCall is null)
        {
            AppendMethodCall(fragment);
        }
        else
        {
            for (var i = 0; i < indent; i++)
            {
                builder.IncrementIndent();
            }

            var current = fragment;
            do
            {
                builder.AppendLine();
                AppendMethodCall(current);

                current = current.ChainedCall;
            }
            while (current is not null);
        }

        return builder.ToString();

        void AppendMethodCall(IMethodCallCodeFragment current)
        {
            builder
                .Append('.')
                .Append(current.Method);

            if (current.TypeArguments.Any())
            {
                builder
                    .Append("<")
                    .Append(string.Join(", ", current.TypeArguments))
                    .Append(">");
            }

            builder
                .Append('(');

            var first = true;
            foreach (var argument in current.Arguments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(", ");
                }

                if (argument is NestedClosureCodeFragment nestedFragment)
                {
                    builder.Append(Fragment(nestedFragment, indent + 1));
                }
                else
                {
                    builder.Append(UnknownLiteral(argument));
                }
            }

            builder.Append(')');
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Fragment(NestedClosureCodeFragment fragment, int indent = 0)
    {
        if (fragment.MethodCalls.Count == 1)
        {
            return fragment.Parameter + " => " + fragment.Parameter + Fragment(fragment.MethodCalls[0], indent);
        }

        var builder = new IndentedStringBuilder();
        builder.AppendLine(fragment.Parameter + " =>");
        for (var i = 0; i < indent - 1; i++)
        {
            builder.IncrementIndent();
        }

        builder.AppendLine("{");
        using (builder.Indent())
        {
            foreach (var methodCall in fragment.MethodCalls)
            {
                builder
                    .Append(fragment.Parameter)
                    .Append(Fragment(methodCall, indent + 1));
                builder.AppendLine(";");
            }
        }

        builder.Append("}");

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Fragment(PropertyAccessorCodeFragment fragment)
        => Lambda(fragment.Properties, fragment.Parameter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Fragment(AttributeCodeFragment fragment)
    {
        var builder = new StringBuilder();

        var attributeName = fragment.Type.Name;
        if (attributeName.EndsWith("Attribute", StringComparison.Ordinal))
        {
            attributeName = attributeName[..^9];
        }

        builder
            .Append('[')
            .Append(attributeName);

        if (fragment.Arguments.Count != 0
            || fragment.NamedArguments.Count != 0)
        {
            builder.Append('(');

            var first = true;
            foreach (var value in fragment.Arguments)
            {
                if (!first)
                {
                    builder.Append(", ");
                }
                else
                {
                    first = false;
                }

                builder.Append(UnknownLiteral(value));
            }

            foreach (var item in fragment.NamedArguments)
            {
                if (!first)
                {
                    builder.Append(", ");
                }
                else
                {
                    first = false;
                }

                builder
                    .Append(item.Key)
                    .Append(" = ")
                    .Append(UnknownLiteral(item.Value));
            }

            builder.Append(')');
        }

        builder.Append(']');

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string XmlComment(string comment, int indent = 0)
    {
        var builder = new StringBuilder();

        var first = true;
        foreach (var line in comment.Split(["\r\n", "\n", "\r"], StringSplitOptions.None))
        {
            if (!first)
            {
                builder
                    .AppendLine()
                    .Append(' ', indent * 4)
                    .Append("/// ");
            }
            else
            {
                first = false;
            }

            builder.Append(SecurityElement.Escape(line));
        }

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Arguments(IEnumerable<object> values)
        => string.Join(", ", values.Select(UnknownLiteral));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<string> GetRequiredUsings(Type type)
        => type.GetNamespaces();

    private string ToSourceCode(SyntaxNode node)
    {
        var code = node.NormalizeWhitespace().ToFullString();
        var document = _project.AddDocument("Code.cs", SourceText.From(code));

        var syntaxRootFoo = document.GetSyntaxRootAsync().Result!;
        var annotatedDocument = document.WithSyntaxRoot(syntaxRootFoo.WithAdditionalAnnotations(Simplifier.Annotation));
        document = Simplifier.ReduceAsync(annotatedDocument).Result;

        var simplifiedCode = document.GetTextAsync().Result.ToString();

        return simplifiedCode;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Statement(
        Expression node,
        ISet<string> collectedNamespaces,
        IReadOnlyDictionary<object, string>? constantReplacements,
        IReadOnlyDictionary<MemberAccess, string>? memberAccessReplacements)
    {
        Dictionary<object, ExpressionSyntax>? constantReplacementExpressions = null;
        if (constantReplacements != null)
        {
            constantReplacementExpressions = [];

            foreach (var instancePair in constantReplacements)
            {
                constantReplacementExpressions[instancePair.Key] = SyntaxFactory.IdentifierName(instancePair.Value);
            }
        }

        Dictionary<MemberAccess, ExpressionSyntax>? memberAccessReplacementExpressions = null;
        if (memberAccessReplacements != null)
        {
            memberAccessReplacementExpressions = [];

            foreach (var methodPair in memberAccessReplacements)
            {
                memberAccessReplacementExpressions[methodPair.Key] = SyntaxFactory.IdentifierName(methodPair.Value);
            }
        }

        return ToSourceCode(_translator.TranslateStatement(
            node,
            constantReplacementExpressions,
            memberAccessReplacementExpressions,
            collectedNamespaces));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string Expression(
        Expression node,
        ISet<string> collectedNamespaces,
        IReadOnlyDictionary<object, string>? constantReplacements,
        IReadOnlyDictionary<MemberAccess, string>? memberAccessReplacements)
    {
        Dictionary<object, ExpressionSyntax>? constantReplacementExpressions = null;
        if (constantReplacements != null)
        {
            constantReplacementExpressions = [];

            foreach (var instancePair in constantReplacements)
            {
                constantReplacementExpressions[instancePair.Key] = SyntaxFactory.IdentifierName(instancePair.Value);
            }
        }

        Dictionary<MemberAccess, ExpressionSyntax>? memberAccessReplacementExpressions = null;
        if (memberAccessReplacements != null)
        {
            memberAccessReplacementExpressions = [];

            foreach (var methodPair in memberAccessReplacements)
            {
                memberAccessReplacementExpressions[methodPair.Key] = SyntaxFactory.IdentifierName(methodPair.Value);
            }
        }

        return ToSourceCode(_translator.TranslateExpression(
            node,
            constantReplacementExpressions,
            memberAccessReplacementExpressions,
            collectedNamespaces));
    }

    private static bool IsIdentifierStartCharacter(char ch)
    {
        if (ch < 'a')
        {
            return ch is >= 'A' and (<= 'Z' or '_');
        }

        if (ch <= 'z')
        {
            return true;
        }

        return ch > '\u007F' && IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
    }

    private static bool IsIdentifierPartCharacter(char ch)
    {
        if (ch < 'a')
        {
            return (ch < 'A'
                    ? ch is >= '0' and <= '9'
                    : ch <= 'Z')
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
