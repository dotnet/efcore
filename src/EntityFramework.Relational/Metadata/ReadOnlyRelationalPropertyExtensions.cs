// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalPropertyExtensions : IRelationalPropertyExtensions
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        protected const string ColumnTypeAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType;
        protected const string DefaultExpressionAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultExpression;
        protected const string DefaultValueAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValue;
        protected const string DefaultValueTypeAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValueType;

        private readonly IProperty _property;

        public ReadOnlyRelationalPropertyExtensions([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            _property = property;
        }

        public virtual string Column => _property[NameAnnotation] as string ?? _property.Name;
        public virtual string ColumnType => _property[ColumnTypeAnnotation] as string;
        public virtual string DefaultExpression => _property[DefaultExpressionAnnotation] as string;

        public virtual object DefaultValue 
            => new TypedAnnotation(
                _property[DefaultValueTypeAnnotation] as string,
                _property[DefaultValueAnnotation] as string).Value;

        protected virtual IProperty Property => _property;
    }
}
