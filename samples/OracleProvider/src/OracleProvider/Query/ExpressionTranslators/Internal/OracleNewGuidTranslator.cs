// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Oracle.Query.ExpressionTranslators.Internal
{
    public class OracleNewGuidTranslator : SingleOverloadStaticMethodCallTranslator
    {
        public OracleNewGuidTranslator()
            : base(typeof(Guid), nameof(Guid.NewGuid), "SYS_GUID")
        {
        }
    }
}
