// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class AttributeConfiguration : IAttributeConfiguration
    {
        public AttributeConfiguration(
            [NotNull] string attributeName, [CanBeNull] params string[] attributeArguments)
        {
            Check.NotEmpty(attributeName, nameof(attributeName));

            AttributeBody =
                attributeArguments == null || attributeArguments.Length == 0
                ? StripAttribute(attributeName)
                : StripAttribute(attributeName) + "(" + string.Join(", ", attributeArguments) + ")";
        }

        public virtual string AttributeBody { get; }

        protected static string StripAttribute([NotNull] string attributeName)
        {
            return attributeName.EndsWith("Attribute", StringComparison.Ordinal)
                ? attributeName.Substring(0, attributeName.Length - 9)
                : attributeName;
        }
    }
}
