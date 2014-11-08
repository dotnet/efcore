// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity;
using QueryExecution.Model;

namespace QueryExecution
{
    public partial class QueryExecutionBase
    {
        public int FuncletizationIterationCount = 5000;

        public void Funcletization_Case1_WithMember(DbContext context)
        {
            var val = 10;
            for (var i = 0; i < FuncletizationIterationCount; i++)
            {
                var query = context.Set<Customer>().Where(customer => customer.CustomerId < val);
                var result = query.ToList();
            }
        }

        public void Funcletization_Case2_WithMember(DbContext context)
        {
            var val = 10;
            var query = context.Set<Customer>().Where(customer => customer.CustomerId < val);
            for (var i = 0; i < FuncletizationIterationCount; i++)
            {
                var result = query.ToList();
            }
        }

        public class FuncletizationWithProperties
        {
            private int value = 10;

            public int FirstLevelProperty
            {
                get { return value; }
                set { this.value = value; }
            }

            public int SecondLevelProperty
            {
                get { return FirstLevelProperty; }
                set { SecondLevelProperty = value; }
            }
        }

        public void Funcletization_Case1_WithProperty(DbContext context)
        {
            var valueHolder = new FuncletizationWithProperties();
            for (var i = 0; i < FuncletizationIterationCount; i++)
            {
                var query = context.Set<Customer>().Where(customer => customer.CustomerId < valueHolder.SecondLevelProperty);
                var result = query.ToList();
            }
        }

        public void Funcletization_Case2_WithProperty(DbContext context)
        {
            var valueHolder = new FuncletizationWithProperties();
            var query = context.Set<Customer>().Where(customer => customer.CustomerId < valueHolder.SecondLevelProperty);
            for (var i = 0; i < FuncletizationIterationCount; i++)
            {
                var result = query.ToList();
            }
        }
    }
}
