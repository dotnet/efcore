// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IRelationalModelAnnotations
    {
        ISequence FindSequence([NotNull] string name, [CanBeNull] string schema = null);

        IReadOnlyList<ISequence> Sequences { get; }

        string DefaultSchema { get; }
        string DatabaseName { get; }
    }
}
