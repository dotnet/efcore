// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityType : IAnnotatable
    {
        [NotNull]
        IModel Model { get; }

        [NotNull]
        string Name { get; }

        bool IsAbstract { get; }

        bool HasDerivedTypes { get; }

        [CanBeNull]
        IEntityType BaseType { get; }

        [CanBeNull]
        Type ClrType { get; }

        [NotNull]
        IKey GetPrimaryKey();

        [CanBeNull]
        IProperty FindProperty([NotNull] string name);

        [NotNull]
        IProperty GetProperty([NotNull] string name);

        [CanBeNull]
        INavigation FindNavigation([NotNull] string name);

        [NotNull]
        INavigation GetNavigation([NotNull] string name);

        [NotNull]
        IEnumerable<IProperty> GetProperties();

        [NotNull]
        IEnumerable<IForeignKey> GetForeignKeys();

        [NotNull]
        IEnumerable<INavigation> GetNavigations();

        [NotNull]
        IEnumerable<IIndex> GetIndexes();

        [NotNull]
        IEnumerable<IKey> GetKeys();

        IEnumerable<IEntityType> GetDerivedTypes();
        IEnumerable<IEntityType> GetConcreteTypesInHierarchy();
    }
}
