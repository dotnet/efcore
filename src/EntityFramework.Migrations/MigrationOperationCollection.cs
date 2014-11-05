// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public class MigrationOperationCollection
    {
        private readonly Dictionary<Type, List<MigrationOperation>> _allOperations
            = new Dictionary<Type, List<MigrationOperation>>();

        public virtual bool Add<T>([NotNull] T operation, [CanBeNull] Func<T, T, bool> compareFunc = null)
            where T : MigrationOperation
        {
            Check.NotNull(operation, "operation");

            List<MigrationOperation> operationList;

            if (_allOperations.TryGetValue(typeof(T), out operationList))
            {
                if (compareFunc != null
                    && operationList.Any(op => compareFunc((T)op, operation)))
                {
                    return false;
                }

                operationList.Add(operation);
            }
            else
            {
                _allOperations.Add(typeof(T), new List<MigrationOperation> { operation });
            }

            return true;
        }

        public virtual void AddRange<T>([NotNull] IEnumerable<T> operations)
            where T : MigrationOperation
        {
            Check.NotNull(operations, "operations");

            List<MigrationOperation> operationList;

            if (_allOperations.TryGetValue(typeof(T), out operationList))
            {
                operationList.AddRange(operations);
            }
            else
            {
                _allOperations.Add(typeof(T), new List<MigrationOperation>(operations));
            }
        }

        public virtual void Set<T>([NotNull] IEnumerable<T> operations)
            where T : MigrationOperation
        {
            Check.NotNull(operations, "operations");

            _allOperations[typeof(T)] = new List<MigrationOperation>(operations);
        }

        public virtual IReadOnlyList<T> Get<T>()
            where T : MigrationOperation
        {
            List<MigrationOperation> operationList;

            return
                _allOperations.TryGetValue(typeof(T), out operationList)
                    ? (IReadOnlyList<T>)operationList.Cast<T>().ToList()
                    : new T[0];
        }

        public virtual bool Remove<T>([NotNull] T operation)
            where T : MigrationOperation
        {
            List<MigrationOperation> operationList;

            return 
                _allOperations.TryGetValue(typeof(T), out operationList) 
                && operationList.Remove(operation);
        }

        public virtual IReadOnlyList<MigrationOperation> GetAll()
        {
            return
                ((IEnumerable<MigrationOperation>)Get<DropSequenceOperation>())
                    .Concat(Get<MoveSequenceOperation>())
                    .Concat(Get<RenameSequenceOperation>())
                    .Concat(Get<AlterSequenceOperation>())
                    .Concat(Get<CreateSequenceOperation>())
                    .Concat(Get<DropIndexOperation>())
                    .Concat(Get<DropForeignKeyOperation>())
                    .Concat(Get<DropUniqueConstraintOperation>())
                    .Concat(Get<DropPrimaryKeyOperation>())
                    .Concat(Get<DropDefaultConstraintOperation>())
                    .Concat(Get<DropColumnOperation>())
                    .Concat(Get<DropTableOperation>())
                    .Concat(Get<MoveTableOperation>())
                    .Concat(Get<RenameTableOperation>())
                    .Concat(Get<RenameColumnOperation>())
                    .Concat(Get<RenameIndexOperation>())
                    .Concat(Get<AlterColumnOperation>())
                    .Concat(Get<AddColumnOperation>())
                    .Concat(Get<CreateTableOperation>())
                    .Concat(Get<AddDefaultConstraintOperation>())
                    .Concat(Get<AddPrimaryKeyOperation>())
                    .Concat(Get<AddUniqueConstraintOperation>())
                    .Concat(Get<AddForeignKeyOperation>())
                    .Concat(Get<CreateIndexOperation>())
                    .ToList();            
        }
    }
}
