// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class ProceduralQueryExpressionGenerator
    {
        private readonly List<ExpressionMutator> _mutators;

        // used to hard code the seed used for test generation
        public static readonly int? Seed = null;

        public ProceduralQueryExpressionGenerator(DbContext context)
        {
            _mutators = new List<ExpressionMutator>
            {
                new AppendSelectConstantExpressionMutator(context),
                new AppendSelectIdentityExpressionMutator(context),
                new AppendSelectPropertyExpressionMutator(context),
                new AppendOrderByIdentityExpressionMutator(context),
                new AppendOrderByPropertyExpressionMutator(context),
                new AppendThenByIdentityExpressionMutator(context),
                new AppendTakeExpressionMutator(context),
                new StringConcatWithSelfExpressionMutator(context),
                new InjectCoalesceExpressionMutator(context),
                new InjectStringFunctionExpressionMutator(context),
                new InjectJoinWithSelfExpressionMutator(context),
                new InjectOrderByPropertyExpressionMutator(context),
                new InjectThenByPropertyExpressionMutator(context),
                new AppendCorrelatedCollectionExpressionMutator(context),
                new AppendIncludeToExistingExpressionMutator(context),
                new InjectIncludeExpressionMutator(context),
                new InjectWhereExpressionMutator(context)
            };
        }

        public Expression Generate(Expression expression, Random random)
        {
            var validMutators = _mutators.Where(m => m.IsValid(expression)).ToList();
            if (validMutators.Any())
            {
                var i = random.Next(validMutators.Count);
                var result = validMutators[i].Apply(expression, random);

                return result;
            }

            return expression;
        }
    }

    public class ProcedurallyGeneratedQueryExecutor
    {
        private static readonly Dictionary<string, List<string>> _knownFailingTests = new Dictionary<string, List<string>>();

        static ProcedurallyGeneratedQueryExecutor()
        {
            // various failures due to random expressions being generated
            AddExpectedFailure("Where_Join_Exists", "Year, Month, and Day parameters describe an un-representable DateTime.");
            AddExpectedFailure("Where_Join_Exists_Inequality", "Year, Month, and Day parameters describe an un-representable DateTime.");
            AddExpectedFailure("Where_Join_Any", "Year, Month, and Day parameters describe an un-representable DateTime.");

            AddExpectedFailure(
                "Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast",
                "Arithmetic overflow error");
            AddExpectedFailure(
                "Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault", "Sequence contains more than one element");

            // NRE due to client eval
            AddExpectedFailure(
                "GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method",
                "Object reference not set to an instance of an object.");
            AddExpectedFailure("Null_reference_protection_complex_client_eval", "Object reference not set to an instance of an object.");
            AddExpectedFailure(
                "Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter",
                "Object reference not set to an instance of an object.");
            AddExpectedFailure(
                "Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault",
                "Object reference not set to an instance of an object.");
            AddExpectedFailure("Select_null_propagation_negative4", "Object reference not set to an instance of an object.");
            AddExpectedFailure("Select_null_propagation_negative5", "Object reference not set to an instance of an object.");
            AddExpectedFailure(
                "Select_conditional_with_anonymous_type_and_null_constant", "Object reference not set to an instance of an object.");
            AddExpectedFailure("SelectMany_with_order_by_and_Include", "Object reference not set to an instance of an object.");
            AddExpectedFailure("Include_with_orderby_skip_preserves_ordering", "Object reference not set to an instance of an object.");
            AddExpectedFailure("SelectMany_with_Include_and_order_by", "Object reference not set to an instance of an object.");
            AddExpectedFailure(
                "Where_complex_predicate_with_with_nav_prop_and_OrElse1", "Object reference not set to an instance of an object.");
            AddExpectedFailure("SelectMany_with_Include1", "Object reference not set to an instance of an object.");
            AddExpectedFailure("Include_collection_with_Cast_to_base", "Object reference not set to an instance of an object.");

            AddExpectedFailure("Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result", "Value cannot be null.");
            AddExpectedFailure(
                "Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result", "Value cannot be null.");
            AddExpectedFailure("Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3", "Value cannot be null.");
            AddExpectedFailure("SelectMany_with_Include_and_order_by", "Value cannot be null.");

            // using EF.Property on client due to client eval
            AddExpectedFailure("SelectMany_navigation_comparison3", "The EF.Property<T> method may only be used within LINQ queries.");
            AddExpectedFailure(
                "Manually_created_left_join_propagates_nullability_to_navigations",
                "The EF.Property<T> method may only be used within LINQ queries.");
            AddExpectedFailure(
                "Correlated_collections_left_join_with_self_reference", "The EF.Property<T> method may only be used within LINQ queries.");
            AddExpectedFailure(
                "Correlated_collections_on_left_join_with_null_value", "The EF.Property<T> method may only be used within LINQ queries.");
            AddExpectedFailure("GroupBy_Shadow", "The EF.Property<T> method may only be used within LINQ queries.");
            AddExpectedFailure("GroupBy_Shadow3", "The EF.Property<T> method may only be used within LINQ queries.");
            AddExpectedFailure(
                "Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method",
                "The EF.Property<T> method may only be used within LINQ queries.");

            // ---------------------------------------------------------------- won't fix bugs -----------------------------------------------------------------

            AddExpectedFailure(
                "Select_constant_null_string", "A constant expression was encountered in the ORDER BY list, position 1."); // 12638

            // -------------------------------------------------------------- actual product bugs --------------------------------------------------------------

            AddExpectedFailure("Except_simple", "cannot be used for"); // 12568
            AddExpectedFailure("Except_dbset", "cannot be used for"); // 12568
            AddExpectedFailure("Except_nested", "cannot be used for"); // 12568

            AddExpectedFailure("GroupBy_aggregate_Pushdown", "Invalid column name 'c'."); // 12569
            AddExpectedFailure("GroupBy_with_orderby_take_skip_distinct", "Invalid column name 'c'."); // 12569

            AddExpectedFailure(
                "GroupBy_Select_First_GroupBy",
                "Query source (from Customer c in [g]) has already been associated with an expression."); // 12573

            AddExpectedFailure("Join_Customers_Orders_Skip_Take", "Object reference not set to an instance of an object."); // 12574
            AddExpectedFailure(
                "Join_Customers_Orders_Projection_With_String_Concat_Skip_Take",
                "Object reference not set to an instance of an object."); // 12574
            AddExpectedFailure(
                "Join_Customers_Orders_Orders_Skip_Take_Same_Properties", "Object reference not set to an instance of an object."); // 12574

            AddExpectedFailure(
                "SelectMany_navigation_property",
                "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "SelectMany_navigation_property_and_filter_before",
                "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "SelectMany_navigation_property_and_filter_after",
                "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "SelectMany_where_with_subquery",
                "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "SelectMany_nested_navigation_property_required",
                "The property '' on entity type 'Level3' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "SelectMany_navigation_property_with_another_navigation_in_subquery",
                "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "SelectMany_navigation_property_with_another_navigation_in_subquery",
                "The property '' on entity type 'Level3' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure(
                "Multiple_SelectMany_calls",
                "The property '' on entity type 'Level3' could not be found. Ensure that the property exists and has been included in the model."); // 12575

            AddExpectedFailure(
                "GroupBy_with_orderby_take_skip_distinct",
                "Unable to cast object of type 'Remotion.Linq.Clauses.ResultOperators.DistinctResultOperator' to type 'Remotion.Linq.Clauses.ResultOperators.GroupResultOperator'."); // 12576
            AddExpectedFailure(
                "GroupBy_Distinct",
                "Unable to cast object of type 'Remotion.Linq.Clauses.ResultOperators.DistinctResultOperator' to type 'Remotion.Linq.Clauses.ResultOperators.GroupResultOperator'."); // 12576

            AddExpectedFailure("Correlated_collections_naked_navigation_with_ToList", "Rewriting child expression from type"); // 12579

            AddExpectedFailure(
                "Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault",
                "Only one expression can be specified in the select list when the subquery is not introduced with EXISTS."); // 12580

            AddExpectedFailure("Optional_navigation_type_compensation_works_with_DTOs", "Value cannot be null."); // 12591
            AddExpectedFailure("ToString_with_formatter_is_evaluated_on_the_client", "Value cannot be null."); // 12591
            AddExpectedFailure("Select_expression_datetime_add_hour", "Value cannot be null."); // 12591
            AddExpectedFailure("Select_expression_long_to_string", "Value cannot be null."); // 12591
            AddExpectedFailure("Query_expression_with_to_string_and_contains", "Value cannot be null."); // 12591
            AddExpectedFailure("Queryable_reprojection", "Value cannot be null."); // 12591
            AddExpectedFailure("Queryable_simple_anonymous_subquery", "Value cannot be null."); // 12591

            AddExpectedFailure(
                "Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault",
                "Object reference not set to an instance of an object."); // 12597
            AddExpectedFailure(
                "Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2",
                "Object reference not set to an instance of an object."); // 12597
            AddExpectedFailure(
                "Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault",
                "Object reference not set to an instance of an object."); // 12597

            AddExpectedFailure(
                "Order_by_length_twice",
                "Unable to cast object of type 'System.Linq.Expressions.PropertyExpression' to type 'Remotion.Linq.Clauses.Expressions.QuerySourceReferenceExpression'."); // 12598

            AddExpectedFailure(
                "GroupBy_Shadow3",
                "Column 'Employees.Title' is invalid in the select list because it is not contained in either an aggregate function or the GROUP BY clause."); // 12598

            AddExpectedFailure("GroupBy_Shadow", "Unable to cast object of type 'System.String' to type"); // 12601
            AddExpectedFailure(
                "Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method",
                "Unable to cast object of type 'System.String' to type"); // 12601

            AddExpectedFailure("GroupBy_Shadow", "Value does not fall within the expected range."); // 12640
            AddExpectedFailure("GroupBy_Shadow3", "Value does not fall within the expected range."); // 12640
            AddExpectedFailure("GroupBy_SelectMany", "Value does not fall within the expected range."); // 12640

            AddExpectedFailure("GroupBy_with_orderby_take_skip_distinct", "_TrackGroupedEntities"); // 12641

            AddExpectedFailure(
                "Select_collection_navigation_simple",
                "Index was out of range. Must be non-negative and less than the size of the collection."); // 12643
            AddExpectedFailure(
                "Correlated_collections_nested_with_custom_ordering",
                "Index was out of range. Must be non-negative and less than the size of the collection."); // 12643
            AddExpectedFailure(
                "Correlated_collections_multiple_nested_complex_collections",
                "Index was out of range. Must be non-negative and less than the size of the collection."); // 12643

            AddExpectedFailure("GroupJoin_GroupBy_Aggregate_5", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_Key_as_part_of_element_selector", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_Property_Select_Key_Min", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_Property_Include_Aggregate_with_anonymous_selector", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_aggregate_Pushdown", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_anonymous_with_alias_Select_Key_Sum", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_Property_Select_Key_LongCount", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_filter_count_OrderBy_count_Select_sum", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_filter_key", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("Join_GroupBy_Aggregate_multijoins", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_Property_Select_Key_Sum_Min_Max_Avg", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_optional_navigation_member_Aggregate", "Incorrect syntax near '+'."); // 12656
            AddExpectedFailure("GroupBy_Property_Select_Key_Max", "Incorrect syntax near '+'."); // 12656

            AddExpectedFailure("Collection_select_nav_prop_sum", "Nullable object must have a value."); // 12657

            AddExpectedFailure("Simple_owned_level1_level2_GroupBy_Having_Count", "Incorrect syntax near '+'."); // 12658

            AddExpectedFailure("Join_navigation_translated_to_subquery_composite_key", "Invalid column name 'Note'."); // 12786
            AddExpectedFailure("Join_on_entity_qsre_keys_inner_key_is_navigation_composite_key", "Invalid column name 'Note'."); // 12786
            AddExpectedFailure("Join_on_entity_qsre_keys_inner_key_is_navigation", "Invalid column name 'Nickname'."); // 12786
            AddExpectedFailure("Client_method_on_collection_navigation_in_outer_join_key", "Invalid column name 'Nickname'."); // 12786

            AddExpectedFailure(
                "Join_navigation_translated_to_subquery_deeply_nested_non_key_join", "Parameter name: tableExpression"); // 12787
            AddExpectedFailure("Join_on_entity_qsre_keys_inner_key_is_nested_navigation", "Parameter name: tableExpression"); // 12787
            AddExpectedFailure("Join_navigation_translated_to_subquery_deeply_nested_required", "Parameter name: tableExpression"); // 12787
            AddExpectedFailure("Join_navigation_translated_to_subquery_nested", "Parameter name: tableExpression"); // 12787
            AddExpectedFailure("Query_source_materialization_bug_4547", "Parameter name: tableExpression"); // 12787
            AddExpectedFailure("GroupJoin_with_complex_subquery_and_LOJ_gets_flattened", "Parameter name: tableExpression"); // 12787
            AddExpectedFailure("GroupJoin_with_complex_subquery_and_LOJ_gets_flattened2", "Parameter name: tableExpression"); // 12787

            AddExpectedFailure("SelectMany_with_Include1", "must be reducible node"); // 12794
            AddExpectedFailure("Multiple_SelectMany_with_Include", "must be reducible node"); // 12794
            AddExpectedFailure("SelectMany_with_Include_ThenInclude", "must be reducible node"); // 12794
            AddExpectedFailure("Include_after_SelectMany_and_reference_navigation", "must be reducible node"); // 12794
            AddExpectedFailure("Include_after_multiple_SelectMany_and_reference_navigation", "must be reducible node"); // 12794
            AddExpectedFailure("Include_after_SelectMany_and_multiple_reference_navigations", "must be reducible node"); // 12794
            AddExpectedFailure("Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4", "must be reducible node"); // 12794
            AddExpectedFailure("Include_with_join_collection2", "must be reducible node"); // 12794
            AddExpectedFailure("Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result", "must be reducible node"); // 12794
            AddExpectedFailure("Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3", "must be reducible node"); // 12794
            AddExpectedFailure("Include_with_join_and_inheritance3", "must be reducible node"); // 12794

            AddExpectedFailure("Join_GroupBy_Aggregate", "must be reducible node"); // 12799
            AddExpectedFailure("GroupJoin_GroupBy_Aggregate_2", "must be reducible node"); // 12799
            AddExpectedFailure("GroupJoin_GroupBy_Aggregate_3", "must be reducible node"); // 12799
            AddExpectedFailure("GroupJoin_GroupBy_Aggregate_4", "must be reducible node"); // 12799
            AddExpectedFailure("GroupJoin_GroupBy_Aggregate_5", "must be reducible node"); // 12799
            AddExpectedFailure("Self_join_GroupBy_Aggregate", "must be reducible node"); // 12799
            AddExpectedFailure("Join_complex_GroupBy_Aggregate", "must be reducible node"); // 12799
            AddExpectedFailure("OrderBy_Take_GroupBy_Aggregate", "must be reducible node"); // 12799

            AddExpectedFailure(
                "Include_with_join_collection2",
                "could not be found. Ensure that the property exists and has been included in the model."); // 12802
            AddExpectedFailure(
                "Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3",
                "could not be found. Ensure that the property exists and has been included in the model."); // 12802
            AddExpectedFailure(
                "Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result4",
                "could not be found. Ensure that the property exists and has been included in the model."); // 12802
            AddExpectedFailure(
                "Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result",
                "could not be found. Ensure that the property exists and has been included in the model."); // 12802
            AddExpectedFailure(
                "Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result",
                "could not be found. Ensure that the property exists and has been included in the model."); // 12802
            AddExpectedFailure(
                "Include_with_join_and_inheritance3",
                "could not be found. Ensure that the property exists and has been included in the model."); // 12802

            AddExpectedFailure(
                "Join_navigation_translated_to_subquery_non_key_join", "Index was outside the bounds of the array."); // 12804

            AddExpectedFailure("OrderBy_Skip_GroupBy_Aggregate", "Value does not fall within the expected range."); // 12805
            AddExpectedFailure("OrderBy_Skip_Take_GroupBy_Aggregate", "Value does not fall within the expected range."); // 12805
            AddExpectedFailure("GroupJoin_complex_GroupBy_Aggregate", "Value does not fall within the expected range."); // 12805
            AddExpectedFailure("OrderBy_GroupBy_SelectMany", "Value does not fall within the expected range."); // 12805

            AddExpectedFailure(
                "GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner",
                "Invalid column name 'Level1_Optional_Id'."); // 12806

            AddExpectedFailure(
                "Let_group_by_nav_prop",
                "A column has been specified more than once in the order by list. Columns in the order by list must be unique."); // 12816

            AddExpectedFailure(
                "Select_expression_references_are_updated_correctly_with_subquery",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_int_to_string",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "ToString_with_formatter_is_evaluated_on_the_client",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Query_expression_with_to_string_and_contains",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Projection_containing_DateTime_subtraction",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_other_to_string",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_long_to_string",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_date_add_year",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_datetime_add_month",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_datetime_add_hour",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_datetime_add_minute",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_datetime_add_second",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_date_add_milliseconds_below_the_range",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_date_add_milliseconds_above_the_range",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_date_add_milliseconds_large_number_divided",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819
            AddExpectedFailure(
                "Select_expression_datetime_add_ticks",
                "The conversion of a varchar data type to a datetime data type resulted in an out-of-range value."); // 12819

            AddExpectedFailure(
                "Projection_containing_DateTime_subtraction",
                "Conversion failed when converting date and/or time from character string."); // 12797
            AddExpectedFailure("Where_chain", "Conversion failed when converting date and/or time from character string."); // 12797
            AddExpectedFailure(
                "Projection_containing_DateTime_subtraction",
                "Conversion failed when converting date and/or time from character string."); // 12797
            AddExpectedFailure(
                "Where_Join_Exists_Inequality", "Conversion failed when converting date and/or time from character string."); // 12797
            AddExpectedFailure("Where_Join_Any", "Conversion failed when converting date and/or time from character string."); // 12797
            AddExpectedFailure("Where_Join_Exists", "Conversion failed when converting date and/or time from character string."); // 12797

            AddExpectedFailure(
                "Parameter_extraction_short_circuits_1",
                "An exception was thrown while attempting to evaluate the LINQ query parameter expression"); // 12820
            AddExpectedFailure(
                "Parameter_extraction_short_circuits_3",
                "An exception was thrown while attempting to evaluate the LINQ query parameter expression"); // 12820

            AddExpectedFailure("Include_with_join_reference2", "Invalid column name '"); // 12827
            AddExpectedFailure("Include_with_join_and_inheritance1", "Invalid column name '"); // 12827
            AddExpectedFailure("Include_with_join_and_inheritance3", "Invalid column name '"); // 12827

            AddExpectedFailure(
                "Entity_equality_local",
                "has already been declared. Variable names must be unique within a query batch or stored procedure."); // 12871
            AddExpectedFailure(
                "Where_poco_closure",
                "has already been declared. Variable names must be unique within a query batch or stored procedure."); // 12871

            AddExpectedFailure("QueryType_with_defining_query", "Object reference not set to an instance of an object."); // 12873

            AddExpectedFailure(
                "QueryType_with_included_navs_multi_level", "Object reference not set to an instance of an object."); // 12874

            AddExpectedFailure(
                "Include_with_concat",
                ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889
            AddExpectedFailure(
                "Concat_with_groupings",
                ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889
            AddExpectedFailure(
                "Union_dbset", ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889
            AddExpectedFailure(
                "Concat_nested", ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889
            AddExpectedFailure(
                "Union_simple", ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889
            AddExpectedFailure(
                "Union_nested", ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889
            AddExpectedFailure(
                "Where_subquery_concat_order_by_firstordefault_boolean",
                ", but it has items of type 'Microsoft.EntityFrameworkCore.Query.Internal.AnonymousObject'."); // 12889

            AddExpectedFailure("Select_null_propagation_negative1", "Specified cast is not valid."); // 12958
            AddExpectedFailure("Select_null_propagation_negative2", "Specified cast is not valid."); // 12958
            AddExpectedFailure("Select_null_propagation_negative3", "Specified cast is not valid."); // 12958
            AddExpectedFailure("Select_null_propagation_negative4", "Specified cast is not valid."); // 12958
            AddExpectedFailure("Select_null_propagation_negative5", "Specified cast is not valid."); // 12958
            AddExpectedFailure("Select_null_propagation_negative6", "Specified cast is not valid."); // 12958
            AddExpectedFailure("Select_null_propagation_negative7", "Specified cast is not valid."); // 12958

            AddExpectedFailure("DefaultIfEmpty_in_subquery", "' is not defined for type '"); // 12960
            AddExpectedFailure("SelectMany_Joined_DefaultIfEmpty2", "' is not defined for type '"); // 12960
        }

        private static void AddExpectedFailure(string testName, string expectedException)
        {
            if (_knownFailingTests.ContainsKey(testName))
            {
                _knownFailingTests[testName].Add(expectedException);
            }
            else
            {
                _knownFailingTests[testName] = new List<string> { expectedException };
            }
        }

        public void Execute<TElement>(IQueryable<TElement> query, DbContext context, string testMethodName)
        {
            var seed = ProceduralQueryExpressionGenerator.Seed ?? new Random().Next();
            var random = new Random(seed);
            var depth = 2;

            IQueryable newQuery = query;
            Expression newExpression = null;
            for (var i = 0; i < depth; i++)
            {
                var expression = newQuery.Expression;

                try
                {
                    var queryGenerator = new ProceduralQueryExpressionGenerator(context);
                    newExpression = queryGenerator.Generate(expression, random);
                }
                catch
                {
                    Console.WriteLine("SEED:" + seed);

                    throw;
                }

                newQuery = query.Provider.CreateQuery(newExpression);
            }

            // printed just for debugging purposes
            var queryString = newExpression.Print();

            try
            {
                foreach (var r in newQuery)
                {
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("A constant expression was encountered in the ORDER BY list")
                    || exception.Message.Contains("has already been associated with an expression.")
                    || exception.Message.Contains("Object reference not set to an instance of an object."))
                {
                }
                else if (exception.Message == @"Invalid column name 'Key'.") // 12564
                {
                }
                else if (exception.Message.StartsWith(
                    @"Error generated for warning 'Microsoft.EntityFrameworkCore.Query.IncludeIgnoredWarning"))
                {
                }
                else if (exception.Message.Contains(@"The expected type was 'System.Int64' but the actual value was of type")) // 12570
                {
                }
                else if (exception.Message.Contains(
                             @"The expected type was 'System.UInt32' but the actual value was of type 'System.Int32'")
                         || exception.Message.Contains(
                             @"The expected type was 'System.Nullable`1[System.UInt32]' but the actual value was of type 'System.Int32'.")
                ) // 13753
                {
                }
                else if (exception.Message
                         == @"The binary operator NotEqual is not defined for the types 'Microsoft.EntityFrameworkCore.Storage.ValueBuffer' and 'Microsoft.EntityFrameworkCore.Storage.ValueBuffer'."
                ) // 12788
                {
                }
                else if (exception.Message.Contains(@"Incorrect syntax near the keyword 'AS'.")) // 12826
                {
                }
                else
                {
                    if (_knownFailingTests.ContainsKey(testMethodName)
                        && _knownFailingTests[testMethodName].Any(e => exception.Message.Contains(e)))
                    {
                    }
                    else
                    {
                        Console.WriteLine("SEED: " + seed + " TEST: " + testMethodName);

                        throw;
                    }
                }
            }
        }
    }
}
