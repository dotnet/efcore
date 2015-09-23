// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqlServerStringToUpperTranslator : ParameterlessInstanceMethodCallTranslator
    {
        public SqlServerStringToUpperTranslator()
            : base(typeof(string), nameof(string.ToUpper), "UPPER")
        {
        }
    }
}
