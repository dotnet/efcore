// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.Internal
{
    public class XGJsonNewtonsoftTypeMapping<T> : XGJsonTypeMapping<T>
    {
        public static new XGJsonNewtonsoftTypeMapping<T> Default { get; } = new("json", null, null, false, true);

        // Called via reflection.
        // ReSharper disable once UnusedMember.Global
        public XGJsonNewtonsoftTypeMapping(
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

        protected XGJsonNewtonsoftTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType xgDbType,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(parameters, xgDbType, noBackslashEscapes, replaceLineBreaksWithCharFunction)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonNewtonsoftTypeMapping<T>(parameters, XGDbType, NoBackslashEscapes, ReplaceLineBreaksWithCharFunction);

        protected override RelationalTypeMapping Clone(bool? noBackslashEscapes = null, bool? replaceLineBreaksWithCharFunction = null)
            => new XGJsonNewtonsoftTypeMapping<T>(
                Parameters,
                XGDbType,
                noBackslashEscapes ?? NoBackslashEscapes,
                replaceLineBreaksWithCharFunction ?? ReplaceLineBreaksWithCharFunction);

        public override Expression GenerateCodeLiteral(object value)
            => value switch
            {
                JToken jToken => Expression.Call(
                    typeof(JToken).GetMethod(nameof(JToken.Parse), new[] {typeof(string), typeof(JsonLoadSettings)}),
                    Expression.Constant(jToken.ToString(Formatting.None)),
                    Expression.New(typeof(JsonLoadSettings))),
                string s => Expression.Constant(s),
                _ => throw new NotSupportedException("Cannot generate code literals for JSON POCOs.")
            };
    }
}
