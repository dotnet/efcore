// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityType : IAnnotatable
    {
        IModel Model { get; }
        string Name { get; }
        IEntityType BaseType { get; }
        Type ClrType { get; }
        IKey FindPrimaryKey();
        IKey FindKey([NotNull] IReadOnlyList<IProperty> properties);
        IEnumerable<IKey> GetKeys();
        IForeignKey FindForeignKey([NotNull] IReadOnlyList<IProperty> properties);
        IEnumerable<IForeignKey> GetForeignKeys();
        INavigation FindNavigation([NotNull] string name);
        IEnumerable<INavigation> GetNavigations();
        IIndex FindIndex([NotNull] IReadOnlyList<IProperty> properties);
        IEnumerable<IIndex> GetIndexes();
        IProperty FindProperty([NotNull] string name);
        IEnumerable<IProperty> GetProperties();
    }
}
