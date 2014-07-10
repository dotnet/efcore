// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.SQLite;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly SimpleValueGeneratorFactory<TemporaryValueGenerator> _tempFactory;

        public SQLiteValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] SimpleValueGeneratorFactory<TemporaryValueGenerator> tempFactory)
            : base(guidFactory)
        {
            Check.NotNull(tempFactory, "tempFactory");

            _tempFactory = tempFactory;
        }

        public override IValueGeneratorFactory Select(IProperty property)
        {
            if (property.ValueGenerationOnAdd == ValueGenerationOnAdd.Client)
            {
                // If INTEGER PRIMARY KEY column...
                var keyProperties = property.EntityType.GetKey().Properties;
                if (keyProperties.Count == 1
                    && keyProperties[0] == property)
                {
                    // TODO: Consider using RelationalTypeMapper service
                    var integerClrType = SQLiteTypeMap.FromDeclaredType("INTEGER", SQLiteType.Integer).ClrType;
                    if (property.PropertyType == integerClrType
                        || string.Equals(property.ColumnType(), "INTEGER", StringComparison.OrdinalIgnoreCase))
                    {
                        // NOTE: This assumes no WITHOUT ROWID tables
                        return _tempFactory;
                    }
                }
            }

            return base.Select(property);
        }
    }
}
