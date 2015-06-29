// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqlBatchBuilder
    {
        private readonly List<SqlBatch> _batches = new List<SqlBatch>();
        private IndentedStringBuilder _stringBuilder = new IndentedStringBuilder();
        private bool _transactionSuppressed;

        public virtual IReadOnlyList<SqlBatch> SqlBatches => _batches;

        public virtual SqlBatchBuilder EndBatch()
        {
            var sql = _stringBuilder.ToString();
            var sqlBatch = new SqlBatch(sql);
            sqlBatch.SuppressTransaction = _transactionSuppressed;

            if (!string.IsNullOrEmpty(sql))
            {
                _batches.Add(sqlBatch);
            }

            _stringBuilder = new IndentedStringBuilder();

            return this;
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
            => AppendLine(string.Empty, suppressTransaction);

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

        public virtual IDisposable Indent() => _stringBuilder.Indent();
    }
}
