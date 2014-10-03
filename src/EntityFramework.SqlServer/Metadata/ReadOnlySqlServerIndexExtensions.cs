// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerIndexExtensions : ReadOnlyRelationalIndexExtensions, ISqlServerIndexExtensions
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public ReadOnlySqlServerIndexExtensions([NotNull] IIndex index)
            : base(index)
        {
        }

        public override string Name
        {
            get { return Index[SqlServerNameAnnotation] ?? base.Name; }
        }
    }
}
