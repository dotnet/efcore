// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqliteMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if ((property.ValueGenerated == ValueGenerated.OnAdd)
                && property.ClrType.UnwrapNullableType().IsInteger())
            {
                yield return new Annotation(SqliteFullAnnotationNames.Instance.Autoincrement, true);
            }
        }
    }
}
