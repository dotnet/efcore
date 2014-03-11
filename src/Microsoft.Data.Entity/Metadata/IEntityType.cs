// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityType : IMetadata
    {
        string Name { get; }
        string StorageName { get; }

        [CanBeNull]
        Type Type { get; }

        IReadOnlyList<IProperty> Key { get; }

        [CanBeNull]
        IProperty TryGetProperty([NotNull] string name);

        [NotNull]
        IProperty GetProperty([NotNull] string name);

        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<INavigation> Navigations { get; }
        object CreateInstance([NotNull] object[] values);
        int ShadowPropertyCount { get; }
        bool HasClrType { get; }
    }
}
