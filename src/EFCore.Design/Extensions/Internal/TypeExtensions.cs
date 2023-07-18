// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

internal static class TypeExtensions
{
    internal static TypeSyntax GetTypeSyntax(this Type type)
    {
        // TODO: Qualification...
        if (type.IsGenericType)
        {
            return GenericName(
                Identifier(type.Name.Substring(0, type.Name.IndexOf('`'))),
                TypeArgumentList(SeparatedList(type.GenericTypeArguments.Select(GetTypeSyntax))));
        }

        if (type == typeof(string))
        {
            return PredefinedType(Token(SyntaxKind.StringKeyword));
        }

        if (type == typeof(bool))
        {
            return PredefinedType(Token(SyntaxKind.BoolKeyword));
        }

        if (type == typeof(byte))
        {
            return PredefinedType(Token(SyntaxKind.ByteKeyword));
        }

        if (type == typeof(sbyte))
        {
            return PredefinedType(Token(SyntaxKind.SByteKeyword));
        }

        if (type == typeof(int))
        {
            return PredefinedType(Token(SyntaxKind.IntKeyword));
        }

        if (type == typeof(uint))
        {
            return PredefinedType(Token(SyntaxKind.UIntKeyword));
        }

        if (type == typeof(short))
        {
            return PredefinedType(Token(SyntaxKind.ShortKeyword));
        }

        if (type == typeof(ushort))
        {
            return PredefinedType(Token(SyntaxKind.UShortKeyword));
        }

        if (type == typeof(long))
        {
            return PredefinedType(Token(SyntaxKind.LongKeyword));
        }

        if (type == typeof(ulong))
        {
            return PredefinedType(Token(SyntaxKind.ULongKeyword));
        }

        if (type == typeof(float))
        {
            return PredefinedType(Token(SyntaxKind.FloatKeyword));
        }

        if (type == typeof(double))
        {
            return PredefinedType(Token(SyntaxKind.DoubleKeyword));
        }

        if (type == typeof(decimal))
        {
            return PredefinedType(Token(SyntaxKind.DecimalKeyword));
        }

        if (type == typeof(char))
        {
            return PredefinedType(Token(SyntaxKind.CharKeyword));
        }

        if (type == typeof(object))
        {
            return PredefinedType(Token(SyntaxKind.ObjectKeyword));
        }

        if (type == typeof(void))
        {
            return PredefinedType(Token(SyntaxKind.VoidKeyword));
        }

        return IdentifierName(type.Name);
    }
}
