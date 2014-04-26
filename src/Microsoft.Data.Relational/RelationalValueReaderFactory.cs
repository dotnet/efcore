// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Relational
{
    public abstract class RelationalValueReaderFactory
    {
        public abstract IValueReader Create([NotNull] DbDataReader dataReader);
    }
}
