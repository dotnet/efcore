// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IFieldMatcher
    {
        FieldInfo TryMatchFieldName(
            [NotNull] IProperty property, [NotNull] PropertyInfo propertyInfo, [NotNull] Dictionary<string, FieldInfo> dclaredFields);
    }
}
