// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledProperty<TProperty> : CompiledMetadataBase
    {
        protected CompiledProperty(IEntityType entityType)
        {
            EntityType = entityType;
        }

        public Type ClrType => typeof(TProperty);

        public Type UnderlyingType => Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);

        public virtual bool IsStoreComputed => false;

        public virtual bool IsValueGeneratedOnAdd => false;

        public bool IsNullable
        {
            get
            {
                var typeInfo = typeof(TProperty).GetTypeInfo();
                return !typeInfo.IsValueType || (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>));
            }
        }

        public bool IsReadOnly => false;

        public bool UseStoreDefault => false;

        public IEntityType EntityType { get; }

        public bool IsShadowProperty => false;

        public bool IsConcurrencyToken => true;

        public object SentinelValue => null;
    }
}
