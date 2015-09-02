// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Internal
{
    public class ModelBuilderContributorExtension : IDbContextOptionsExtension
    {
        private readonly IList<EntityFrameworkServicesBuilderVisitor> _visitors = new List<EntityFrameworkServicesBuilderVisitor>();

        public void ApplyServices(EntityFrameworkServicesBuilder builder)
        {
            foreach(var visitor in _visitors.Distinct())
                visitor.Apply(builder);
        }

        public void AddConvention<T>() where T : class, IModelBuilderConvention
        {
            _visitors.Add(new EntityFrameworkServicesBuilderVisitor<T>());
        }

        public void AddConvention(Type type)
        {
            _visitors.Add(new EntityFrameworkServicesBuilderVisitor(type));
        }

        
    }
}
