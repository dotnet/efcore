// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    /// <summary>
    /// Helper class to generate useful information for templating
    /// </summary>
    public abstract class BaseTemplatingHelper
    {
        private readonly BaseTemplateModel _model;

        public BaseTemplatingHelper(BaseTemplateModel model)
        {
            _model = model;
        }

        public BaseTemplateModel Model
        {
            get { return _model; }
        }

        public static string ConstructUsings(IEnumerable<string> namespaces)
        {
            return namespaces == null || namespaces.Count() == 0
                ? string.Empty
                : string.Join("", namespaces.Distinct().OrderBy(s => s).Select(s => "using " + s + ";" + Environment.NewLine));
        }
    }
}