// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    public interface IKeyValueIndexFactory
    {
        IKeyValueIndex CreatePrincipalKeyValue([NotNull] InternalEntityEntry entry, [NotNull] IForeignKey foreignKey);
        IKeyValueIndex CreatePrincipalKeyValueFromOriginalValues([NotNull] InternalEntityEntry entry, [NotNull] IForeignKey foreignKey);
        IKeyValueIndex CreateDependentKeyValue([NotNull] InternalEntityEntry entry, [NotNull] IForeignKey foreignKey);
        IKeyValueIndex CreateDependentKeyValueFromOriginalValues([NotNull] InternalEntityEntry entry, [NotNull] IForeignKey foreignKey);
    }
}
