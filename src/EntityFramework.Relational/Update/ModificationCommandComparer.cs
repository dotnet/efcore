// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommandComparer : IComparer<ModificationCommand>
    {
        public virtual int Compare(ModificationCommand x, ModificationCommand y)
        {
            var result = 0;
            if (ReferenceEquals(x, y))
            {
                return result;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            result = StringComparer.Ordinal.Compare(x.SchemaQualifiedName.Schema, y.SchemaQualifiedName.Schema);
            if (0 != result)
            {
                return result;
            }

            result = StringComparer.Ordinal.Compare(x.SchemaQualifiedName.Name, y.SchemaQualifiedName.Name);
            if (0 != result)
            {
                return result;
            }

            result = (int)x.EntityState - (int)y.EntityState;
            if (0 != result)
            {
                return result;
            }

            return result;
        }
    }
}
