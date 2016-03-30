// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IRelationalCommandBuilder : IInfrastructure<IndentedStringBuilder>
    {
        IRelationalParameterBuilder ParameterBuilder { get; }

        IRelationalCommand Build();
    }
}
