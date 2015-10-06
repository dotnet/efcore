// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;

namespace Microsoft.Data.Entity.TestUtilities.FakeProvider
{
    public class FakeDbTransaction : DbTransaction
    {
        public FakeDbTransaction(FakeDbConnection connection)
        {
            DbConnection = connection;
        }

        protected override DbConnection DbConnection { get; }

        public override IsolationLevel IsolationLevel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Commit()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}
