// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;

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

        public static string UniqueSortedList(IEnumerable<string> objects, string pre, string post)
        {
            var sortedSet = new SortedSet<string>();
            foreach (var obj in objects)
            {
                sortedSet.Add(obj);
            }

            if (sortedSet.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var item in sortedSet)
            {
                sb.Append(pre);
                sb.Append(item);
                sb.Append(post);
            }

            return sb.ToString();
        }

    }
}