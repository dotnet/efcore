// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class TableNameFromDbSetConvention : IBaseTypeConvention
    {
        private readonly IDictionary<Type, DbSetProperty> _sets;

        public TableNameFromDbSetConvention([CanBeNull] DbContext context, [CanBeNull] IDbSetFinder setFinder)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            _sets = setFinder?.CreateClrTypeDbSetMapping(context);
        }

        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (_sets != null)
            {
                var entityType = entityTypeBuilder.Metadata;

                if (oldBaseType == null
                    && entityType.BaseType != null)
                {
                    entityTypeBuilder.Relational(ConfigurationSource.Convention).TableName = null;
                }
                else if (oldBaseType != null
                         && entityType.BaseType == null
                         && _sets.ContainsKey(entityType.ClrType))
                {
                    entityTypeBuilder.Relational(ConfigurationSource.Convention).TableName
                        = _sets[entityType.ClrType].Name;
                }
            }
            return true;
        }
    }
}
