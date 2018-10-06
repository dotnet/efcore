// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal
{
    public class KeyAccessExpression : Expression
    {
        private readonly IPropertyBase _propertyBase;
        private readonly Expression _outerExpression;

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _propertyBase.ClrType;

        public KeyAccessExpression(IPropertyBase propertyBase, Expression outerExpression)
        {
            _propertyBase = propertyBase;
            _outerExpression = outerExpression;
        }

        public override string ToString()
        {
            return $"{_outerExpression}[\"{_propertyBase.Name}\"]";
        }
    }
}
