// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Entity.Migrations.Utilities
{
    internal static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetNonIndexerProperties(this Type type)
        {
            return type.GetRuntimeProperties().Where(p => p.IsPublic() && !p.GetIndexParameters().Any());
        }
    }
}
