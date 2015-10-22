// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMutableEntityType : IEntityType, IMutableAnnotatable
    {
        new IMutableModel Model { get; }
        new IMutableEntityType BaseType { get; [param: CanBeNull] set; }

        IMutableKey SetPrimaryKey([CanBeNull] IReadOnlyList<IMutableProperty> properties);
        IMutableKey GetOrSetPrimaryKey([NotNull] IReadOnlyList<IMutableProperty> properties);
        new IMutableKey FindPrimaryKey();

        IMutableKey AddKey([NotNull] IReadOnlyList<IMutableProperty> properties);
        IMutableKey GetOrAddKey([NotNull] IReadOnlyList<IMutableProperty> properties);
        IMutableKey FindKey([NotNull] IReadOnlyList<IMutableProperty> properties);
        new IEnumerable<IMutableKey> GetKeys();
        IMutableKey RemoveKey([NotNull] IReadOnlyList<IMutableProperty> properties);

        IMutableForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<IMutableProperty> properties, [NotNull] IMutableKey principalKey, [NotNull] IMutableEntityType principalEntityType);

        IMutableForeignKey GetOrAddForeignKey(
            [NotNull] IReadOnlyList<IMutableProperty> properties, [NotNull] IMutableKey principalKey, [NotNull] IMutableEntityType principalEntityType);

        IMutableForeignKey FindForeignKey([NotNull] IReadOnlyList<IMutableProperty> properties);
        new IEnumerable<IMutableForeignKey> GetForeignKeys();
        IMutableForeignKey RemoveForeignKey([NotNull] IReadOnlyList<IMutableProperty> properties);

        IMutableNavigation AddNavigation([NotNull] string name, [NotNull] IMutableForeignKey foreignKey, bool pointsToPrincipal);
        IMutableNavigation GetOrAddNavigation([NotNull] string name, [NotNull] IMutableForeignKey foreignKey, bool pointsToPrincipal);
        new IMutableNavigation FindNavigation([NotNull] string name);
        new IEnumerable<IMutableNavigation> GetNavigations();
        IMutableNavigation RemoveNavigation([NotNull] string name);

        IMutableIndex AddIndex([NotNull] IReadOnlyList<IMutableProperty> properties);
        IMutableIndex GetOrAddIndex([NotNull] IReadOnlyList<IMutableProperty> properties);
        IMutableIndex FindIndex([NotNull] IReadOnlyList<IMutableProperty> properties);
        new IEnumerable<IMutableIndex> GetIndexes();
        IMutableIndex RemoveIndex([NotNull] IReadOnlyList<IMutableProperty> properties);

        IMutableProperty AddProperty([NotNull] string name);
        IMutableProperty GetOrAddProperty([NotNull] string name);
        new IMutableProperty FindProperty([NotNull] string name);
        new IEnumerable<IMutableProperty> GetProperties();
        IMutableProperty RemoveProperty([NotNull] string name);
    }
}
