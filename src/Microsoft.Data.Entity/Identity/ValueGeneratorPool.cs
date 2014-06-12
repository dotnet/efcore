// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Identity
{
    public class ValueGeneratorPool : IValueGeneratorPool
    {
        private readonly ThreadSafeLazyRef<IValueGenerator>[] _pool;
        private int _currentPosition;

        public ValueGeneratorPool([NotNull] IValueGeneratorFactory factory, [NotNull] IProperty property, int poolSize)
        {
            Check.NotNull(factory, "factory");
            Check.NotNull(property, "property");

            _pool = new ThreadSafeLazyRef<IValueGenerator>[poolSize];
            for (var i = 0; i < poolSize; i++)
            {
                _pool[i] = new ThreadSafeLazyRef<IValueGenerator>(() => factory.Create(property));
            }
        }

        public virtual IValueGenerator GetGenerator()
        {
            _currentPosition = (_currentPosition + 1) % _pool.Length;

            return _pool[_currentPosition].Value;
        }
    }
}
