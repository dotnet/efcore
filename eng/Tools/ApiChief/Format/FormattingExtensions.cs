// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace ApiChief.Format;

internal static class FormattingExtensions
{
    private static readonly HashSet<char> _numberLiterals = ['l', 'L', 'u', 'U', 'f', 'F', 'd', 'D', 'm', 'M'];
    private static readonly HashSet<char> _secondCharInLiterals = ['l', 'L', 'u', 'U'];
    private static readonly HashSet<char> _possibleSpecialCharactersInANumber = ['.', 'x', 'X', 'b', 'B'];

    /// <summary>
    /// Ensures a single space between parameters.
    /// </summary>
    public static string WithSpaceBetweenParameters(this string signature)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < signature.Length; i++)
        {
            var current = signature[i];
            sb.Append(current);

            if (current == '"')
            {
                var index = i + 1;
                var next = signature[index];

                while (next != '"')
                {
                    sb.Append(next);
                    index++;
                    next = signature[index];
                }

                sb.Append(next);
                i = index;
            }
            else if (current == ',')
            {
                if (i + 1 < signature.Length)
                {
                    var next = signature[i + 1];

                    if (next != ' ')
                    {
                        sb.Append(' ');
                    }
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Deletes literal letters from the number.
    /// </summary>
    public static string WithNumbersWithoutLiterals(this string memberDecl)
    {
        var sb = new StringBuilder();

        var inMethod = false;
        var sawEqualitySign = false;

        for (var i = 0; i < memberDecl.Length; i++)
        {
            var current = memberDecl[i];

            if (!inMethod)
            {
                if (current == '(')
                {
                    inMethod = true;
                }

                sb.Append(current);

                continue;
            }

            sb.Append(current);

            if (current == ')')
            {
                inMethod = false;
            }
            else if (current == '"')
            {
                var initial = i + 1;
                var next = memberDecl[initial];

                while (next != '"')
                {
                    sb.Append(next);
                    initial++;
                    next = memberDecl[initial];
                }

                sb.Append(next);
                i = initial;
            }
            else if (current == '=')
            {
                sawEqualitySign = true;
            }
            else if (char.IsDigit(current) && sawEqualitySign)
            {
                var initial = i + 1;
                var next = memberDecl[initial];
                while (char.IsDigit(next) || (char.IsDigit(memberDecl[initial - 1]) && _possibleSpecialCharactersInANumber.Contains(next)))
                {
                    sb.Append(next);
                    initial++;
                    next = memberDecl[initial];
                }

                if (!_numberLiterals.Contains(next))
                {
                    sb.Append(next);
                }
                else if (_secondCharInLiterals.Contains(memberDecl[initial + 1]))
                {
                    initial++;
                }

                i = initial;
                sawEqualitySign = false;
            }
        }

        return sb.ToString();
    }

    public static string WithSimpleTypeNames(this string memberDecl)
    {
        var openParen = memberDecl.IndexOf('(');
        if (openParen >= 0)
        {
            var closeParen = memberDecl.LastIndexOf(')');
            if (closeParen > openParen)
            {
                var prefix = SimplifyDeclarationPrefix(memberDecl[..openParen]);
                var parameters = SimplifyParameterList(memberDecl.Substring(openParen + 1, closeParen - openParen - 1));
                return prefix + "(" + parameters + memberDecl[closeParen..];
            }
        }

        var bodyIndex = memberDecl.IndexOf(" {", StringComparison.Ordinal);
        if (bodyIndex < 0)
        {
            bodyIndex = memberDecl.IndexOf(';');
        }

        return bodyIndex > 0
            ? SimplifyDeclarationPrefix(memberDecl[..bodyIndex]) + memberDecl[bodyIndex..]
            : memberDecl;
    }

    private static string SimplifyDeclarationPrefix(string declarationPrefix)
    {
        var tokens = SplitByTopLevelDelimiter(declarationPrefix, ' ');
        if (tokens.Count == 0)
        {
            return declarationPrefix;
        }

        for (var i = 0; i < tokens.Count - 1; i++)
        {
            if (!IsModifierToken(tokens[i]))
            {
                tokens[i] = SimplifyTypeExpression(tokens[i]);
            }
        }

        tokens[^1] = SimplifyMemberIdentifier(tokens[^1]);

        return string.Join(' ', tokens);
    }

    private static string SimplifyParameterList(string parameterList)
    {
        var parameters = SplitByTopLevelDelimiter(parameterList, ',');
        if (parameters.Count == 0)
        {
            return parameterList;
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i].Trim();
            if (parameter.Length == 0)
            {
                continue;
            }

            var defaultValueIndex = IndexOfTopLevel(parameter, '=');
            var parameterWithoutDefault = defaultValueIndex >= 0 ? parameter[..defaultValueIndex].TrimEnd() : parameter;
            var defaultValue = defaultValueIndex >= 0 ? parameter[defaultValueIndex..] : string.Empty;

            var tokens = SplitByTopLevelDelimiter(parameterWithoutDefault, ' ');
            for (var j = 0; j < tokens.Count - 1; j++)
            {
                if (!IsModifierToken(tokens[j]))
                {
                    tokens[j] = SimplifyTypeExpression(tokens[j]);
                }
            }

            parameters[i] = string.Join(' ', tokens) + (defaultValue.Length == 0 ? string.Empty : " " + defaultValue.TrimStart());
        }

        return string.Join(", ", parameters);
    }

    private static string SimplifyTypeExpression(string typeExpression)
    {
        var sb = new StringBuilder(typeExpression.Length);

        for (var i = 0; i < typeExpression.Length; i++)
        {
            var current = typeExpression[i];
            if (!IsIdentifierCharacter(current))
            {
                sb.Append(current);
                continue;
            }

            var start = i;
            while (i + 1 < typeExpression.Length && IsIdentifierCharacter(typeExpression[i + 1]))
            {
                i++;
            }

            var identifier = typeExpression[start..(i + 1)];
            var lastDot = identifier.LastIndexOf('.');
            sb.Append(lastDot >= 0 ? identifier[(lastDot + 1)..] : identifier);
        }

        return sb.ToString();
    }

    private static string SimplifyMemberIdentifier(string memberIdentifier)
    {
        var segments = SplitQualifiedSegments(memberIdentifier);
        return segments.Count <= 2
            ? string.Join('.', segments)
            : string.Join('.', segments.GetRange(segments.Count - 2, 2));
    }

    private static List<string> SplitQualifiedSegments(string value)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var depth = 0;

        foreach (var c in value)
        {
            switch (c)
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case '.':
                    if (depth == 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                        continue;
                    }
                    break;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    private static List<string> SplitByTopLevelDelimiter(string value, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var depth = 0;

        foreach (var c in value)
        {
            switch (c)
            {
                case '<':
                case '(':
                case '[':
                case '{':
                    depth++;
                    break;
                case '>':
                case ')':
                case ']':
                case '}':
                    depth--;
                    break;
            }

            if (c == delimiter && depth == 0)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    private static int IndexOfTopLevel(string value, char delimiter)
    {
        var depth = 0;

        for (var i = 0; i < value.Length; i++)
        {
            switch (value[i])
            {
                case '<':
                case '(':
                case '[':
                case '{':
                    depth++;
                    break;
                case '>':
                case ')':
                case ']':
                case '}':
                    depth--;
                    break;
                default:
                    if (value[i] == delimiter && depth == 0)
                    {
                        return i;
                    }
                    break;
            }
        }

        return -1;
    }

    private static bool IsIdentifierCharacter(char c)
        => char.IsLetterOrDigit(c) || c is '_' or '.' or '`' or '@';

    private static bool IsModifierToken(string token)
        => token is "this" or "ref" or "readonly" or "in" or "out" or "params" or "scoped"
            or "static" or "virtual" or "override" or "abstract" or "sealed" or "async"
            or "extern" or "unsafe" or "new" or "partial" or "const" or "volatile" or "event"
            or "operator" or "implicit" or "explicit";
}
