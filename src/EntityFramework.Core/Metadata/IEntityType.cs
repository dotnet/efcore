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

        IProperty FindProperty([NotNull] string name);

        IForeignKey FindForeignKey([NotNull] IReadOnlyList<IProperty> properties);

        INavigation FindNavigation([NotNull] string name);

        IIndex FindIndex([NotNull] IReadOnlyList<IProperty> properties);

        IKey FindKey([NotNull] IReadOnlyList<IProperty> properties);

        IEnumerable<IProperty> GetProperties();

        IEnumerable<IForeignKey> GetForeignKeys();

        IEnumerable<INavigation> GetNavigations();

        IEnumerable<IIndex> GetIndexes();

        IEnumerable<IKey> GetKeys();
    }
}
