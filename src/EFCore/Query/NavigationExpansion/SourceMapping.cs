// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class SourceMapping
    {
        public virtual IEntityType RootEntityType { get; set; }

        public virtual NavigationTreeNode NavigationTree { get; set; }
    }
}
