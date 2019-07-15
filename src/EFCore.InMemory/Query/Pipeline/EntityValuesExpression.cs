// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, Expression> _readExpressionMap;

        public EntityProjectionExpression(
            IEntityType entityType, IDictionary<IProperty, Expression> readExpressionMap)
        {
            EntityType = entityType;
            _readExpressionMap = readExpressionMap;
        }

        public IEntityType EntityType { get; }
        public override Type Type => EntityType.ClrType;
        public override ExpressionType NodeType => ExpressionType.Extension;

        public Expression BindProperty(IProperty property)
        {
            if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    $"Called EntityProjectionExpression.BindProperty() with incorrect IProperty. EntityType:{EntityType.DisplayName()}, Property:{property.Name}");
            }

            return _readExpressionMap[property];
        }
    }
}
