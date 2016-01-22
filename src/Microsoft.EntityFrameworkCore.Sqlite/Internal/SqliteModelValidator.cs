// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class SqliteModelValidator : RelationalModelValidator
    {
        public SqliteModelValidator([NotNull] ILogger<RelationalModelValidator> loggerFactory, [NotNull] IRelationalAnnotationProvider relationalExtensions)
            : base(loggerFactory, relationalExtensions)
        {
        }
    }
}
