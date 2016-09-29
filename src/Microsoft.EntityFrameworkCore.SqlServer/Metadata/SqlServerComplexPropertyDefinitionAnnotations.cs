// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerComplexPropertyDefinitionAnnotations : RelationalComplexPropertyDefinitionAnnotations, ISqlServerComplexPropertyDefinitionAnnotations
    {
        public SqlServerComplexPropertyDefinitionAnnotations(
            [NotNull] IComplexPropertyDefinition propertyDefinition)
            : base(propertyDefinition, SqlServerFullAnnotationNames.Instance)
        {
        }

        protected SqlServerComplexPropertyDefinitionAnnotations(
            [NotNull] RelationalAnnotations annotations)
            : base(annotations, SqlServerFullAnnotationNames.Instance)
        {
        }

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategyDefault
        {
            get
            {
                return (SqlServerValueGenerationStrategy?)Annotations.GetAnnotation(
                    SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, null);
            }
            [param: CanBeNull] set { SetValueGenerationStrategyDefault(value); }
        }

        protected virtual bool SetValueGenerationStrategyDefault(SqlServerValueGenerationStrategy? value)
            => Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, null, value);

        public virtual string HiLoSequenceNameDefault
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    SqlServerFullAnnotationNames.Instance.HiLoSequenceName, null);
            }
            [param: CanBeNull] set { SetHiLoSequenceNameDefault(value); }
        }

        protected virtual bool SetHiLoSequenceNameDefault([CanBeNull] string value)
            => Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName, null, value);

        public virtual string HiLoSequenceSchemaDefault
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema, null);
            }
            [param: CanBeNull] set { SetHiLoSequenceSchemaDefault(value); }
        }

        protected virtual bool SetHiLoSequenceSchemaDefault([CanBeNull] string value)
            => Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema, null, value);
    }
}
