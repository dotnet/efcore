// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    internal class DatabaseColumnRef : DatabaseColumn
    {
        public DatabaseColumnRef(string name)
        {
            Name = name;
        }

        public override DatabaseTable Table
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override bool IsNullable
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override string StoreType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override string DefaultValueSql
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override string ComputedColumnSql
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override string Comment
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override ValueGenerated? ValueGenerated
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
