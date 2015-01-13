// Copyright(c) Microsoft Open Technologies, Inc.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Extensions
{
    public class RelationalDbContextOptions
    {
        private readonly DbContextOptions _options;

        public RelationalDbContextOptions([NotNull] DbContextOptions options)
        {
            Check.NotNull(options, "options");

            _options = options;
        }

        protected DbContextOptions Options
        {
            get { return _options; }
        }
    }
}
