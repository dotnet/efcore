// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IMemberMapper
    {
        IEnumerable<Tuple<IProperty, MemberInfo>> MapPropertiesToMembers([NotNull] IEntityType entityType);
    }
}
