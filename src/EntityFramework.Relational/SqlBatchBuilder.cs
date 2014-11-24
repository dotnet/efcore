// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational
{
    public class SqlBatchBuilder
    {
        private List<SqlBatch> _batches;
        private IndentedStringBuilder _stringBuilder;
        private bool _transactionSuppressed;

        public SqlBatchBuilder()
        {
            _batches = new List<SqlBatch>();
            _stringBuilder = new IndentedStringBuilder();
        }

        public virtual IReadOnlyList<SqlBatch> SqlBatches
        {
            get
            {
                return _batches;
            }
        }

        public virtual void EndBatch()
        {
            var sql = _stringBuilder.ToString();
            var sqlBatch = new SqlBatch(sql);
            sqlBatch.SuppressTransaction = _transactionSuppressed;

            if (!string.IsNullOrEmpty(sql))
            {
                _batches.Add(sqlBatch);
            }

            _stringBuilder = new IndentedStringBuilder();
        }

        public virtual SqlBatchBuilder Append([NotNull] object o, bool suppressTransaction = false)
        {
            if (suppressTransaction && !_transactionSuppressed)
            {
                EndBatch();
                _transactionSuppressed = true;
            }

            _stringBuilder.Append(o);
            return this;
        }

        public virtual SqlBatchBuilder AppendLine(bool suppressTransaction = false)
        {
            return AppendLine(string.Empty, suppressTransaction);
        }

        public virtual SqlBatchBuilder AppendLine([NotNull] object o, bool suppressTransaction = false)
        {
            if (suppressTransaction && !_transactionSuppressed)
            {
                EndBatch();
                _transactionSuppressed = true;
            }
            
            _stringBuilder.AppendLine(o);
            return this;
        }

        public virtual IDisposable Indent()
        {
            return _stringBuilder.Indent();
        }
    }
}
