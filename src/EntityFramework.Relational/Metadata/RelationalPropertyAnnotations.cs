// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalPropertyAnnotations : IRelationalPropertyAnnotations
    {
        public RelationalPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(property, providerPrefix))
        {
        }
        
        protected RelationalPropertyAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected RelationalAnnotations Annotations { get; }

        protected virtual IProperty Property => (IProperty)Annotations.Metadata;

        public virtual string ColumnName
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.ColumnName) ?? Property.Name; }
            [param: CanBeNull] set { SetColumnName(value); }
        }

        protected bool SetColumnName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.ColumnName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ColumnType
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.ColumnType); }
            [param: CanBeNull] set { SetColumnType(value); }
        }

        protected bool SetColumnType([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.ColumnType, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string GeneratedValueSql
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.GeneratedValueSql); }
            [param: CanBeNull] set { SetGeneratedValueSql(value); }
        }

        protected bool SetGeneratedValueSql([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.GeneratedValueSql, Check.NullButNotEmpty(value, nameof(value)));

        public virtual object DefaultValue
        {
            get
            {
                return new TypedAnnotation(
                    (string)Annotations.GetAnnotation(RelationalAnnotationNames.DefaultValueType),
                    (string)Annotations.GetAnnotation(RelationalAnnotationNames.DefaultValue)).Value;
            }
            [param: CanBeNull]
            set { SetDefaultValue(value); }
        }

        protected bool SetDefaultValue([CanBeNull] object value)
        {
            var typedAnnotation = new TypedAnnotation(value);
            return Annotations.SetAnnotation(RelationalAnnotationNames.DefaultValueType, typedAnnotation.TypeString) &&
                   Annotations.SetAnnotation(RelationalAnnotationNames.DefaultValue, typedAnnotation.ValueString);
        }
    }
}
