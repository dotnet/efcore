// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Utilities
{
    [DebuggerStepThrough]
    public sealed class LazyRef<T>
    {
        private Func<T> _initializer;
        private T _value;

        public LazyRef([NotNull] Func<T> initializer)
        {
            Check.NotNull(initializer, nameof(initializer));

            _initializer = initializer;
        }

        public LazyRef([CanBeNull] T value)
        {
            _value = value;
        }

        public T Value
        {
            get
            {
                if (_initializer != null)
                {
                    _value = _initializer();
                    _initializer = null;
                }

                return _value;
            }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _value = value;
                _initializer = null;
            }
        }

        public bool HasValue => _initializer == null;

        public void Reset([NotNull] Func<T> initializer)
        {
            Check.NotNull(initializer, nameof(initializer));

            _initializer = initializer;
            _value = default(T);
        }
    }
}
