// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerNameMapper
    {
        private readonly IModel _sourceModel;
        private readonly Func<IEntityType, string> _defaultEntityNameFunc;
        private readonly Func<IProperty, string> _defaultPropertyNameFunc;
        private Dictionary<IEntityType, string> _entityTypeToClassNameMap;
        private Dictionary<IProperty, string> _propertyToPropertyNameMap;

        public SqlServerNameMapper([NotNull] IModel sourceModel,
            [NotNull] Func<IEntityType, string> defaultEntityNameFunc,
            [NotNull] Func<IProperty, string> defaultPropertyNameFunc)
        {
            Check.NotNull(defaultEntityNameFunc, nameof(defaultEntityNameFunc));
            Check.NotNull(defaultPropertyNameFunc, nameof(defaultPropertyNameFunc));

            _sourceModel = sourceModel;
            _defaultEntityNameFunc = defaultEntityNameFunc;
            _defaultPropertyNameFunc = defaultPropertyNameFunc;
        }

        public virtual Dictionary<IEntityType, string> EntityTypeToClassNameMap
        {
            get
            {
                if (_entityTypeToClassNameMap == null)
                {
                    ConstructNameMaps();
                }

                return _entityTypeToClassNameMap;
            }
        }

        public virtual Dictionary<IProperty, string> PropertyToPropertyNameMap
        {
            get
            {
                if (_entityTypeToClassNameMap == null)
                {
                    ConstructNameMaps();
                }

                return _propertyToPropertyNameMap;
            }
        }

        private void ConstructNameMaps()
        {
            _entityTypeToClassNameMap = new Dictionary<IEntityType, string>();
            _propertyToPropertyNameMap = new Dictionary<IProperty, string>();
            foreach (var entityType in _sourceModel.EntityTypes)
            {
                _entityTypeToClassNameMap[entityType] =
                    CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        _defaultEntityNameFunc(entityType), _entityTypeToClassNameMap.Values);
                ConstructPropertyNameMap(entityType);
            }
        }

        private void ConstructPropertyNameMap(IEntityType entityType)
        {
            // use local propertyToPropertyNameMap to ensure no clashes in Property names
            // within an EntityType but to allow them for properties in different EntityTypes.
            // Also name of Property cannot be the same as the name of the enclosing EntityType.
            var entityTypeName = new[] { _entityTypeToClassNameMap[entityType] };
            var propertyToPropertyNameMap = new Dictionary<IProperty, string>();
            foreach (var property in entityType.GetProperties())
            {
                var existingNames = propertyToPropertyNameMap.Values
                    .Concat(entityTypeName).ToList();
                propertyToPropertyNameMap[property] =
                    CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        _defaultPropertyNameFunc(property),
                        existingNames);
            }

            foreach (var keyValuePair in propertyToPropertyNameMap)
            {
                _propertyToPropertyNameMap.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
