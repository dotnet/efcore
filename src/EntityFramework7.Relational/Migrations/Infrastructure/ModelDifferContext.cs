// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class ModelDifferContext
    {
        private readonly IDictionary<IEntityType, IEntityType> _entityTypeMap = new Dictionary<IEntityType, IEntityType>();
        private readonly IDictionary<IProperty, IProperty> _propertyMap = new Dictionary<IProperty, IProperty>();
        private readonly IDictionary<IEntityType, CreateTableOperation> _createTableOperations
            = new Dictionary<IEntityType, CreateTableOperation>();
        private readonly IDictionary<IEntityType, DropTableOperation> _dropTableOperations
            = new Dictionary<IEntityType, DropTableOperation>();
        private readonly IDictionary<DropTableOperation, IEntityType> _removedEntityTypes
            = new Dictionary<DropTableOperation, IEntityType>();

        public virtual void AddMapping([NotNull] IEntityType source, [NotNull] IEntityType target)
            => _entityTypeMap.Add(source, target);
        public virtual void AddMapping([NotNull] IProperty source, [NotNull] IProperty target)
            => _propertyMap.Add(source, target);
        public virtual void AddCreate([NotNull] IEntityType target, [NotNull] CreateTableOperation operation)
            => _createTableOperations.Add(target, operation);

        public virtual void AddDrop([NotNull] IEntityType source, [NotNull] DropTableOperation operation)
        {
            _dropTableOperations.Add(source, operation);
            _removedEntityTypes.Add(operation, source);
        }

        public virtual IEntityType FindTarget([NotNull] IEntityType source)
        {
            IEntityType target;
            _entityTypeMap.TryGetValue(source, out target);

            return target;
        }

        public virtual IProperty FindTarget([NotNull] IProperty source)
        {
            IProperty target;
            _propertyMap.TryGetValue(source, out target);

            return target;
        }

        public virtual CreateTableOperation FindCreate([NotNull] IEntityType target)
        {
            CreateTableOperation operation;
            _createTableOperations.TryGetValue(target, out operation);

            return operation;
        }

        public virtual DropTableOperation FindDrop([NotNull] IEntityType source)
        {
            DropTableOperation operation;
            _dropTableOperations.TryGetValue(source, out operation);

            return operation;
        }

        public virtual IEntityType GetMetadata([NotNull] DropTableOperation operation)
        {
            IEntityType source;
            _removedEntityTypes.TryGetValue(operation, out source);

            return source;
        }
    }
}
