// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IRelationalModelAnnotations
    {
        Sequence TryGetSequence([NotNull] string name, [CanBeNull] string schema = null);

        IReadOnlyList<Sequence> Sequences { get; }
    }
}
