// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    internal class XGCodeGenerationServerVersionCreationTypeMapping : RelationalTypeMapping
    {
        private const string DummyStoreType = "clrOnly";

        public static XGCodeGenerationServerVersionCreationTypeMapping Default { get; } = new();

        public XGCodeGenerationServerVersionCreationTypeMapping()
            : base(new RelationalTypeMappingParameters(new CoreTypeMappingParameters(typeof(XGCodeGenerationServerVersionCreation)), DummyStoreType))
        {
        }

        protected XGCodeGenerationServerVersionCreationTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGCodeGenerationServerVersionCreationTypeMapping(parameters);

        public override string GenerateSqlLiteral(object value)
            => throw new InvalidOperationException("This type mapping exists for code generation only.");

        public override Expression GenerateCodeLiteral(object value)
            => value is XGCodeGenerationServerVersionCreation serverVersionCreation
                ? Expression.Call(
                    typeof(ServerVersion).GetMethod(nameof(ServerVersion.Parse), new[] {typeof(string)}),
                    Expression.Constant(serverVersionCreation.ServerVersion.ToString()))
                : null;
    }
}
