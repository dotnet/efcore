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
        Type Type { get; }
        IReadOnlyList<IProperty> Key { get; }
        IProperty Property([NotNull] string name);
        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<INavigation> Navigations { get; }
        int PropertyIndex([NotNull] string name);
    }
}
