// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerKeyExtensions : ReadOnlyRelationalKeyExtensions, ISqlServerKeyExtensions
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqlServerKeyExtensions([NotNull] IKey key)
            : base(key)
        {
        }

        public override string Name
        {
            get { return Key[SqlServerNameAnnotation] ?? base.Name; }
        }
    }
}
