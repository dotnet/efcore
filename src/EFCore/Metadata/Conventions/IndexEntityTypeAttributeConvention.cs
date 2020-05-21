// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures database indexes based on the <see cref="IndexAttribute" />.
    /// </summary>
    public class IndexEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<IndexAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IndexEntityTypeAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public IndexEntityTypeAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc/>
        protected override void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IndexAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            var ignoredMembers = entityType.GetIgnoredMembers();
            var indexProperties = new List<IConventionProperty>();
            foreach (var memberName in attribute.MemberNames)
            {
                if (string.IsNullOrWhiteSpace(memberName))
                {
                    throw new InvalidOperationException(
                        CoreStrings.IndexMemberNameEmpty(
                            entityType.DisplayName(),
                            Format(attribute.MemberNames)));
                }

                if (ignoredMembers.Contains(memberName))
                {
                    throw new InvalidOperationException(
                        CoreStrings.IndexMemberIsIgnored(
                            entityType.DisplayName(),
                            Format(attribute.MemberNames),
                            memberName));
                }

                var member = entityType.FindProperty(memberName);
                if (member == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IndexMemberHasNoMatchingMember(
                            entityType.DisplayName(),
                            Format(attribute.MemberNames),
                            memberName));
                }

                indexProperties.Add(member);
            }

            var indexBuilder = entityTypeBuilder.HasIndex(indexProperties, fromDataAnnotation: true);
            indexBuilder.HasName(attribute.Name, fromDataAnnotation: true);
            indexBuilder.IsUnique(attribute.IsUnique, fromDataAnnotation: true);
        }

        private static string Format(string[] memberNames)
            => "{" + string.Join(", ", memberNames.Select(s => "'" + s + "'")) + "}";
    }
}
