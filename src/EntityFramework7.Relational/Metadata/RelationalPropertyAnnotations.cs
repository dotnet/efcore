// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalPropertyAnnotations : ReadOnlyRelationalPropertyAnnotations
    {
        public RelationalPropertyAnnotations([NotNull] Property property)
            : base(property)
        {
        }

        public new virtual string Column
        {
            get { return base.Column; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[NameAnnotation] = value;
            }
        }

        public new virtual int? ColumnOrder
        {
            get { return base.ColumnOrder; }
            [param: CanBeNull]
            set
            {
                ((Property)Property)[ColumnOrderAnnotation] = value;
            }
        }

        public new virtual string ColumnType
        {
            get { return base.ColumnType; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[ColumnTypeAnnotation] = value;
            }
        }

        public new virtual string DefaultValueSql
        {
            get { return base.DefaultValueSql; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

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
