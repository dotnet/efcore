// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Helper for generating C# code.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface ICSharpHelper
{
    /// <summary>
    ///     Generates a method call code fragment.
    /// </summary>
    /// <param name="fragment">The method call.</param>
    /// <param name="instanceIdentifier">An identifier on which the method call will be generated.</param>
    /// <param name="typeQualified">
    ///     <see langword="true" /> if the method call should be type-qualified, <see langword="false" /> for instance/extension syntax.
    /// </param>
    /// <returns>The fragment.</returns>
    string Fragment(IMethodCallCodeFragment fragment, string? instanceIdentifier, bool typeQualified);

    /// <summary>
    ///     Generates a method call code fragment.
    /// </summary>
    /// <param name="fragment">The method call. If null, no code is generated.</param>
    /// <param name="indent">The indentation level to use when multiple lines are generated.</param>
    /// <returns>The fragment.</returns>
    string Fragment(IMethodCallCodeFragment? fragment, int indent = 0);

    /// <summary>
    ///     Generates a lambda code fragment.
    /// </summary>
    /// <param name="fragment">The lambda.</param>
    /// <param name="indent">The indentation level to use when multiple lines are generated.</param>
    /// <returns>The fragment.</returns>
    string Fragment(NestedClosureCodeFragment fragment, int indent = 0);

    /// <summary>
    ///     Generates a property accessor lambda code fragment.
    /// </summary>
    /// <param name="fragment">The property accessor lambda.</param>
    /// <returns>A code representation of the lambda.</returns>
    string Fragment(PropertyAccessorCodeFragment fragment);

    /// <summary>
    ///     Generates a valid C# identifier from the specified string unique to the scope.
    /// </summary>
    /// <param name="name">The base identifier name.</param>
    /// <param name="scope">A list of in-scope identifiers.</param>
    /// <param name="capitalize">
    ///     <see langword="true" /> if the first letter should be converted to uppercase;
    ///     <see langword="false" /> if the first letter should be converted to lowercase;
    /// </param>
    /// <returns>The identifier.</returns>
    string Identifier(string name, ICollection<string>? scope = null, bool? capitalize = null);

    /// <summary>
    ///     Generates a property accessor lambda.
    /// </summary>
    /// <param name="properties">The property names.</param>
    /// <param name="lambdaIdentifier">The identifier to use for parameter in the lambda.</param>
    /// <returns>The lambda.</returns>
    string Lambda(IReadOnlyList<string> properties, string? lambdaIdentifier = null);

    /// <summary>
    ///     Generates a property accessor lambda.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="lambdaIdentifier">The identifier to use for parameter in the lambda.</param>
    /// <returns>The lambda.</returns>
    string Lambda(IEnumerable<IProperty> properties, string? lambdaIdentifier = null)
        => Lambda(properties.Select(p => p.Name).ToList(), lambdaIdentifier);

    /// <summary>
    ///     Generates a multidimensional array literal.
    /// </summary>
    /// <param name="values">The multidimensional array.</param>
    /// <returns>The literal.</returns>
    string Literal(object?[,] values);

    /// <summary>
    ///     Generates a nullable literal.
    /// </summary>
    /// <typeparam name="T">The underlying type of the nullable type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>The literal.</returns>
    string Literal<T>(T? value)
        where T : struct;

    /// <summary>
    ///     Generates a BigInteger literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(BigInteger value);

    /// <summary>
    ///     Generates a bool literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(bool value);

    /// <summary>
    ///     Generates a byte literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(byte value);

    /// <summary>
    ///     Generates a char literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(char value);

    /// <summary>
    ///     Generates a DateOnly literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(DateOnly value);

    /// <summary>
    ///     Generates a DateTime literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(DateTime value);

    /// <summary>
    ///     Generates a DateTimeOffset literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(DateTimeOffset value);

    /// <summary>
    ///     Generates a decimal literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(decimal value);

    /// <summary>
    ///     Generates a double literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(double value);

    /// <summary>
    ///     Generates an enum literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="fullName">Whether the type should be namespace-qualified.</param>
    /// <returns>The literal.</returns>
    string Literal(Enum value, bool fullName = false);

    /// <summary>
    ///     Generates a float literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(float value);

    /// <summary>
    ///     Generates a Guid literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(Guid value);

    /// <summary>
    ///     Generates an int literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(int value);

    /// <summary>
    ///     Generates a long literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(long value);

    /// <summary>
    ///     Generates a sbyte literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(sbyte value);

    /// <summary>
    ///     Generates a short literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(short value);

    /// <summary>
    ///     Generates a string literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(string? value);

    /// <summary>
    ///     Generates a TimeOnly literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(TimeOnly value);

    /// <summary>
    ///     Generates a TimeSpan literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(TimeSpan value);

    /// <summary>
    ///     Generates a uint literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(uint value);

    /// <summary>
    ///     Generates a ulong literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(ulong value);

    /// <summary>
    ///     Generates a ushort literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string Literal(ushort value);

    /// <summary>
    ///     Generates a <see cref="Type" /> literal.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="fullName">Whether the type should be namespace-qualified.</param>
    /// <returns>The literal.</returns>
    string Literal(Type value, bool? fullName = null);

    /// <summary>
    ///     Generates an object array literal.
    /// </summary>
    /// <param name="values">The object array.</param>
    /// <param name="vertical">A value indicating whether to layout the literal vertically.</param>
    /// <returns>The literal.</returns>
    string Literal<T>(T[] values, bool vertical = false);

    /// <summary>
    ///     Generates a list literal.
    /// </summary>
    /// <param name="values">The list.</param>
    /// <param name="vertical">A value indicating whether to layout the literal vertically.</param>
    /// <returns>The literal.</returns>
    string Literal<T>(List<T> values, bool vertical = false);

    /// <summary>
    ///     Generates a dictionary literal.
    /// </summary>
    /// <param name="values">The dictionary.</param>
    /// <param name="vertical">A value indicating whether to layout the literal vertically.</param>
    /// <returns>The literal.</returns>
    string Literal<TKey, TValue>(Dictionary<TKey, TValue> values, bool vertical = false)
        where TKey : notnull;

    /// <summary>
    ///     Generates a valid C# namespace from the specified parts.
    /// </summary>
    /// <param name="name">The base parts of the namespace.</param>
    /// <returns>The namespace.</returns>
    string Namespace(params string[] name);

    /// <summary>
    ///     Generates a C# type reference.
    /// </summary>
    /// <param name="type">The type to reference.</param>
    /// <param name="fullName">Whether the type should be namespace-qualified.</param>
    /// <returns>The reference.</returns>
    string Reference(Type type, bool? fullName = null);

    /// <summary>
    ///     Generates a literal for a type not known at compile time.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The literal.</returns>
    string UnknownLiteral(object? value);

    /// <summary>
    ///     Generates an XML documentation comment. Handles escaping and newlines.
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <param name="indent">The indentation level to use when multiple lines are generated.</param>
    /// <returns>The comment.</returns>
    string XmlComment(string comment, int indent = 0);

    /// <summary>
    ///     Generates an attribute specification.
    /// </summary>
    /// <param name="fragment">The attribute code fragment.</param>
    /// <returns>The attribute specification code.</returns>
    string Fragment(AttributeCodeFragment fragment);

    /// <summary>
    ///     Generates a comma-separated argument list of values.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The argument list.</returns>
    string Arguments(IEnumerable<object> values);

    /// <summary>
    ///     Gets the using statements required when referencing a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The usings.</returns>
    IEnumerable<string> GetRequiredUsings(Type type);

    /// <summary>
    ///     Translates a node representing a statement into source code that would produce it.
    /// </summary>
    /// <param name="node">The node to be translated.</param>
    /// <param name="collectedNamespaces">Any namespaces required by the translated code will be added to this set.</param>
    /// <param name="constantReplacements">Collection of translations for statically known instances.</param>
    /// <param name="memberAccessReplacements">Collection of translations for non-public member accesses.</param>
    /// <returns>Source code that would produce <paramref name="node" />.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    string Statement(Expression node,
        ISet<string> collectedNamespaces,
        IReadOnlyDictionary<object, string>? constantReplacements = null,
        IReadOnlyDictionary<MemberAccess, string>? memberAccessReplacements = null);

    /// <summary>
    ///     Translates a node representing an expression into source code that would produce it.
    /// </summary>
    /// <param name="node">The node to be translated.</param>
    /// <param name="collectedNamespaces">Any namespaces required by the translated code will be added to this set.</param>
    /// <param name="constantReplacements">Collection of translations for statically known instances.</param>
    /// <param name="memberAccessReplacements">Collection of translations for non-public member accesses.</param>
    /// <returns>Source code that would produce  <paramref name="node" />.</returns>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    string Expression(Expression node,
        ISet<string> collectedNamespaces,
        IReadOnlyDictionary<object, string>? constantReplacements = null,
        IReadOnlyDictionary<MemberAccess, string>? memberAccessReplacements = null);
}
