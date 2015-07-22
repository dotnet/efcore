// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalPropertyAnnotations : RelationalAnnotationsBase, IRelationalPropertyAnnotations
    {
        public RelationalPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix)
            : base(property, providerPrefix)
        {
        }

        protected virtual IProperty Property => (IProperty)Metadata;

        public virtual string ColumnName
        {
            get { return (string)GetAnnotation(RelationalAnnotationNames.ColumnName) ?? Property.Name; }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.ColumnName, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual string ColumnType
        {
            get { return (string)GetAnnotation(RelationalAnnotationNames.ColumnType); }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.ColumnType, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual string GeneratedValueSql
        {
            get { return (string)GetAnnotation(RelationalAnnotationNames.GeneratedValueSql); }
            [param: CanBeNull] set { SetAnnotation(RelationalAnnotationNames.GeneratedValueSql, Check.NullButNotEmpty(value, nameof(value))); }
        }

        public virtual object DefaultValue
        {
            get
            {
                return new TypedAnnotation(
                    (string)GetAnnotation(RelationalAnnotationNames.DefaultValueType),
                    (string)GetAnnotation(RelationalAnnotationNames.DefaultValue)).Value;
            }
            [param: CanBeNull]
            set
            {
                var typedAnnotation = new TypedAnnotation(value);
                SetAnnotation(RelationalAnnotationNames.DefaultValueType, typedAnnotation.TypeString);
                SetAnnotation(RelationalAnnotationNames.DefaultValue, typedAnnotation.ValueString);
            }
        }
    }
}
