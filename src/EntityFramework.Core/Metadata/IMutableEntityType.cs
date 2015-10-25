// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMutableEntityType : IEntityType, IMutableAnnotatable
    {
        new IMutableModel Model { get; }
        new IMutableEntityType BaseType { get; [param: CanBeNull] set; }

        IMutableKey SetPrimaryKey([CanBeNull] IReadOnlyList<IMutableProperty> properties);
        new IMutableKey FindPrimaryKey();

        IMutableKey AddKey([NotNull] IReadOnlyList<IMutableProperty> properties);
        new IMutableKey FindKey([NotNull] IReadOnlyList<IProperty> properties);
        new IEnumerable<IMutableKey> GetKeys();
        IMutableKey RemoveKey([NotNull] IReadOnlyList<IProperty> properties);

        IMutableForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<IMutableProperty> properties, [NotNull] IMutableKey principalKey, [NotNull] IMutableEntityType principalEntityType);

        new IMutableForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType);

        new IEnumerable<IMutableForeignKey> GetForeignKeys();
        IMutableForeignKey RemoveForeignKey([NotNull] IReadOnlyList<IProperty> properties, [NotNull] IKey principalKey, [NotNull] IEntityType principalEntityType);

        IMutableNavigation AddNavigation([NotNull] string name, [NotNull] IMutableForeignKey foreignKey, bool pointsToPrincipal);
        new IMutableNavigation FindNavigation([NotNull] string name);
        new IEnumerable<IMutableNavigation> GetNavigations();
        IMutableNavigation RemoveNavigation([NotNull] string name);

        IMutableIndex AddIndex([NotNull] IReadOnlyList<IMutableProperty> properties);
        new IMutableIndex FindIndex([NotNull] IReadOnlyList<IProperty> properties);
        new IEnumerable<IMutableIndex> GetIndexes();
        IMutableIndex RemoveIndex([NotNull] IReadOnlyList<IProperty> properties);

        IMutableProperty AddProperty([NotNull] string name);
        new IMutableProperty FindProperty([NotNull] string name);
        new IEnumerable<IMutableProperty> GetProperties();
        IMutableProperty RemoveProperty([NotNull] string name);
    }
}
