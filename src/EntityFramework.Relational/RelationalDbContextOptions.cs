// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalDbContextOptions
    {
        public RelationalDbContextOptions([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, "options");

            Options = options;
        }

        protected DbContextOptions Options { get; }
    }
}
