// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public static class QueryExtensions
    {
        public static readonly MethodInfo ValueBufferPropertyMethodInfo
            = typeof(QueryExtensions).GetTypeInfo().GetDeclaredMethod(nameof(ValueBufferProperty));

        [UsedImplicitly]
        private static TProperty ValueBufferProperty<TProperty>(ValueBuffer valueBuffer, string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}
