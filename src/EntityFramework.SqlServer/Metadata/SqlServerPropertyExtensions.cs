// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerPropertyExtensions : ReadOnlySqlServerPropertyExtensions
    {
        public SqlServerPropertyExtensions([NotNull] Property property)
            : base(property)
        {
        }

        public new virtual string Column
        {
            get { return base.Column; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerNameAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string ColumnType
        {
            get { return base.ColumnType; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerColumnTypeAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string DefaultExpression
        {
            get { return base.DefaultExpression; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerDefaultExpressionAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string SequenceName
        {
            get { return base.SequenceName; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerSequenceNameAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get { return base.ValueGenerationStrategy; }
            [param: CanBeNull]
            set
            {
                // TODO: Issue #777: Non-string annotations
                ((Property)Property)[SqlServerValueGenerationAnnotation] = value == null ? null : value.ToString();
            }
        }
    }
}
