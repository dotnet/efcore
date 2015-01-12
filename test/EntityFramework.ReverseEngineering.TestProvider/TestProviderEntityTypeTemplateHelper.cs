// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.ReverseEngineering.TestProvider
{
    public class TestProviderEntityTypeTemplateHelper : EntityTypeTemplatingHelper
    {
        public TestProviderEntityTypeTemplateHelper(EntityTypeTemplateModel model) : base(model) { }

        public override string PropertyAttributesCode(string indent, IProperty property)
        {
            if (property.IsKey())
            {
                return indent + "[Key]" + Environment.NewLine;
            }
            if (property.IsForeignKey())
            {
                return indent + "[ForeignKey]" + Environment.NewLine;
            }

            return null;
        }
    }
}