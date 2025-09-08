// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGJsonTypeMapping<T> : XGJsonTypeMapping
    {
        public static new XGJsonTypeMapping<T> Default { get; } = new("json", null, null, false, true);

        public XGJsonTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter valueConverter,
            [CanBeNull] ValueComparer valueComparer,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(
                storeType,
                typeof(T),
                valueConverter,
                valueComparer,
                noBackslashEscapes,
                replaceLineBreaksWithCharFunction)
        {
        }

        protected XGJsonTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType xgDbType,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(parameters, xgDbType, noBackslashEscapes, replaceLineBreaksWithCharFunction)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGJsonTypeMapping<T>(parameters, XGDbType, NoBackslashEscapes, ReplaceLineBreaksWithCharFunction);

        protected override RelationalTypeMapping Clone(bool? noBackslashEscapes = null, bool? replaceLineBreaksWithCharFunction = null)
            => new XGJsonTypeMapping<T>(
                Parameters,
                XGDbType,
                noBackslashEscapes ?? NoBackslashEscapes,
                replaceLineBreaksWithCharFunction ?? ReplaceLineBreaksWithCharFunction);
    }

    public abstract class XGJsonTypeMapping : XGStringTypeMapping, IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator
    {
        public XGJsonTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] ValueConverter valueConverter,
            [CanBeNull] ValueComparer valueComparer,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType,
                        valueConverter,
                        valueComparer),
                    storeType,
                    unicode: true),
                XGDbType.Json,
                noBackslashEscapes,
                replaceLineBreaksWithCharFunction,
                false,
                false)
        {
            if (storeType != "json")
            {
                throw new ArgumentException($"The store type '{nameof(storeType)}' must be 'json'.", nameof(storeType));
            }
        }

        protected XGJsonTypeMapping(
            RelationalTypeMappingParameters parameters,
            XGDbType xgDbType,
            bool noBackslashEscapes,
            bool replaceLineBreaksWithCharFunction)
            : base(
                parameters,
                xgDbType,
                noBackslashEscapes,
                replaceLineBreaksWithCharFunction,
                isUnquoted: false,
                forceToString: false)
        {
        }

        /// <summary>
        /// Supports compiled models via IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator.Create.
        /// </summary>
        protected abstract RelationalTypeMapping Clone(
            bool? noBackslashEscapes = null,
            bool? replaceLineBreaksWithCharFunction = null);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            // XuguClient does not know how to handle our custom XGJsonString type, that could be used when a
            // string parameter is explicitly cast to it.
            if (parameter.Value is XGJsonString xgJsonString)
            {
                parameter.Value = (string)xgJsonString;
            }
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
