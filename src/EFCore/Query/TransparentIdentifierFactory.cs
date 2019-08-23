// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{
    public static class TransparentIdentifierFactory
    {
        public static Type Create(Type outerType, Type innerType)
            => typeof(TransparentIdentifier<,>).MakeGenericType(outerType, innerType);

        private readonly struct TransparentIdentifier<TOuter, TInner>
        {
            [UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members
            public TransparentIdentifier(TOuter outer, TInner inner)
#pragma warning restore IDE0051 // Remove unused private members
            {
                Outer = outer;
                Inner = inner;
            }

            [UsedImplicitly]
            public readonly TOuter Outer;

            [UsedImplicitly]
            public readonly TInner Inner;
        }
    }
}
