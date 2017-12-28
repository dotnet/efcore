// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConstructorBindingConvention : IModelBuiltConvention
    {
        private readonly IConstructorBindingFactory _bindingFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ConstructorBindingConvention([NotNull] IConstructorBindingFactory bindingFactory)
            => _bindingFactory = bindingFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.ClrType != null
                    && !entityType.ClrType.IsAbstract)
                {
                    var foundBinding = (ConstructorBinding)null;
                    var bindingFailures = new List<IEnumerable<ParameterInfo>>();

                    foreach (var constructor in entityType.ClrType.GetTypeInfo()
                        .DeclaredConstructors
                        .Where(c => !c.IsStatic)
                        .OrderByDescending(c => c.GetParameters().Length))
                    {
                        var parameterCount = constructor.GetParameters().Length;

                        if (foundBinding != null
                            && foundBinding.ParameterBindings.Count != parameterCount)
                        {
                            break;
                        }

                        if (_bindingFactory.TryBindConstructor(entityType, constructor, out var binding, out var failures))
                        {
                            if (foundBinding?.ParameterBindings.Count == parameterCount)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.ConstructorConflict(
                                        FormatConstructorString(entityType, foundBinding),
                                        FormatConstructorString(entityType, binding)));
                            }

                            foundBinding = binding;
                        }

                        bindingFailures.Add(failures);
                    }

                    if (foundBinding == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConstructorNotFound(
                                entityType.DisplayName(),
                                string.Join("', '", bindingFailures.SelectMany(f => f).Select(f => f.Name))));
                    }

                    entityType.Builder.HasAnnotation(
                        CoreAnnotationNames.ConstructorBinding,
                        foundBinding,
                        ConfigurationSource.Convention);
                }
            }

            return modelBuilder;
        }

        private static string FormatConstructorString(EntityType entityType, ConstructorBinding binding)
            => entityType.DisplayName() + "(" + string.Join(", ", binding.ParameterBindings.Select(b => b.ParameterType.ShortDisplayName())) + ")";
    }
}
