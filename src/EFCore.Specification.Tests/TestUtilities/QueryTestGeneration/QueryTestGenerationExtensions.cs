// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public static class QueryTestGenerationExtensions
    {
        public static TResult Choose<TResult>(this Random random, List<TResult> list)
        {
            if (list.Count == 0)
            {
                return default;
            }

            return list[random.Next(list.Count)];
        }
    }
}
