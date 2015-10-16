// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata
{
    public class RelationalDesignEntityTypeAnnotations : RelationalEntityTypeAnnotations
    {
        public RelationalDesignEntityTypeAnnotations([NotNull] IEntityType entityType, [CanBeNull] string providerPrefix)
            : base(entityType, providerPrefix)
        {
        }

        public virtual string EntityTypeError
        {
            get { return (string)Annotations.GetAnnotation(RelationalDesignAnnotationNames.EntityTypeError); }
            [param: CanBeNull] set { SetEntityTypeError(value); }
        }

        protected virtual bool SetEntityTypeError([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalDesignAnnotationNames.EntityTypeError, Check.NullButNotEmpty(value, nameof(value)));
    }
}
