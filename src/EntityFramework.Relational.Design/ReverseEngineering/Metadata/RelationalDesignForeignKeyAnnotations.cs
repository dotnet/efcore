// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata
{
    public class RelationalDesignForeignKeyAnnotations : RelationalForeignKeyAnnotations
    {
        public RelationalDesignForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [CanBeNull] string providerPrefix)
            : base(foreignKey, providerPrefix)
        {
        }

        public virtual string DependentEndNavPropName
        {
            get { return (string)Annotations.GetAnnotation(RelationalDesignAnnotationNames.DependentEndNavPropName); }
            [param: CanBeNull] set { SetDependentEndNavPropName(value); }
        }

        protected virtual bool SetDependentEndNavPropName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalDesignAnnotationNames.DependentEndNavPropName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string PrincipalEndNavPropName
        {
            get { return (string)Annotations.GetAnnotation(RelationalDesignAnnotationNames.PrincipalEndNavPropName); }
            [param: CanBeNull] set { SetPrincipalEndNavPropName(value); }
        }

        protected virtual bool SetPrincipalEndNavPropName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalDesignAnnotationNames.PrincipalEndNavPropName, Check.NullButNotEmpty(value, nameof(value)));
    }
}
