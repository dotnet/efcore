// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Operations;

namespace Microsoft.Data.Entity.Relational.Migrations.Builders
{
    public class ColumnBuilder
    {
        public virtual ColumnModel Column(
            [NotNull] string storeType,
            [CanBeNull] string name = null,
            bool nullable = false,
            [CanBeNull] object defaultValue = null,
            [CanBeNull] string defaultValueSql = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
            new ColumnModel(name, storeType, nullable, defaultValue, defaultValueSql, annotations);
    }
}
