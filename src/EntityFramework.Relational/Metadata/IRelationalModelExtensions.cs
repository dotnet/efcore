// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public interface IRelationalModelExtensions
    {
        Sequence TryGetSequence([NotNull] string name, [CanBeNull] string schema = null);

        IReadOnlyList<Sequence> Sequences { get; }
    }
}
