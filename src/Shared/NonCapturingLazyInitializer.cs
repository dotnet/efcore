// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    internal static class NonCapturingLazyInitializer
    {
        public static TValue EnsureInitialized<TParam, TValue>(
            [CanBeNull] ref TValue target,
            [CanBeNull] TParam param,
            [NotNull] Func<TParam, TValue> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                return tmp;
            }

            Interlocked.CompareExchange(ref target, valueFactory(param), null);

            return target!;
        }

        public static TValue EnsureInitialized<TParam1, TParam2, TValue>(
            [CanBeNull] ref TValue target,
            [CanBeNull] TParam1 param1,
            [CanBeNull] TParam2 param2,
            [NotNull] Func<TParam1, TParam2, TValue> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                return tmp;
            }

            Interlocked.CompareExchange(ref target, valueFactory(param1, param2), null);

            return target!;
        }

        public static TValue EnsureInitialized<TValue>(
            [CanBeNull] ref TValue target,
            [NotNull] TValue value)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                return tmp;
            }

            Interlocked.CompareExchange(ref target, value, null);

            return target!;
        }

        public static TValue EnsureInitialized<TParam, TValue>(
            [CanBeNull] ref TValue target,
            [CanBeNull] TParam param,
            [NotNull] Action<TParam> valueFactory)
            where TValue : class
        {
            if (Volatile.Read(ref target) != null)
            {
                return target!;
            }

            valueFactory(param);

            return Volatile.Read(ref target)!;
        }
    }
}
