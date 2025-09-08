// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGGuidTypeMapping : GuidTypeMapping, IJsonSpecificTypeMapping, IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator
    {
        public static new XGGuidTypeMapping Default { get; } = new();

        public XGGuidTypeMapping()
            : this(new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(Guid),
                        jsonValueReaderWriter: JsonGuidReaderWriter.Instance),
                    "guid",
                    StoreTypePostfix.Size,
                    System.Data.DbType.Guid,
                    false,
                    null,
                    true))
        {
        }

        protected XGGuidTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGGuidTypeMapping(parameters);

        public virtual RelationalTypeMapping Clone()
            => new XGGuidTypeMapping();


        protected override string GenerateNonNullSqlLiteral(object value)
        {
            return $"'{value:D}'";
        }


        protected static byte[] GetBytesFromGuid(Guid guid)
        {
            var bytes = guid.ToByteArray();

            return bytes;
        }

        /// <summary>
        /// For JSON values, we will always use the 36 character string representation.
        /// </summary>
        public virtual RelationalTypeMapping CloneAsJsonCompatible()
            => new XGGuidTypeMapping();

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
