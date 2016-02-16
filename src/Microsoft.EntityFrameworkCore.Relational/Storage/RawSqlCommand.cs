// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RawSqlCommand
    {
        public RawSqlCommand(
            [NotNull] IRelationalCommand relationalCommand,
            [NotNull] IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(relationalCommand, nameof(relationalCommand));
            Check.NotNull(parameterValues, nameof(parameterValues));

            RelationalCommand = relationalCommand;
            ParameterValues = parameterValues;
        }

        public virtual IRelationalCommand RelationalCommand { get; }
        public virtual IReadOnlyDictionary<string, object> ParameterValues { get; }
    }
}
