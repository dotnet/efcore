// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class RelationalParameterBuilder : IRelationalParameterBuilder
    {
        private readonly List<IRelationalParameter> _parameters = new List<IRelationalParameter>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalParameterBuilder(
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            TypeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IRelationalParameter> Parameters => _parameters;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IRelationalTypeMappingSource TypeMappingSource { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddParameter(string invariantName, string name)
            => _parameters.Add(
                new DynamicRelationalParameter(
                    Check.NotEmpty(invariantName, nameof(invariantName)),
                    Check.NotEmpty(name, nameof(name)),
                    TypeMappingSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddParameter(string invariantName, string name, RelationalTypeMapping typeMapping, bool nullable)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(typeMapping, nameof(typeMapping));

            _parameters.Add(
                new TypeMappedRelationalParameter(
                    invariantName,
                    name,
                    typeMapping,
                    nullable));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddParameter(string invariantName, string name, IProperty property)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            _parameters.Add(
                new TypeMappedRelationalParameter(
                    invariantName,
                    name,
                    property.FindRelationalMapping(),
                    property.IsNullable));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddCompositeParameter(string invariantName, Action<IRelationalParameterBuilder> buildAction)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(buildAction, nameof(buildAction));

            var parameterBuilder = Create();

            buildAction(parameterBuilder);

            if (parameterBuilder.Parameters.Count > 0)
            {
                _parameters.Add(
                    new CompositeRelationalParameter(
                        invariantName,
                        parameterBuilder.Parameters));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual RelationalParameterBuilder Create()
            => new RelationalParameterBuilder(TypeMappingSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddPropertyParameter(string invariantName, string name, IProperty property)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            _parameters.Add(
                new TypeMappedPropertyRelationalParameter(
                    invariantName,
                    name,
                    property.FindRelationalMapping(),
                    property));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AddRawParameter(string invariantName, DbParameter dbParameter)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(dbParameter, nameof(dbParameter));

            _parameters.Add(new RawRelationalParameter(invariantName, dbParameter));
        }
    }
}
