// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    [DebuggerDisplay("AtsTableQuery")]
    public class AtsTableQuery
    {
        private readonly IList<TableFilter> _filters = new List<TableFilter>();

        internal TableQuery ToExecutableQuery()
        {
            var query = new TableQuery();
            query.Where(ToString());
            return query;
        }

        public override string ToString()
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

        public virtual AtsTableQuery WithFilter([NotNull] TableFilter filter)
        {
            Check.NotNull(filter, "filter");
            _filters.Add(filter);
            return this;
        }

        protected bool Equals(AtsTableQuery other)
        {
            return ToString().Equals(other.ToString());
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((AtsTableQuery)obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
