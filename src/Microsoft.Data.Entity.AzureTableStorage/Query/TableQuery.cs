// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    [DebuggerDisplay("TableQuery<{ResultType}>")]
    public class TableQuery<TEntity> : ITableQuery
        where TEntity : ITableEntity
    {
        private readonly IList<TableFilter> _filters = new List<TableFilter>();

        public TableQuery()
        {
            ResultType = typeof(TEntity);
        }

        internal WindowsAzure.Storage.Table.TableQuery<TEntity> ToExecutableQuery()
        {
            var query = new WindowsAzure.Storage.Table.TableQuery<TEntity>();
            query.Where(Where);
            return query;
        }

        public string Where
        {
            get
            {
                if (_filters.Count == 0)
                {
                    return "";
                }
                return _filters
                    .Select(f => f.ToString())
                    .Aggregate((combined, piece) =>
                        String.IsNullOrWhiteSpace(piece) ?
                            combined :
                            TableQuery.CombineFilters(combined, TableOperators.And, piece)
                    );
            }
        }

        public override string ToString()
        {
            return Where;
        }

        public Type ResultType { get; private set; }

        public ITableQuery WithFilter(TableFilter filter)
        {
            _filters.Add(filter);
            return this;
        }
    }
}
