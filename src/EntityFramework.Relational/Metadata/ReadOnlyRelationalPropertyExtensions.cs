// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class ReadOnlyRelationalPropertyExtensions : IRelationalPropertyExtensions
    {
        protected const string NameAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        protected const string ColumnTypeAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType;
        protected const string DefaultExpressionAnnotation = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultExpression;

        private readonly IProperty _property;

        public ReadOnlyRelationalPropertyExtensions([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            _property = property;
        }

        public virtual string Column
        {
            get { return _property[NameAnnotation] ?? _property.Name; }
        }

        public virtual string ColumnType
        {
            get { return _property[ColumnTypeAnnotation]; }
        }

        public virtual string DefaultExpression
        {
            get { return _property[DefaultExpressionAnnotation]; }
        }

        protected virtual IProperty Property
        {
            get { return _property; }
        }
    }
}
