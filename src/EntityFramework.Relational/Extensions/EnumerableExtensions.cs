// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Finally<TSource>(this IEnumerable<TSource> source, Action finallyAction)
        {
            try
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
            finally
            {
                finallyAction();
            }
        }
    }
}
