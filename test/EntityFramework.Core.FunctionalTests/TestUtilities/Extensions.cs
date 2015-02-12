// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity.FunctionalTests
{
    public static class Extensions
    {
        public static IEnumerable<T> NullChecked<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }
    }
}
