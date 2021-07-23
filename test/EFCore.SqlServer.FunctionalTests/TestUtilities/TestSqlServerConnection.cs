// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestSqlServerConnection : SqlServerConnection
    {
        public TestSqlServerConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        public int ErrorNumber { get; set; } = -2;
        public Queue<bool?> OpenFailures { get; } = new();
        public int OpenCount { get; set; }
        public Queue<bool?> CommitFailures { get; } = new();
        public Queue<bool?> ExecutionFailures { get; } = new();
        public int ExecutionCount { get; set; }

        public override bool Open(bool errorsExpected = false)
        {
            PreOpen();

            return base.Open(errorsExpected);
        }

        public override Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            PreOpen();

            return base.OpenAsync(cancellationToken, errorsExpected);
        }

        private void PreOpen()
        {
            if (DbConnection.State == ConnectionState.Open)
            {
                return;
            }

            OpenCount++;
            if (OpenFailures.Count <= 0)
            {
                return;
            }

            var fail = OpenFailures.Dequeue();

            if (fail.HasValue)
            {
                throw SqlExceptionFactory.CreateSqlException(ErrorNumber);
            }
        }
    }
}
