// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ReadOnlyRelationalPropertyAnnotations : IRelationalPropertyAnnotations
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        protected const string ColumnOrderAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnOrder;
        protected const string ColumnTypeAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType;
        protected const string GeneratedValueSqlAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.GeneratedValueSql;
        protected const string DefaultValueAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.DefaultValue;
        protected const string DefaultValueTypeAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValueType;

        private readonly IProperty _property;

        public ReadOnlyRelationalPropertyAnnotations([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            _property = property;
        }

        public virtual string ColumnName => _property[NameAnnotation] as string ?? _property.Name;
        public virtual int? ColumnOrder => _property[ColumnOrderAnnotation] as int?;
        public virtual string ColumnType => _property[ColumnTypeAnnotation] as string;
        public virtual string GeneratedValueSql => _property[GeneratedValueSqlAnnotation] as string;

        public virtual object DefaultValue
            => new TypedAnnotation(
                _property[DefaultValueTypeAnnotation] as string,
                _property[DefaultValueAnnotation] as string).Value;

        protected virtual IProperty Property => _property;
    }
}
