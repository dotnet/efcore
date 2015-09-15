// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineerFiles
    {
        public virtual string ContextFile { get;[param: NotNull] set; }
        public virtual IList<string> EntityTypeFiles { get; } = new List<string>();
    }
}
