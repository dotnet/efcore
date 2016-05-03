// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public abstract class ClrAccessorFactory<TAccessor>
        where TAccessor : class
    {
        private static readonly MethodInfo _genericCreate
             = typeof(ClrAccessorFactory<TAccessor>).GetTypeInfo().GetDeclaredMethods(nameof(CreateGeneric)).Single();

        public virtual TAccessor Create([NotNull] IPropertyBase property)
            => property as TAccessor ?? Create(property.DeclaringEntityType.ClrType.GetAnyProperty(property.Name));

        public virtual TAccessor Create([NotNull] PropertyInfo property)
        {
            var boundMethod = _genericCreate.MakeGenericMethod(
                property.DeclaringType,
                property.PropertyType,
                property.PropertyType.UnwrapNullableType());

            try
            {
                return (TAccessor)boundMethod.Invoke(this, new object[] { property });
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                throw e.InnerException;
            }
        }

        protected abstract TAccessor CreateGeneric<TEntity, TValue, TNonNullableEnumValue>([NotNull] PropertyInfo property)
            where TEntity : class;
    }
}
