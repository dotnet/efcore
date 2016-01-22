// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public abstract class ClrAccessorFactory<TAccessor>
        where TAccessor : class
    {
        public virtual TAccessor Create([NotNull] IPropertyBase property)
            => property as TAccessor ?? Create(property.DeclaringEntityType.ClrType.GetAnyProperty(property.Name));

        // TODO revisit when .NET Native supports ImpliesMethodInstantiation
        // original version used generics, which is much cleaner and performant but fails after ILC strips reflection info
        // https://github.com/aspnet/EntityFramework/issues/3477
        public abstract TAccessor Create([NotNull] PropertyInfo property);
    }
}
