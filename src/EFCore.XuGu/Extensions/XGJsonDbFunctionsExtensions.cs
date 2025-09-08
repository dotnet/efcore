// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides methods for supporting translation to MySQL JSON operators and functions.
    /// </summary>
    public static class XGJsonDbFunctionsExtensions
    {
        /// <summary>
        /// Explicitly converts <paramref name="value"/> to JSON.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="value">
        /// The string to convert to JSON.
        /// </param>
        /// <returns> The JSON value. </returns>
        /// <remarks>
        /// Translates <paramref name="value"/> to `CAST(value as json)` where appropriate on server implementations
        /// that support the `json` store type.
        /// </remarks>
        public static XGJsonString AsJson([CanBeNull] this DbFunctions _, [NotNull] string value)
            => value;

        /// <summary>
        /// Returns the type of the outermost JSON value as a text string.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <returns> The JSON type as a text string. </returns>
        /// <remarks> For possible return values see: https://dev.mysql.com/doc/refman/8.0/en/json-attribute-functions.html#function_json-type </remarks>
        public static string JsonType([CanBeNull] this DbFunctions _, [NotNull] object json)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonType)));

        /// <summary>
        /// Quotes a string as a JSON value by wrapping it with double quote characters and escaping interior quote and
        /// other characters, then returning the result as a `utf8mb4` string. Returns `null` if the argument is `null`.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="value">
        /// The string value.
        /// </param>
        public static string JsonQuote(
            [CanBeNull] this DbFunctions _,
            [NotNull] string value)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonQuote)));

        /// <summary>
        /// Unquotes JSON value and returns the result as a `utf8mb4` string. Returns `null` if the argument is `null`.
        /// An error occurs if the value starts and ends with double quotes but is not a valid JSON string literal.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        public static string JsonUnquote(
            [CanBeNull] this DbFunctions _,
            [NotNull] object json)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonUnquote)));

        /// <summary>
        /// Returns data from a JSON document, selected from the parts of the document matched by the path arguments.
        /// Returns `null` if any argument is `null` or no paths locate a value in the document. An error occurs if the
        /// `json` argument is not a valid JSON document or any path argument is not a valid path expression.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="paths">
        /// A set of paths to extract from <paramref name="json"/>.
        /// </param>
        public static T JsonExtract<T>(
            [CanBeNull] this DbFunctions _,
            [NotNull] object json,
            [NotNull] params string[] paths)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonExtract)));

        /// <summary>
        /// Checks if <paramref name="json1"/> overlaps <paramref name="json2"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json1">
        /// A JSON column or value. Can be a JSON DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="json2">
        /// A JSON column or value. Can be a JSON DOM object, a string, or a user POCO mapped to JSON.
        /// </param>
        public static bool JsonOverlaps(
            [CanBeNull] this DbFunctions _, [NotNull] object json1, [NotNull] object json2)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonOverlaps)));

        /// <summary>
        /// Checks if <paramref name="json"/> contains <paramref name="candidate"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a JSON DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="candidate">
        /// A JSON column or value. Can be a JSON DOM object, a string, or a user POCO mapped to JSON.
        /// </param>
        public static bool JsonContains(
            [CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] object candidate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContains)));

        /// <summary>
        /// Checks if <paramref name="json"/> contains <paramref name="candidate"/> at a specific <paramref name="path"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="candidate">
        /// A JSON column or value. Can be a JSON DOM object, a string, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="path">
        /// A string containing a valid JSON path (staring with `$`).
        /// </param>
        public static bool JsonContains(
            [CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] object candidate, [CanBeNull] string path)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContains)));

        /// <summary>
        /// Checks if <paramref name="path"/> exists within <paramref name="json"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="path">A path to be checked inside <paramref name="json"/>.</param>
        public static bool JsonContainsPath([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] string path)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContainsPath)));

        /// <summary>
        /// Checks if any of the given <paramref name="paths"/> exist within <paramref name="json"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="paths">A set of paths to be checked inside <paramref name="json"/>.</param>
        public static bool JsonContainsPathAny([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] params string[] paths)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContainsPathAny)));

        /// <summary>
        /// Checks if all of the given <paramref name="paths"/> exist within <paramref name="json"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="paths">A set of paths to be checked inside <paramref name="json"/>.</param>
        public static bool JsonContainsPathAll([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] params string[] paths)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonContainsPathAll)));

        /// <summary>
        /// Checks if <paramref name="json"/> contains <paramref name="searchString"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a JSON DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="searchString">
        /// The string to search for.
        /// </param>
        public static bool JsonSearchAny([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] string searchString)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonSearchAny)));

        /// <summary>
        /// Checks if <paramref name="json"/> contains <paramref name="searchString"/> under <paramref name="path"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a JSON DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="searchString">
        /// The string to search for.
        /// </param>
        /// <param name="path">
        /// A string containing a valid JSON path (staring with `$`).
        /// </param>
        public static bool JsonSearchAny([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] string searchString, string path)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonSearchAny)));

        /// <summary>
        /// Checks if <paramref name="json"/> contains <paramref name="searchString"/> under <paramref name="path"/>.
        /// </summary>
        /// <param name="_">DbFunctions instance</param>
        /// <param name="json">
        /// A JSON column or value. Can be a JSON DOM object, a string property mapped to JSON, or a user POCO mapped to JSON.
        /// </param>
        /// <param name="searchString">
        /// The string to search for.
        /// </param>
        /// <param name="path">
        /// A string containing a valid JSON path (staring with `$`).
        /// </param>
        /// <param name="escapeChar">
        /// Can be `null`, an empty string or a one character wide string used for escaping characters in <paramref name="searchString"/>.
        /// </param>
        public static bool JsonSearchAny([CanBeNull] this DbFunctions _, [NotNull] object json, [NotNull] string searchString, string path, string escapeChar)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonSearchAny)));
    }
}
