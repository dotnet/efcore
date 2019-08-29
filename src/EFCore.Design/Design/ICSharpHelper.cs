// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Helper for generating C# code.
    /// </summary>
    public interface ICSharpHelper
    {
        /// <summary>
        ///     Generates a method call code fragment.
        /// </summary>
        /// <param name="fragment"> The method call. </param>
        /// <returns> The fragment. </returns>
        string Fragment([NotNull] MethodCallCodeFragment fragment);

        /// <summary>
        ///     Generates a valid C# identifier from the specified string unique to the scope.
        /// </summary>
        /// <param name="name"> The base identifier name. </param>
        /// <param name="scope"> A list of in-scope identifiers. </param>
        /// <returns> The identifier. </returns>
        string Identifier([NotNull] string name, [CanBeNull] ICollection<string> scope = null);

        /// <summary>
        ///     Generates a property accessor lambda.
        /// </summary>
        /// <param name="properties"> The property names. </param>
        /// <returns> The lambda. </returns>
        string Lambda([NotNull] IReadOnlyList<string> properties);

        /// <summary>
        ///     Generates a multidimensional array literal.
        /// </summary>
        /// <param name="values"> The multidimensional array. </param>
        /// <returns> The literal. </returns>
        string Literal([NotNull] object[,] values);

        /// <summary>
        ///     Generates a nullable literal.
        /// </summary>
        /// <typeparam name="T"> The underlying type of the nullable type. </typeparam>
        /// <param name="value"> The nullable value. </param>
        /// <returns> The literal. </returns>
        string Literal<T>(T? value)
            where T : struct;

        /// <summary>
        ///     Generates a bool literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(bool value);

        /// <summary>
        ///     Generates a byte literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(byte value);

        /// <summary>
        ///     Generates a char literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(char value);

        /// <summary>
        ///     Generates a DateTime literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(DateTime value);

        /// <summary>
        ///     Generates a DateTimeOffset literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(DateTimeOffset value);

        /// <summary>
        ///     Generates a decimal literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(decimal value);

        /// <summary>
        ///     Generates a double literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(double value);

        /// <summary>
        ///     Generates an enum literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal([NotNull] Enum value);

        /// <summary>
        ///     Generates a float literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(float value);

        /// <summary>
        ///     Generates a Guid literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(Guid value);

        /// <summary>
        ///     Generates an int literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(int value);

        /// <summary>
        ///     Generates a long literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(long value);

        /// <summary>
        ///     Generates a sbyte literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(sbyte value);

        /// <summary>
        ///     Generates a short literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(short value);

        /// <summary>
        ///     Generates a string literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal([NotNull] string value);

        /// <summary>
        ///     Generates a TimeSpan literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(TimeSpan value);

        /// <summary>
        ///     Generates a uint literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(uint value);

        /// <summary>
        ///     Generates a ulong literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(ulong value);

        /// <summary>
        ///     Generates a ushort literal.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string Literal(ushort value);

        /// <summary>
        ///     Generates an object array literal.
        /// </summary>
        /// <param name="values"> The object array. </param>
        /// <param name="vertical"> A value indicating whether to layout the literal vertically. </param>
        /// <returns> The literal. </returns>
        string Literal<T>([NotNull] T[] values, bool vertical = false);

        /// <summary>
        ///     Generates a valid C# namespace from the specified parts.
        /// </summary>
        /// <param name="name"> The base parts of the namespace. </param>
        /// <returns> The namespace. </returns>
        string Namespace([NotNull] params string[] name);

        /// <summary>
        ///     Generates a C# type reference.
        /// </summary>
        /// <param name="type"> The type to reference. </param>
        /// <returns> The reference. </returns>
        string Reference([NotNull] Type type);

        /// <summary>
        ///     Generates a literal for a type not known at compile time.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <returns> The literal. </returns>
        string UnknownLiteral([CanBeNull] object value);
    }
}
