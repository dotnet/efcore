// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Utilities
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
            Check.NotNull(initializer, "initializer");

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
            Check.NotNull(newValueCreator, "newValueCreator");

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

        public bool HasValue
        {
            get { return _value != null; }
        }
    }
}
