// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RelationalParameterBuilder : IRelationalParameterBuilder
    {
        private readonly List<IRelationalParameter> _parameters = new List<IRelationalParameter>();

        public RelationalParameterBuilder([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            TypeMapper = typeMapper;
        }

        public virtual IReadOnlyList<IRelationalParameter> Parameters => _parameters;

        protected virtual IRelationalTypeMapper TypeMapper { get; }

        public virtual void AddParameter(string invariantName, string name)
            => _parameters.Add(
                new DynamicRelationalParameter(
                    Check.NotEmpty(invariantName, nameof(invariantName)),
                    Check.NotEmpty(name, nameof(name)),
                    TypeMapper));

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

        public virtual void AddParameter(string invariantName, string name, IProperty property)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            _parameters.Add(
                new TypeMappedRelationalParameter(
                    invariantName,
                    name,
                    TypeMapper.GetMapping(property),
                    property.IsNullable));
        }

        public virtual void AddCompositeParameter(string invariantName, Action<IRelationalParameterBuilder> buildAction)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotNull(buildAction, nameof(buildAction));

            var innerList = new RelationalParameterBuilder(TypeMapper);

            buildAction(innerList);

            if (innerList.Parameters.Count > 0)
            {
                _parameters.Add(
                    new CompositeRelationalParameter(
                        invariantName,
                        innerList.Parameters));
            }
        }

        public virtual void AddPropertyParameter(string invariantName, string name, IProperty property)
        {
            Check.NotEmpty(invariantName, nameof(invariantName));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            _parameters.Add(
                new TypeMappedPropertyRelationalParameter(
                    invariantName,
                    name,
                    TypeMapper.GetMapping(property),
                    property));
        }
    }
}
