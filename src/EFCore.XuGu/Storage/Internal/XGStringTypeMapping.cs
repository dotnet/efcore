// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Utilities;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGStringTypeMapping : XGTypeMapping, IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator
    {
        public static XGStringTypeMapping Default { get; } = new("varchar", StoreTypePostfix.Size);

        private const int UnicodeMax = 4000;
        private const int AnsiMax = 8000;

        private readonly int _maxSpecificSize;

        public virtual bool NoBackslashEscapes { get; }
        public virtual bool ReplaceLineBreaksWithCharFunction { get; }
        public virtual bool IsUnquoted { get; }
        public virtual bool ForceToString { get; }

        public virtual bool IsNationalChar
            => StoreTypeNameBase.StartsWith("n", StringComparison.OrdinalIgnoreCase) &&
               StoreTypeNameBase.Contains("char", StringComparison.OrdinalIgnoreCase);

        public XGStringTypeMapping(
            [NotNull] string storeType,
            StoreTypePostfix storeTypePostfix,
            bool unicode = true,
            int? size = null,
            bool fixedLength = false,
            bool noBackslashEscapes = false,
            bool replaceLineBreaksWithCharFunction = true,
            bool unquoted = false,
            bool forceToString = false)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(string),
                        jsonValueReaderWriter: JsonStringReaderWriter.Instance),
                    storeType,
                    storeTypePostfix,
                    unicode
                        ? fixedLength
                            ? System.Data.DbType.StringFixedLength
                            : System.Data.DbType.String
                        : fixedLength
                            ? System.Data.DbType.AnsiStringFixedLength
                            : System.Data.DbType.AnsiString,
                    unicode,
                    size,
                    fixedLength),
                fixedLength
                    ? XGDbType.Char
                    : XGDbType.VarChar,
                noBackslashEscapes,
                replaceLineBreaksWithCharFunction,
                unquoted,
                forceToString)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected XGStringTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType xgDbType,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction,
            bool isUnquoted,
            bool forceToString)
            : base(parameters, xgDbType)
        {
            _maxSpecificSize = CalculateSize(parameters.Unicode, parameters.Size);
            NoBackslashEscapes = noBackslashEscapes;
            ReplaceLineBreaksWithCharFunction = replaceLineBreaksWithCharFunction;
            ForceToString = forceToString;
            IsUnquoted = isUnquoted;
        }

        private static int CalculateSize(bool unicode, int? size)
            => unicode
                ? size.HasValue && size <= UnicodeMax ? size.Value : UnicodeMax
                : size.HasValue && size <= AnsiMax ? size.Value : AnsiMax;

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGStringTypeMapping(parameters, XGDbType, NoBackslashEscapes, ReplaceLineBreaksWithCharFunction, IsUnquoted, ForceToString);

        public virtual RelationalTypeMapping Clone(
            bool? unquoted = null,
            bool? forceToString = null,
            bool? noBackslashEscapes = null,
            bool? replaceLineBreaksWithCharFunction = null)
            => new XGStringTypeMapping(
                Parameters,
                XGDbType,
                noBackslashEscapes ?? NoBackslashEscapes,
                replaceLineBreaksWithCharFunction ?? ReplaceLineBreaksWithCharFunction,
                unquoted ?? IsUnquoted,
                forceToString ?? ForceToString);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to the size facet if specified, or
            // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise always set to
            // -1 (unbounded) to avoid size inference.

            var value = parameter.Value;
            if (ForceToString && value != null && value != DBNull.Value)
            {
                value = value.ToString();
            }

            int? length;
            if (value is string stringValue)
            {
                length = stringValue.Length;
            }
            else if (value is byte[] byteArray)
            {
                length = byteArray.Length;
            }
            else
            {
                length = null;
            }

            parameter.Size = value == null || value == DBNull.Value || length != null && length <= _maxSpecificSize
                ? _maxSpecificSize
                : -1;

            if (parameter.Value != value)
            {
                parameter.Value = value;
            }
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var stringValue = ForceToString
                ? value.ToString()
                : (string)value;

            return IsUnquoted
                ? EscapeSqlLiteral(stringValue, !NoBackslashEscapes)
                : EscapeSqlLiteralWithLineBreaks(stringValue, !NoBackslashEscapes, ReplaceLineBreaksWithCharFunction);
        }

        public static string EscapeSqlLiteralWithLineBreaks(string value, bool escapeBackslashes, bool replaceLineBreaksWithCharFunction)
        {
            var escapedLiteral = $"'{EscapeSqlLiteral(value, escapeBackslashes)}'";

            // BUG: EF Core indents idempotent scripts, which can lead to unexpected values for strings
            //      that contain line breaks.
            //      Tracked by: https://github.com/aspnet/EntityFrameworkCore/issues/15256
            //
            //      Convert line break characters to their CHAR() representation as a workaround.

            if (replaceLineBreaksWithCharFunction
                && (value.Contains("\r") || value.Contains("\n")))
            {
                escapedLiteral = "CONCAT(" + escapedLiteral
                    .Replace("\r\n", "', CHR(13) || CHR(10), '")
                    .Replace("\r", "', CHR(13), '")
                    .Replace("\n", "', CHR(10), '") + ")";
            }

            return escapedLiteral;
        }

        public static string EscapeSqlLiteral(string literal, bool escapeBackslashes)
            => EscapeBackslashes(Check.NotNull(literal, nameof(literal)).Replace("'", "''"), escapeBackslashes);

        public static string EscapeBackslashes(string literal, bool escapeBackslashes)
        {
            return escapeBackslashes
                ? literal.Replace(@"\", @"\\")
                : literal;
        }

        void IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator.Create(
            CSharpRuntimeAnnotationCodeGeneratorParameters codeGeneratorParameters,
            CSharpRuntimeAnnotationCodeGeneratorDependencies codeGeneratorDependencies)
        {
            var defaultTypeMapping = Default;
            if (defaultTypeMapping == this)
            {
                return;
            }

            var code = codeGeneratorDependencies.CSharpHelper;

            var cloneParameters = new List<string>();

            if (IsUnquoted != defaultTypeMapping.IsUnquoted)
            {
                cloneParameters.Add($"unquoted: {code.Literal(IsUnquoted)}");
            }

            if (ForceToString != defaultTypeMapping.ForceToString)
            {
                cloneParameters.Add($"forceToString: {code.Literal(ForceToString)}");
            }

            if (NoBackslashEscapes != defaultTypeMapping.NoBackslashEscapes)
            {
                cloneParameters.Add($"noBackslashEscapes: {code.Literal(NoBackslashEscapes)}");
            }

            if (ReplaceLineBreaksWithCharFunction != defaultTypeMapping.ReplaceLineBreaksWithCharFunction)
            {
                cloneParameters.Add($"replaceLineBreaksWithCharFunction: {code.Literal(ReplaceLineBreaksWithCharFunction)}");
            }

            if (cloneParameters.Any())
            {
                var mainBuilder = codeGeneratorParameters.MainBuilder;

                mainBuilder.AppendLine(";");

                mainBuilder
                    .AppendLine($"{codeGeneratorParameters.TargetName}.TypeMapping = (({code.Reference(GetType())}){codeGeneratorParameters.TargetName}.TypeMapping).Clone(")
                    .IncrementIndent();

                for (var i = 0; i < cloneParameters.Count; i++)
                {
                    if (i > 0)
                    {
                        mainBuilder.AppendLine(",");
                    }

                    mainBuilder.Append(cloneParameters[i]);
                }

                mainBuilder
                    .Append(")")
                    .DecrementIndent();
            }
        }
    }
}
