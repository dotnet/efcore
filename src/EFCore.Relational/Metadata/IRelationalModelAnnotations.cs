// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IRelationalModelAnnotations
    {
        ISequence FindSequence([NotNull] string name, [CanBeNull] string schema = null);
        IDbFunction FindDbFunction([NotNull] MethodInfo method);

        IReadOnlyList<ISequence> Sequences { get; }
        IReadOnlyList<IDbFunction> DbFunctions { get; }

        string DefaultSchema { get; }
    }
}
