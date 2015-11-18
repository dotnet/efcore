// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class SqlServerColumnModel : ColumnModel
    {
        public virtual bool IsIdentity { get; [param: CanBeNull] set; }

        public virtual int? DateTimePrecision { get; [param: CanBeNull] set; }
    }
}