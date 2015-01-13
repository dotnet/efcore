// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    public class ContextTemplatingHelper : BaseTemplatingHelper
    {
        public ContextTemplatingHelper(ContextTemplateModel model) : base(model) { }

        public ContextTemplateModel ContextTemplateModel
        {
            get { return Model as ContextTemplateModel; }
        }

        //public virtual string Usings()
        //{
        //    var entityTypes = ContextTemplateModel.MetadataModel.EntityTypes;
        //    if (entityTypes.Count == 0)
        //    {
        //        return "// No EntityTypes found in model";
        //    }
        //    else
        //    {
        //        var entityTypeNamespaces = entityTypes.Select(et => et.Type.Namespace);

        //        return ConstructUsings(entityTypeNamespaces);
        //    }
        //}

        public virtual string OnConfiguringCode(string indent)
        {
            return string.Empty;
        }

        public virtual string OnModelCreatingCode(string indent)
        {
            return string.Empty;
        }
    }
}