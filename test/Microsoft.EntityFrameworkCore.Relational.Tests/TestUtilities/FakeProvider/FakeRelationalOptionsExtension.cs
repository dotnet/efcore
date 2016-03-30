// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeRelationalOptionsExtension : RelationalOptionsExtension
    {
        public FakeRelationalOptionsExtension()
        {
        }

        public FakeRelationalOptionsExtension(FakeRelationalOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        public override void ApplyServices(IServiceCollection bservices)
        {
            throw new NotImplementedException();
        }
    }
}
