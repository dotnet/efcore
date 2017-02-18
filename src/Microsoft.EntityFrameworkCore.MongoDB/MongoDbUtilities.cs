using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class MongoDbUtilities
    {
        private static readonly Regex _leadingUppercaseRegex
            = new Regex(pattern: "^(([A-Z](?![a-z]))+|([A-Z](?=[a-z])))", options: RegexOptions.Compiled);
        private static readonly Regex _singularRegex
            = new Regex(pattern: "(ey|.)(?<!s)$", options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ToCamelCase([NotNull] string value)
            => _leadingUppercaseRegex.Replace(
                Check.NotNull(value, nameof(value)), match => match.Value.ToLower());

        public static string Pluralize([NotNull] string value)
            => _singularRegex.Replace(
                Check.NotNull(value, nameof(value)),
                match => string.Equals(a: "y", b: match.Value, comparisonType: StringComparison.OrdinalIgnoreCase)
                    ? "ies"
                    : $"{match.Value}s");
    }
}