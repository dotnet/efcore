// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if K10
using System;
using System.Collections.Generic;

namespace Cud.Utilities
{
    internal static class ListExtensions
    {
        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            foreach (var element in list)
            {
                action(element);
            }
        }
    }
}
#endif
