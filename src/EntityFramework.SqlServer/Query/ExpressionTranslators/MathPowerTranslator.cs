// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public class MathPowerTranslator : SingleOverloadStaticMethodCallTranslator
    {
        public MathPowerTranslator()
            : base(typeof(Math), nameof(Math.Pow), "POWER")
        {
        }
    }
}
