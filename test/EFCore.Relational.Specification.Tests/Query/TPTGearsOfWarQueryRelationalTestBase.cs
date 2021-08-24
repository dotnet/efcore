// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTGearsOfWarQueryRelationalTestBase<TFixture> : GearsOfWarQueryRelationalTestBase<TFixture>
        where TFixture : TPTGearsOfWarQueryRelationalFixture, new()
    {
        protected TPTGearsOfWarQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override Task Project_discriminator_columns(bool async)
            => Task.CompletedTask;
    }
}
