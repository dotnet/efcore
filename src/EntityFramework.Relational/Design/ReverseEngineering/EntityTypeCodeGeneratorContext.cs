// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class EntityTypeCodeGeneratorContext : ModelCodeGeneratorContext
    {
        private readonly IEntityType _entityType;
        private readonly string _namespaceName;
        private readonly DbContextCodeGeneratorContext _contextCodeGeneratorContext;

        public EntityTypeCodeGeneratorContext([NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName,
            [NotNull]DbContextCodeGeneratorContext contextCodeGeneratorContext)
        {
            _entityType = entityType;
            _namespaceName = namespaceName;
            _contextCodeGeneratorContext = contextCodeGeneratorContext;
        }

        public override string GetClassName()
        {
            return _contextCodeGeneratorContext.EntityTypeToClassNameMap[_entityType];
        }

        public override string GetClassNamespace()
        {
            return _namespaceName;
        }
        
        public override void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            sb.AppendLine("//");
            sb.AppendLine("// Generated code");
            sb.AppendLine("//");
            sb.AppendLine();
        }

        public override void GenerateProperties(IndentedStringBuilder sb)
        {
            //TODO
        }

        //
        // helper methods
        //

        public override IEnumerable<string> GetUsedNamespaces()
        {
            return base.GetUsedNamespaces().Concat(
                _entityType.Properties.Select(p => p.PropertyType.Namespace).Distinct().OrderBy(p => p));
        }

        //public override IEnumerable<IProperty> SortedProperties()
        //{
        //    return _entityTypeToClassNameMap.Values
        //    var primaryKeyPropertiesList = new List<IProperty>();
        //    IKey key = entityType.TryGetPrimaryKey();
        //    if (key != null)
        //    {
        //        primaryKeyPropertiesList = new List<IProperty>(key.Properties.OrderBy(p => p.Name));
        //    }

        //    return primaryKeyPropertiesList.Concat(
        //        entityType.Properties
        //        .Where(p => !primaryKeyPropertiesList.Contains(p)).OrderBy(p => p.Name));
        //}
    }
}