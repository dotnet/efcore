// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalKeyAnnotations : ReadOnlyRelationalKeyAnnotations
    {
        public RelationalKeyAnnotations([NotNull] Key key)
            : base(key)
        {
        }

        public new virtual string Name
        {
            get { return base.Name; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Key)Key)[NameAnnotation] = value;
            }
        }
    }
}
