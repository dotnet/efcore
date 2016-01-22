// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class ForeignKeyColumnModel : Annotatable
    {
        public virtual int Ordinal { get; [param: NotNull] set; }
        public virtual ColumnModel Column { get; [param: NotNull] set; }
        public virtual ColumnModel PrincipalColumn { get; [param: CanBeNull] set; }
    }
}
