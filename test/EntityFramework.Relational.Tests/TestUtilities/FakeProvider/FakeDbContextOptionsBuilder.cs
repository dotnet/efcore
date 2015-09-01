// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.TestUtilities.FakeProvider
{
    public class FakeDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<FakeDbContextOptionsBuilder, FakeRelationalOptionsExtension>
    {
        public FakeDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        protected override FakeRelationalOptionsExtension CloneExtension()
            => new FakeRelationalOptionsExtension(OptionsBuilder.Options.GetExtension<FakeRelationalOptionsExtension>());
    }
}
