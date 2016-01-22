// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable 0169

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    internal class ImplyGeneric<T1, T2, T3, T4>
    {
        public void ImplyMethods()
        {
            QueryMethodProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null, null);

            AsyncQueryMethodProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null, null);
        }
    }
}
