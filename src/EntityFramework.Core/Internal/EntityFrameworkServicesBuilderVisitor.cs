// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Extensions;

namespace Microsoft.Data.Entity.Internal
{
    public class EntityFrameworkServicesBuilderVisitor<T> : EntityFrameworkServicesBuilderVisitor where T : class, IModelBuilderConvention
    {
        public override void Apply(EntityFrameworkServicesBuilder builder)
        {
            ((IAccessor<IServiceCollection>) builder).Service.TryAddScoped<IModelBuilderConvention, T>();
        }
    }

    public class EntityFrameworkServicesBuilderVisitor
    {
        public EntityFrameworkServicesBuilderVisitor()
        {
        }

        private readonly Type _type;
        private readonly IModelBuilderConvention _instance;

        public EntityFrameworkServicesBuilderVisitor(Type type)
        {
            _type = type;
        }

        public EntityFrameworkServicesBuilderVisitor(IModelBuilderConvention instance)
        {
            _instance = instance;
        }

        public virtual void Apply(EntityFrameworkServicesBuilder builder)
        {
            var service = ((IAccessor<IServiceCollection>) builder).Service;
            if (_type != null)
                service.TryAddScoped(typeof (IModelBuilderConvention), _type);
            if(_instance != null)
                service.AddInstance(typeof(IModelBuilderConvention), _instance);
        }
    }

}