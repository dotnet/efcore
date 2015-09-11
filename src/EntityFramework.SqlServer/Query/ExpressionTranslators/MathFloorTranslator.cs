// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public class MathFloorTranslator : MultipleOverloadStaticMethodCallTranslator
    {
        public MathFloorTranslator()
            : base(typeof(Math), nameof(Math.Floor), "FLOOR")
        {
        }
    }
}
