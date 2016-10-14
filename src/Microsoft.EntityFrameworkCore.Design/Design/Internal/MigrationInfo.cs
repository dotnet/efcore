// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class MigrationInfo
    {
        public virtual string Id { get; [param: NotNull] set; }
        public virtual string Name { get; [param: NotNull] set; }
    }
}
