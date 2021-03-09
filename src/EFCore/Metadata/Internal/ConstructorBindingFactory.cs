// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using CA = System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ConstructorBindingFactory : IConstructorBindingFactory
    {
        private readonly IPropertyParameterBindingFactory _propertyFactory;
        private readonly IParameterBindingFactories _factories;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ConstructorBindingFactory(
            [NotNull] IPropertyParameterBindingFactory propertyFactory,
            [NotNull] IParameterBindingFactories factories)
        {
            _propertyFactory = propertyFactory;
            _factories = factories;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void GetBindings(
            IConventionEntityType entityType,
            out InstantiationBinding constructorBinding,
            out InstantiationBinding? serviceOnlyBinding)
            => GetBindings(
                entityType,
                static (f, e, p, n) => f?.Bind((IConventionEntityType)e, p, n),
                out constructorBinding,
                out serviceOnlyBinding);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void GetBindings(
            IMutableEntityType entityType,
            out InstantiationBinding constructorBinding,
            out InstantiationBinding? serviceOnlyBinding)
            => GetBindings(
                entityType,
                static (f, e, p, n) => f?.Bind((IMutableEntityType)e, p, n),
                out constructorBinding,
                out serviceOnlyBinding);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void GetBindings(
            IReadOnlyEntityType entityType,
            out InstantiationBinding constructorBinding,
            out InstantiationBinding? serviceOnlyBinding)
            => GetBindings(
                entityType,
                static (f, e, p, n) => f?.Bind(e, p, n),
                out constructorBinding,
                out serviceOnlyBinding);

        private void GetBindings(
            IReadOnlyEntityType entityType,
            Func<IParameterBindingFactory?, IReadOnlyEntityType, Type, string, ParameterBinding?> bind,
            out InstantiationBinding constructorBinding,
            out InstantiationBinding? serviceOnlyBinding)
        {
            var maxServiceParams = 0;
            var maxServiceOnlyParams = 0;
            var minPropertyParams = int.MaxValue;
            var foundBindings = new List<InstantiationBinding>();
            var foundServiceOnlyBindings = new List<InstantiationBinding>();
            var bindingFailures = new List<IEnumerable<ParameterInfo>>();

            foreach (var constructor in entityType.ClrType.GetTypeInfo()
                .DeclaredConstructors
                .Where(c => !c.IsStatic))
            {
                // Trying to find the constructor with the most service properties
                // followed by the least scalar property parameters
                if (TryBindConstructor(
                    entityType, constructor, bind, out var binding, out var failures))
                {
                    var serviceParamCount = binding.ParameterBindings.OfType<ServiceParameterBinding>().Count();
                    var propertyParamCount = binding.ParameterBindings.Count - serviceParamCount;

                    if (propertyParamCount == 0)
                    {
                        if (serviceParamCount == maxServiceOnlyParams)
                        {
                            foundServiceOnlyBindings.Add(binding);
                        }
                        else if (serviceParamCount > maxServiceOnlyParams)
                        {
                            foundServiceOnlyBindings.Clear();
                            foundServiceOnlyBindings.Add(binding);

                            maxServiceOnlyParams = serviceParamCount;
                        }
                    }

                    if (serviceParamCount == maxServiceParams
                        && propertyParamCount == minPropertyParams)
                    {
                        foundBindings.Add(binding);
                    }
                    else if (serviceParamCount > maxServiceParams)
                    {
                        foundBindings.Clear();
                        foundBindings.Add(binding);

                        maxServiceParams = serviceParamCount;
                        minPropertyParams = propertyParamCount;
                    }
                    else if (propertyParamCount < minPropertyParams)
                    {
                        foundBindings.Clear();
                        foundBindings.Add(binding);

                        maxServiceParams = serviceParamCount;
                        minPropertyParams = propertyParamCount;
                    }
                }
                else
                {
                    bindingFailures.Add(failures);
                }
            }

            if (foundBindings.Count == 0)
            {
                var constructorErrors = bindingFailures.SelectMany(f => f)
                    .GroupBy(f => (ConstructorInfo)f.Member)
                    .Select(
                        x => CoreStrings.ConstructorBindingFailed(
                            string.Join("', '", x.Select(f => f.Name)),
                            entityType.DisplayName()
                            + "("
                            + string.Join(
                                ", ", x.Key.GetParameters().Select(
                                    y => y.ParameterType.ShortDisplayName() + " " + y.Name)
                            )
                            + ")"
                        )
                    );

                throw new InvalidOperationException(
                    CoreStrings.ConstructorNotFound(
                        entityType.DisplayName(),
                        string.Join("; ", constructorErrors)));
            }

            if (foundBindings.Count > 1)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConstructorConflict(
                        FormatConstructorString(entityType, foundBindings[0]),
                        FormatConstructorString(entityType, foundBindings[1])));
            }

            constructorBinding = foundBindings[0];
            if (foundServiceOnlyBindings.Count == 1)
            {
                serviceOnlyBinding = foundServiceOnlyBindings[0];
            }
            else
            {
                serviceOnlyBinding = null;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryBindConstructor(
            IMutableEntityType entityType,
            ConstructorInfo constructor,
            [CA.NotNullWhen(true)] out InstantiationBinding? binding,
            [CA.NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters)
            => TryBindConstructor(
                entityType,
                constructor,
                static (f, e, p, n) => f?.Bind((IMutableEntityType)e, p, n),
                out binding,
                out unboundParameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool TryBindConstructor(
            IConventionEntityType entityType,
            ConstructorInfo constructor,
            [CA.NotNullWhen(true)] out InstantiationBinding? binding,
            [CA.NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters)
            => TryBindConstructor(
                entityType,
                constructor,
                static (f, e, p, n) => f?.Bind((IConventionEntityType)e, p, n),
                out binding,
                out unboundParameters);

        private bool TryBindConstructor(
            IReadOnlyEntityType entityType,
            ConstructorInfo constructor,
            Func<IParameterBindingFactory?, IReadOnlyEntityType, Type, string, ParameterBinding?> bind,
            [CA.NotNullWhen(true)] out InstantiationBinding? binding,
            [CA.NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters)
        {
            IEnumerable<(ParameterInfo Parameter, ParameterBinding? Binding)> bindings
                = constructor.GetParameters().Select(
                        p => (p, string.IsNullOrEmpty(p.Name)
                            ? null
                            : _propertyFactory.FindParameter((IEntityType)entityType, p.ParameterType, p.Name)
                            ?? bind(_factories.FindFactory(p.ParameterType, p.Name), entityType, p.ParameterType, p.Name)))
                    .ToList();

            if (bindings.Any(b => b.Binding == null))
            {
                unboundParameters = bindings.Where(b => b.Binding == null).Select(b => b.Parameter);
                binding = null;

                return false;
            }

            unboundParameters = null;
            binding = new ConstructorBinding(constructor, bindings.Select(b => b.Binding).ToList()!);

            return true;
        }

        private static string FormatConstructorString(IReadOnlyEntityType entityType, InstantiationBinding binding)
            => entityType.ClrType.ShortDisplayName()
                + "("
                + string.Join(", ", binding.ParameterBindings.Select(b => b.ParameterType.ShortDisplayName()))
                + ")";
    }
}
