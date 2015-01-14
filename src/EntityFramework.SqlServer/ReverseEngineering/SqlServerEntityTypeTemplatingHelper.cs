// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.SqlServer.ReverseEngineering
{
    public class SqlServerEntityTypeTemplatingHelper : EntityTypeTemplatingHelper
    {
        public static readonly string EntityTypeTemplate =
@"@inherits Microsoft.Framework.CodeGeneration.Templating.RazorTemplateBase
@using Microsoft.Data.Entity.Metadata
// Generated using Provider Assembly: @Model.ProviderAssembly
// And Database Connection String: @Model.ConnectionString
// With Database Filters: @Model.Filters

@Model.Helper.Usings()
namespace @Model.Namespace
{
    public class @Model.EntityType.SimpleName
    {
@Model.Helper.PropertiesCode(indent: ""        "")
@Model.Helper.NavigationsCode(indent:  ""        "")
    }
}";
        public SqlServerEntityTypeTemplatingHelper(EntityTypeTemplateModel model) : base(model) { }

        public override string PropertyAttributesCode(string indent, IProperty property)
        {
            var prop = (Property)property;
            if (prop.IsKey())
            {
                string ordinal = string.Empty;
                Annotation primaryKeyOrdinalPositionAnnotation;
                if ((primaryKeyOrdinalPositionAnnotation = prop.TryGetAnnotation("PrimaryKeyOrdinalPosition")) != null)
                {
                    ordinal = "(Ordinal = " + primaryKeyOrdinalPositionAnnotation.Value + ")";
                }
                return indent + "[Key" + ordinal + "]" + Environment.NewLine;
            }
            if (prop.IsForeignKey())
            {
                return indent + "[ForeignKey]" + Environment.NewLine;
            }

            return null;
        }
    }
}