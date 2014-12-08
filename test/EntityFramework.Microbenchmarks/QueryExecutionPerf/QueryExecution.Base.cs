// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;
using Microsoft.Data.Entity;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf
{
    public partial class QueryExecutionBase
    {
        public readonly string DefaultConnectionString =
            @"Data Source={0};Initial Catalog=EF7_QueryExecution_TPT;Integrated Security=True; MultipleActiveResultSets=true";

        public const string AppSettingsDefaultServerKey = @"Server";

        public bool EnableMultipleActiveResultSets = false;

        public QueryExecutionBase()
        {
            DefaultConnectionString = string.Format(DefaultConnectionString, TestConfig.Instance.DataSource);
        }

        public void Filter_Where(DbContext context)
        {
            var id = 0;
            var query = from customer in context.Set<Customer>()
                where customer.CustomerId == id
                select new
                    {
                        customer.CustomerId,
                        customer.Name
                    };
            var result = query.ToList();
        }

        public void Projection_Select(DbContext context)
        {
            var query = from customer in context.Set<Customer>()
                select new
                    {
                        customer.CustomerId,
                        customer.Name
                    };
            var result = query.ToList();
        }

        public void Projection_SelectMany(DbContext context)
        {
            var query = from product in context.Set<Product>()
                from productOrder in context.Set<Product>()
                where product.ProductId == productOrder.ProductId
                select new
                    {
                        product.ProductId,
                        productOrder.Description
                    };
            var result = query.ToList();
        }

        public void Projection_Nested(DbContext context)
        {
            var query = from product in context.Set<Product>()
                from productOrder in context.Set<Product>()
                where product.ProductId == productOrder.ProductId
                select new
                    {
                        product.ProductId,
                        productOrder.Description,
                        AdditionalData = new
                            {
                                product.Description,
                                productOrder.ComplexConcurrency_QueriedDateTime,
                                productOrder.ComplexConcurrency_Token,
                                product.NestedComplexConcurrency_ModifiedBy,
                                product.NestedComplexConcurrency_ModifiedDate,
                            },
                    };
            var result = query.ToList();
        }

        public void Ordering_OrderBy(DbContext context)
        {
            var query = from product in context.Set<Product>()
                orderby product.ProductId
                select product;

            var result = query.ToList();
        }

        public void Aggregate_Count(DbContext context)
        {
            var query = from product in context.Set<Product>()
                select new
                    {
                        PhotoCount = product.Photos.Count,
                        product.ProductId
                    };

            var result = query.ToList();
        }

        public void Partitioning_Skip(DbContext context)
        {
            var query = (from product in context.Set<Product>()
                orderby product.ProductId
                select product).Skip(1).Take(3);

            var result = query.ToList();
        }

        public void Join_Join(DbContext context)
        {
            var query = from product in context.Set<Product>()
                join productOrder in context.Set<Product>()
                    on product.ProductId equals productOrder.ProductId
                select new
                    {
                        product.ProductId,
                        productOrder.Description
                    };

            var result = query.ToList();
        }

        public void Grouping_Groupby(DbContext context)
        {
            var query = from customer in context.Set<Customer>()
                group customer by customer.Name
                into customerGroup
                select new
                    {
                        Name = customerGroup.Key,
                        customerGroup
                    };

            var result = query.ToList();
        }

        public void Include(DbContext context)
        {
            var query = from customer in context.Set<Customer>()
                select customer;

            var result = query.ToList();
        }

        public void OfType_Linq(DbContext context)
        {
            var query = from orderline in context.Set<OrderLine>().OfType<BackOrderLine>()
                select orderline;
            var result = query.ToList();
        }

        public void Filter_Not_PK_Parameter(DbContext context)
        {
            var id = 0;
            var query = from customer in context.Set<Customer>()
                where customer.CustomerId != id
                select new
                    {
                        customer.CustomerId,
                        customer.Name
                    };

            var result = query.ToList();
        }

        public void Filter_Not_NF_Parameter(DbContext context)
        {
            var extension = "x123";
            var query = from customer in context.Set<Customer>()
                where customer.ContactInfo_WorkPhone_Extension != extension
                select new
                    {
                        customer.CustomerId,
                        customer.Name
                    };

            var result = query.ToList();
        }

        public void Filter_Not_NNF_Parameter(DbContext context)
        {
            var name = "10";
            var query = from customer in context.Set<Customer>()
                where customer.Name != name
                select new
                    {
                        customer.CustomerId,
                        customer.Name
                    };

            var result = query.ToList();
        }
    }
}
