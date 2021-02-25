// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    internal class DatabaseTableRef : DatabaseTable
    {
        public DatabaseTableRef(string name, string schema = null)
        {
            Name = name;
            Schema = schema;
        }

        public override DatabaseModel Database
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override string Comment
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DatabasePrimaryKey PrimaryKey
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override IList<DatabaseColumn> Columns
            => throw new NotImplementedException();

        public override IList<DatabaseUniqueConstraint> UniqueConstraints
            => throw new NotImplementedException();

        public override IList<DatabaseIndex> Indexes
            => throw new NotImplementedException();

        public override IList<DatabaseForeignKey> ForeignKeys
            => throw new NotImplementedException();
    }
}
