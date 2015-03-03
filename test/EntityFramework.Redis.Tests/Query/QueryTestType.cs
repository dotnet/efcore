// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Redis.Tests.Query
{
    public class QueryTestType
    {
        public int Id { get; set; }
        public string SomeValue { get; set; }

        public static EntityType EntityType()
        {
            return Model().EntityTypes.Single();
        }

        public static Model Model()
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(QueryTestType));
            foreach (var property in typeof(QueryTestType).GetProperties())
            {
                entityType.GetOrAddProperty(property);
            }

            return model;
        }
    }
}
