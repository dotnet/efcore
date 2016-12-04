// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerValueGenerationStrategyConvention : DatabaseGeneratedAttributeConvention, IModelConvention
    {
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder, DatabaseGeneratedAttribute attribute, MemberInfo clrMember)
        {
            propertyBuilder.SqlServer(ConfigurationSource.DataAnnotation).ValueGenerationStrategy(
                attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity
                    ? SqlServerValueGenerationStrategy.IdentityColumn
                    : (SqlServerValueGenerationStrategy?)null);

            return base.Apply(propertyBuilder, attribute, clrMember);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            modelBuilder.SqlServer(ConfigurationSource.Convention).ValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);

            return modelBuilder;
        }
    }
}
