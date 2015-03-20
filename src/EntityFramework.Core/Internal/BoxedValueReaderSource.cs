// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Internal
{
    public class BoxedValueReaderSource : IBoxedValueReaderSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(BoxedValueReaderSource).GetTypeInfo().GetDeclaredMethods("CreateGeneric").Single();

        private readonly ThreadSafeDictionaryCache<Type, IBoxedValueReader> _cache
            = new ThreadSafeDictionaryCache<Type, IBoxedValueReader>();

        public virtual IBoxedValueReader GetReader(IProperty property)
            => _cache.GetOrAdd(Check.NotNull(property, nameof(property)).ClrType.UnwrapNullableType(), Create);

        private IBoxedValueReader Create(Type type)
            => (IBoxedValueReader)_genericCreate.MakeGenericMethod(type).Invoke(this, null);

        protected IBoxedValueReader CreateGeneric<TValue>() => new GenericBoxedValueReader<TValue>();
    }
}
