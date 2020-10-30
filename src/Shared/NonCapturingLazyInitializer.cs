// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using CA = System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Internal
{
    internal static class NonCapturingLazyInitializer
    {
        public static TValue EnsureInitialized<TParam, TValue>(
            [CanBeNull, CA.NotNull] ref TValue? target,
            [NotNull] TParam param,
            [NotNull] Func<TParam, TValue> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                Check.DebugAssert(target != null, $"target was null in {nameof(EnsureInitialized)} after check");
                return tmp;
            }

            Interlocked.CompareExchange(ref target, valueFactory(param), null);

            return target;
        }

        public static TValue EnsureInitialized<TParam1, TParam2, TValue>(
            [CanBeNull, CA.NotNull] ref TValue? target,
            [NotNull] TParam1 param1,
            [NotNull] TParam2 param2,
            [NotNull] Func<TParam1, TParam2, TValue> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                Check.DebugAssert(target != null, $"target was null in {nameof(EnsureInitialized)} after check");
                return tmp;
            }

            Interlocked.CompareExchange(ref target, valueFactory(param1, param2), null);

            return target;
        }

        public static TValue EnsureInitialized<TValue>(
            [CanBeNull, CA.NotNull] ref TValue? target,
            [NotNull] TValue value)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                Check.DebugAssert(target != null, $"target was null in {nameof(EnsureInitialized)} after check");
                return tmp;
            }

            Interlocked.CompareExchange(ref target, value, null);

            return target;
        }

        public static TValue EnsureInitialized<TParam, TValue>(
            [CanBeNull, CA.NotNull] ref TValue? target,
            [NotNull] TParam param,
            [NotNull] Action<TParam> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                Check.DebugAssert(target != null, $"target was null in {nameof(EnsureInitialized)} after check");
                return tmp;
            }

            valueFactory(param);

            var tmp2 = Volatile.Read(ref target);
            Check.DebugAssert(target != null && tmp2 != null,
                $"{nameof(valueFactory)} did not initialize {nameof(target)} in {nameof(EnsureInitialized)}");
            return tmp2;
        }
    }
}
