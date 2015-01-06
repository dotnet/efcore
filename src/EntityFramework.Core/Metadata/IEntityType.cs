// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityType : IMetadata
    {
        IModel Model { get; }

        string Name { get; }

        string SimpleName { get; }

        [CanBeNull]
        Type Type { get; }

        [CanBeNull]
        IKey TryGetPrimaryKey();

        IKey GetPrimaryKey();

        [CanBeNull]
        IProperty TryGetProperty([NotNull] string name);

        [NotNull]
        IProperty GetProperty([NotNull] string name);

        [CanBeNull]
        INavigation TryGetNavigation([NotNull] string name);

        [NotNull]
        INavigation GetNavigation([NotNull] string name);

        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<INavigation> Navigations { get; }
        IReadOnlyList<IIndex> Indexes { get; }
        IReadOnlyList<IKey> Keys { get; }

        int ShadowPropertyCount { get; }
        int OriginalValueCount { get; }
        bool HasClrType { get; }
        bool UseEagerSnapshots { get; }
    }
}
