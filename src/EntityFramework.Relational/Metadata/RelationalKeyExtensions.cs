// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalKeyExtensions : ReadOnlyRelationalKeyExtensions
    {
        public RelationalKeyExtensions([NotNull] Key key)
            : base(key)
        {
        }

        public new virtual string Name
        {
            get { return base.Name; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Key)Key)[NameAnnotation] = value;
            }
        }
    }
}
