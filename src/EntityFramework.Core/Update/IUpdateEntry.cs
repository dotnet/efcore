// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Update
{
    public interface IUpdateEntry
    {
        IEntityType EntityType { get; }
        EntityState EntityState { get; }
        bool IsModified([NotNull] IProperty property);
        bool IsStoreGenerated([NotNull] IProperty property);
        object GetOriginalValue([NotNull] IProperty property);
        object this[[NotNull] IPropertyBase propertyBase] { get; [param: CanBeNull] set; }

        IKeyValue GetPrimaryKeyValue(bool originalValue = false);
        IKeyValue GetPrincipalKeyValue([NotNull] IForeignKey foreignKey, bool originalValue = false);
        IKeyValue GetDependentKeyValue([NotNull] IForeignKey foreignKey, bool originalValue = false);

        EntityEntry ToEntityEntry();
    }
}
