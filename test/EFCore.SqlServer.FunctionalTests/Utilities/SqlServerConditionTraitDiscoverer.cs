// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    public class SqlServerConditionTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var sqlServerCondition = (traitAttribute as IReflectionAttributeInfo)?.Attribute as SqlServerConditionAttribute;
            if (sqlServerCondition == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
            return Enum.GetValues(typeof(SqlServerCondition)).Cast<SqlServerCondition>()
                .Where(c => sqlServerCondition.Conditions.HasFlag(c))
                .Select(c => new KeyValuePair<string, string>(nameof(SqlServerCondition), c.ToString()));
        }
    }
}
