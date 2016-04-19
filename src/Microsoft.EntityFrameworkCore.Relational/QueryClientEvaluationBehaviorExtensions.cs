// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore
{
    public static class QueryClientEvaluationBehaviorExtensions
    {
        private static readonly int _maxQueryClientEvaluationBehavior
            = Enum.GetValues(typeof(QueryClientEvaluationBehavior)).Cast<int>().Max();

        public static void Validate(this QueryClientEvaluationBehavior behavior)
        {
            if (behavior < 0
                || (int)behavior > _maxQueryClientEvaluationBehavior)
            {
                throw new ArgumentException(CoreStrings.InvalidEnumValue(nameof(behavior), typeof(QueryClientEvaluationBehavior)));
            }
        }
    }
}
