// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalPropertyExtensions : ReadOnlyRelationalPropertyExtensions
    {
        public RelationalPropertyExtensions([NotNull] Property property)
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

                ((Property)Property)[NameAnnotation] = value;
            }
        }

        public new virtual string ColumnType
        {
            get { return base.ColumnType; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[ColumnTypeAnnotation] = value;
            }
        }

        public new virtual string DefaultExpression
        {
            get { return base.DefaultExpression; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[DefaultExpressionAnnotation] = value;
            }
        }

        public new virtual object DefaultValue
        {
            get { return base.DefaultValue; }
            [param: CanBeNull]
            set
            {
                var typedAnnotation = new TypedAnnotation(value);

                ((Property)Property)[DefaultValueTypeAnnotation] = typedAnnotation.TypeString;
                ((Property)Property)[DefaultValueAnnotation] = typedAnnotation.ValueString;
            }
        }
    }
}
