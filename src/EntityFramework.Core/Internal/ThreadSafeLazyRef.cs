// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Internal
{
    [DebuggerStepThrough]
    public sealed class ThreadSafeLazyRef<T>
        where T : class
    {
        private Func<T> _initializer;
        private object _syncLock;

        private T _value;

        public ThreadSafeLazyRef([NotNull] Func<T> initializer)
        {
            _initializer = initializer;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    var syncLock = new object();

                    syncLock
                        = Interlocked.CompareExchange(ref _syncLock, syncLock, null)
                          ?? syncLock;

                    lock (syncLock)
                    {
                        if (_value == null)
                        {
                            _value = _initializer();

                            _syncLock = null;
                            _initializer = null;
                        }
                    }
                }

                return _value;
            }
        }

        public void ExchangeValue([NotNull] Func<T, T> newValueCreator)
        {
            T originalValue, newValue;

            do
            {
                originalValue = Value;
                newValue = newValueCreator(originalValue);

                if (ReferenceEquals(newValue, originalValue))
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref _value, newValue, originalValue) != originalValue);
        }

        public bool HasValue => _value != null;
    }
}
