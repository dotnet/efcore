// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public interface ISqlQueryGenerator
    {
        string GenerateSql([NotNull] IDictionary<string, object> parameterValues);

        IReadOnlyList<CommandParameter> Parameters { get; }
    }
}
