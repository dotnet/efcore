// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable 0169

using System;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Utilities
{
    /// <summary>
    ///     DO NOT USE. <see cref="ImpliedEntityType{TEntity}" />
    /// </summary>
    internal partial class RelationalImplyTypes
    {
        public ImplyGeneric<ValueBuffer> ValueBufferProp;
        public ImplyGeneric<object> Object;
        public ImplyGeneric<string> String;
    }

    internal partial class ImplyGeneric<T>
    {
        public ImplyJoin<T, T> Join;

        public void ImplyMethods()
        {
            QueryMethodProvider.GetResult<T>(null);
            QueryMethodProvider._ShapedQuery<T>(null, null, null);
            QueryMethodProvider._Include<T>(null, null, null, null, null, true);
        }
    }

    internal partial class ImplyGeneric<T1, T2>
    {
        public Func<T1, T2> Func1;
        public Func<T2, T1> Func2;
        public ImplyJoin<T1, T2> Join1;
        public ImplyJoin<T2, T1> Join2;
    }

    internal class ImplyGeneric<T1, T2, T3, T4>
    {
        public void ImplyMethods()
        {
            QueryMethodProvider._GroupJoin<T1, T2, T3, T4>(null, null, null, null, null);
        }
    }
}
