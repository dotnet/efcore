// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution
{
    using QueryExecution.Model;
    using Microsoft.Data.Entity;
    using System.Linq;

    public partial class QueryExecutionBase
    {
        public int FuncletizationIterationCount = 5000;

        public void Funcletization_Case1_WithMember(DbContext context)
        {
            int val = 10;
            for (int i = 0; i < FuncletizationIterationCount; i++)
            {
                var query = context.Set<Customer>().Where(customer => customer.CustomerId < val);
                var result = query.ToList();
            }
        }

        public void Funcletization_Case2_WithMember(DbContext context)
        {
            int val = 10;
            var query = context.Set<Customer>().Where(customer => customer.CustomerId < val);
            for (int i = 0; i < FuncletizationIterationCount; i++)
            {
                var result = query.ToList();
            }
        }

        public class FuncletizationWithProperties
        {
            private int value = 10;

            public int FirstLevelProperty
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                }
            }

            public int SecondLevelProperty
            {
                get
                {
                    return this.FirstLevelProperty;
                }
                set
                {
                    this.SecondLevelProperty = value;
                }
            }
        }

        public void Funcletization_Case1_WithProperty(DbContext context)
        {
            FuncletizationWithProperties valueHolder = new FuncletizationWithProperties();
            for (int i = 0; i < FuncletizationIterationCount; i++)
            {
                var query = context.Set<Customer>().Where(customer => customer.CustomerId < valueHolder.SecondLevelProperty);
                var result = query.ToList();
            }
        }

        public void Funcletization_Case2_WithProperty(DbContext context)
        {
            FuncletizationWithProperties valueHolder = new FuncletizationWithProperties();
            var query = context.Set<Customer>().Where(customer => customer.CustomerId < valueHolder.SecondLevelProperty);
            for (int i = 0; i < FuncletizationIterationCount; i++)
            {
                var result = query.ToList();
            }
        }
    }
}
