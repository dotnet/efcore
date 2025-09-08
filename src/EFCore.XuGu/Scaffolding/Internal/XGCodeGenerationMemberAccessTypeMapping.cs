// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    internal class XGCodeGenerationMemberAccessTypeMapping : RelationalTypeMapping
    {
        private const string DummyStoreType = "clrOnly";

        public static XGCodeGenerationMemberAccessTypeMapping Default { get; } = new();

        public XGCodeGenerationMemberAccessTypeMapping()
            : base(new RelationalTypeMappingParameters(new CoreTypeMappingParameters(typeof(XGCodeGenerationMemberAccess)), DummyStoreType))
        {
        }

        protected XGCodeGenerationMemberAccessTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGCodeGenerationMemberAccessTypeMapping(parameters);

        public override string GenerateSqlLiteral(object value)
            => throw new InvalidOperationException("This type mapping exists for code generation only.");

        public override Expression GenerateCodeLiteral(object value)
            => value is XGCodeGenerationMemberAccess memberAccess
                ? Expression.MakeMemberAccess(null, memberAccess.MemberInfo)
                : null;
    }
}
