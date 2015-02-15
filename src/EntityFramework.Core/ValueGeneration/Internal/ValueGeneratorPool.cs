// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration.Internal
{
    public class ValueGeneratorPool : IValueGeneratorPool
    {
        private readonly ThreadSafeLazyRef<ValueGenerator>[] _pool;
        private int _currentPosition;

        public ValueGeneratorPool([NotNull] ValueGeneratorFactory factory, [NotNull] IProperty property, int poolSize)
        {
            _pool = new ThreadSafeLazyRef<ValueGenerator>[poolSize];
            for (var i = 0; i < poolSize; i++)
            {
                _pool[i] = new ThreadSafeLazyRef<ValueGenerator>(() => factory.Create(property));
            }
        }

        public virtual ValueGenerator GetGenerator()
        {
            _currentPosition = (_currentPosition + 1) % _pool.Length;

            return _pool[_currentPosition].Value;
        }
    }
}
