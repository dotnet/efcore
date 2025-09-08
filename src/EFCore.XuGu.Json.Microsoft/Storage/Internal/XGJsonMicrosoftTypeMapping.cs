// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.Internal
{
    public class XGJsonMicrosoftTypeMapping<T> : XGJsonTypeMapping<T>
    {
        public static new XGJsonMicrosoftTypeMapping<T> Default { get; } = new("json", null, null, false, true);

        // Called via reflection.
        // ReSharper disable once UnusedMember.Global
        public XGJsonMicrosoftTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter valueConverter,
            [CanBeNull] ValueComparer valueComparer,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(
                storeType,
                valueConverter,
                valueComparer,
                noBackslashEscapes,
                replaceLineBreaksWithCharFunction)
        {
        }

        protected XGJsonMicrosoftTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType xgDbType,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(
                parameters,
                xgDbType,
                noBackslashEscapes,
                replaceLineBreaksWithCharFunction)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonMicrosoftTypeMapping<T>(parameters, XGDbType, NoBackslashEscapes, ReplaceLineBreaksWithCharFunction);

        protected override RelationalTypeMapping Clone(bool? noBackslashEscapes = null, bool? replaceLineBreaksWithCharFunction = null)
            => new XGJsonMicrosoftTypeMapping<T>(
                Parameters,
                XGDbType,
                noBackslashEscapes ?? NoBackslashEscapes,
                replaceLineBreaksWithCharFunction ?? ReplaceLineBreaksWithCharFunction);

        public override Expression GenerateCodeLiteral(object value)
            => value switch
            {
                JsonDocument document => Expression.Call(
                    typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), new[] {typeof(string), typeof(JsonDocumentOptions)}),
                    Expression.Constant(document.RootElement.ToString()),
                    Expression.New(typeof(JsonDocumentOptions))),
                JsonElement element => Expression.Property(
                    Expression.Call(
                        typeof(JsonDocument).GetMethod(nameof(JsonDocument.Parse), new[] {typeof(string), typeof(JsonDocumentOptions)}),
                        Expression.Constant(element.ToString()),
                        Expression.New(typeof(JsonDocumentOptions))),
                    nameof(JsonDocument.RootElement)),
                string s => Expression.Constant(s),
                _ => throw new NotSupportedException("Cannot generate code literals for JSON POCOs.")
            };
    }
}
