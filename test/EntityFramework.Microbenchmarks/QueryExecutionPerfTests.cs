using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.QueryExecutionPerf;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public class QueryExecutionPerfTests
    {
        private readonly string defaultResultDirectory;
        private PerfTestRunner runner;
        private QueryExecutionTestsTPT tests;


        public QueryExecutionPerfTests()
        {
            runner = new PerfTestRunner();
            tests = new QueryExecutionTestsTPT();
            defaultResultDirectory = TestConfig.Instance.ResultsDirectory;
        }

        [Fact]
        public void Query_Execution_TPT_model_Filter_Where()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Filter_Where",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Where,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_model_Projection_Select()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Projection_Select",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Projection_Select,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact]
        public void Query_Execution_TPT_model_Projection_SelectMany()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Projection_SelectMany",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Projection_SelectMany,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact]
        public void Query_Execution_TPT_model_Projection_Nested()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Projection_Nested",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Projection_Nested,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact]
        public void Query_Execution_TPT_model_Ordering_OrderBy()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Ordering_OrderBy",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Ordering_OrderBy,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact] // This test currently fails, trowing an exception from relinq 
        public void Query_Execution_TPT_model_Aggregate_Count()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Aggregate_Count",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Aggregate_Count,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact]
        public void Query_Execution_TPT_model_Partitioning_Skip()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Partitioning_Skip",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Partitioning_Skip,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact]
        public void Query_Execution_TPT_model_Join_Join()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Join_Join",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Join_Join,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_model_Grouping_Groupby()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Grouping_Groupby",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Grouping_Groupby,
                    Setup = tests.Setup
                };
    

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_model_Include()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Include",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Include,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact] // This test currently fails, trowing an exception from relinq 
        public void Query_Execution_TPT_model_OfType_Linq()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_OfType_Linq",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_OfType_Linq,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
            
        [Fact]
        public void Query_Execution_TPT_Filter_Not_PK_Parameter()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_Filter_Not_PK_Parameter",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Not_PK_Parameter,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_Filter_Not_NF_Parameter()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_Filter_Not_NF_Parameter",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Not_NF_Parameter,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_Filter_Not_NNF_Parameter()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_Filter_Not_NNF_Parameter",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Not_NNF_Parameter,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_model_Funcletization_Case1_WithMember()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case1_WithMember",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case1_WithMember,
                    Setup = tests.Setup
                }; 

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_model_Funcletization_Case2_WithMember()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case2_WithMember",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case2_WithMember,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    
        [Fact]
        public void Query_Execution_TPT_model_Funcletization_Case1_WithProperty()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case1_WithProperty",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case1_WithProperty,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void Query_Execution_TPT_model_Funcletization_Case2_WithProperty()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case2_WithProperty",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case2_WithProperty,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

    }
}
