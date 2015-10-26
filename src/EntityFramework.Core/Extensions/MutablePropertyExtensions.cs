// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class MutablePropertyExtensions
    {
        public static void SetMaxLength([NotNull] this IMutableProperty property, int? maxLength)
        {
            Check.NotNull(property, nameof(property));

            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            property[CoreAnnotationNames.MaxLengthAnnotation] = maxLength;
        }

        public static IEnumerable<IMutableForeignKey> FindContainingForeignKeys([NotNull] this IMutableProperty property)
            => ((IProperty)property).FindContainingForeignKeys().Cast<IMutableForeignKey>();

        public static IMutableKey FindContainingPrimaryKey([NotNull] this IMutableProperty property)
            => (IMutableKey)((IProperty)property).FindContainingPrimaryKey();

        public static IEnumerable<IMutableKey> FindContainingKeys([NotNull] this IMutableProperty property)
            => ((IProperty)property).FindContainingKeys().Cast<IMutableKey>();
    }
}
