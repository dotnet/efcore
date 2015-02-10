// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ModelUtilities
    {
        private static readonly ModelUtilities _instance = new ModelUtilities();

        public static ModelUtilities Instance
        {
            get
            {
                return _instance;
            }
        }

        public string GenerateLambdaToKey(
            [NotNull]IEnumerable<IProperty> properties,
            [NotNull]string lambdaIdentifier)
        {
            var sb = new StringBuilder();

            if (properties.Count() > 1)
            {
                sb.Append("new object[] { ");
                sb.Append(string.Join(", ", properties.Select(p => lambdaIdentifier + "." + p.Name)));
                sb.Append(" }");
            }
            else
            {
                sb.Append(lambdaIdentifier + "." + properties.ElementAt(0).Name);
            }

            return sb.ToString();
        }

    }
}