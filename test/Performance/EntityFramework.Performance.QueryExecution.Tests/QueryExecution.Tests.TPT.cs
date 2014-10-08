// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution
{
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Services;
    using Microsoft.Framework.ConfigurationModel;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.DependencyInjection.Advanced;
    using Microsoft.Framework.DependencyInjection.Fallback;
    using QueryExecution.Model;
    using System;


    public class QueryExecutionTestsTPT : QueryExecutionBase
    {
        QueryExecutionTPT _tptContext;

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddSqlServer().UseLoggerFactory<NullLoggerFactory>();
            return services.BuildServiceProvider();
        }

        public void Setup()
        {
            var configuration = new Configuration();
            string connectionString = null;

            try
            {
                configuration.AddJsonFile(@"LocalConfig.json");
                connectionString = configuration.Get("Data:DefaultConnection:Connectionstring");
            }
            catch (Exception e)
            {
                Console.WriteLine("error reading config: " + e.Message);
            }
            
            connectionString = connectionString ?? QueryExecutionBase.DefaultConnectionString;
            var serviceProvider = CreateServiceProvider();
            var options = new DbContextOptions();
            base.SetupDatabase(() => new QueryExecutionTPT(connectionString, serviceProvider, options));

            _tptContext = new QueryExecutionTPT(connectionString, serviceProvider, options);
        }

        //[Test("Query_Execution_TPT_model_Filter_Where",
        //    Description = "Query Execution with TPT model that uses Where",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Filter_Where()
        {
            base.Filter_Where(_tptContext);
        }


        //[Test("Query_Execution_TPT_model_Projection_Select",
        //    Description = "Query Execution with TPT model that uses Select",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Projection_Select()
        {
            base.Projection_Select(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Projection_SelectMany",
        //    Description = "Query Execution with TPT model that uses SelectMany",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Projection_SelectMany()
        {
            base.Projection_SelectMany(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Projection_Nested",
        //    Description = "Query Execution with TPT model that uses Select with nested anonymous types",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.Medium)]
        public void TPT_Projection_Nested()
        {
            base.Projection_Nested(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Ordering_OrderBy",
        //    Description = "Query Execution with TPT model that uses OrderBy",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Ordering_OrderBy()
        {
            base.Ordering_OrderBy(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Aggregate_Count",
        //    Description = "Query Execution with TPT model that uses Count",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Aggregate_Count()
        {
            base.Aggregate_Count(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Partitioning_Skip",
        //    Description = "Query Execution with TPT model that uses Skip",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Partitioning_Skip()
        {
            base.Partitioning_Skip(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Join_Join",
        //    Description = "Query Execution with TPT model that uses Join",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Join_Join()
        {
            base.Join_Join(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Grouping_Groupby",
        //    Description = "Query Execution with TPT model that uses Groupby",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Grouping_Groupby()
        {
            base.Grouping_Groupby(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Include",
        //    Description = "Query Execution with TPT model that uses include",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.High)]
        public void TPT_Include()
        {
            base.Include(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_OfType_Linq",
        //    Description = "Query Execution with TPT model that uses OfType in Linq",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.BVT)]
        public void TPT_OfType_Linq()
        {
            base.OfType_Linq(_tptContext);
        }

        //[Test("Query_Execution_TPT_Filter_Not_PK_Parameter",
        //    Description = "Query Execution with TPT model that uses a WHERE NOT filter applied on a PK field, with a parameter",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.Medium)]
        public void TPT_Filter_Not_PK_Parameter()
        {
            base.Filter_Not_PK_Parameter(_tptContext);
        }

        //[Test("Query_Execution_TPT_Filter_Not_NF_Parameter",
        //    Description = "Query Execution with TPT model that uses a WHERE NOT filter applied on a NULLABLE field, with a parameter",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.Medium)]
        public void TPT_Filter_Not_NF_Parameter()
        {
            base.Filter_Not_NF_Parameter(_tptContext);
        }

        //[Test("Query_Execution_TPT_Filter_Not_NNF_Parameter",
        //    Description = "Query Execution with TPT model that uses a WHERE NOT filter applied on a NON-NULLABLE field, with a parameter",
        //    WarmupIterations = 10,
        //    TestIterations = 500,
        //    Priority = TestPriority.Medium)]
        public void TPT_Filter_Not_NNF_Parameter()
        {
            base.Filter_Not_NNF_Parameter(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Funcletization_Case1_WithMember",
        //    Description = "Query Execution with TPT model that tests Funcletization Case 1 With Member",
        //    WarmupIterations = 0,
        //    TestIterations = 1,
        //    Priority = TestPriority.Medium)]
        public void TPT_Funcletization_Case1_WithMember()
        {
            base.Funcletization_Case1_WithMember(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Funcletization_Case2_WithMember",
        //    Description = "Query Execution with TPT model that tests Funcletization Case 2 With Member",
        //    WarmupIterations = 0,
        //    TestIterations = 1,
        //    Priority = TestPriority.Medium)]
        public void TPT_Funcletization_Case2_WithMember()
        {
            base.Funcletization_Case2_WithMember(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Funcletization_Case1_WithProperty",
        //    Description = "Query Execution with TPT model that tests Funcletization Case 1 With Property",
        //    WarmupIterations = 0,
        //    TestIterations = 1,
        //    Priority = TestPriority.Medium)]
        public void TPT_Funcletization_Case1_WithProperty()
        {
            base.Funcletization_Case1_WithProperty(_tptContext);
        }

        //[Test("Query_Execution_TPT_model_Funcletization_Case2_WithProperty",
        //    Description = "Query Execution with TPT model that tests Funcletization Case 2 With Property",
        //    WarmupIterations = 0,
        //    TestIterations = 1,
        //    Priority = TestPriority.Medium)]
        public void TPT_Funcletization_Case2_WithProperty()
        {
            base.Funcletization_Case2_WithProperty(_tptContext);
        }
    }
}
