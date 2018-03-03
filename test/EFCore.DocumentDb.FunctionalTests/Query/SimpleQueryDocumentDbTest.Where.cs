// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryDocumentDbTest : SimpleQueryTestBase<NorthwindQueryDocumentDbFixture<NoopModelCustomizer>>
    {
        public override void Where_simple()
        {
            base.Where_simple();

            AssertSql(
                @"SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = ""London""");
        }

        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            AssertSql(
                @"@__city_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_0");
        }

        public override void Where_indexer_closure()
        {
            base.Where_indexer_closure();

            AssertSql(
                @"@__p_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__p_0");
        }

        public override void Where_dictionary_key_access_closure()
        {
            base.Where_dictionary_key_access_closure();

            AssertSql(
                @"@__get_Item_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__get_Item_0");
        }

        public override void Where_tuple_item_closure()
        {
            base.Where_tuple_item_closure();

            AssertSql(
                @"@__predicateTuple_Item2_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__predicateTuple_Item2_0");
        }

        public override void Where_named_tuple_item_closure()
        {
            base.Where_named_tuple_item_closure();

            AssertSql(
                @"@__predicateTuple_Item2_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__predicateTuple_Item2_0");
        }

        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();

            AssertSql(
                @"@__predicate_0='True'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE @__predicate_0");
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            AssertSql(
                @"@__city_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_0",
                //
                @"@__city_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_0");
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            AssertSql(
                @"@__city_Int_0='2'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__city_Int_0",
                //
                @"@__city_Int_0='5'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__city_Int_0");
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            AssertSql(
                @"@__city_NullableInt_0='1'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""EmployeeID""] > @__city_NullableInt_0",
                //
                @"@__city_NullableInt_0='5'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""EmployeeID""] > @__city_NullableInt_0");
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            AssertSql(
                @"@__GetCity_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__GetCity_0",
                //
                @"@__GetCity_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__GetCity_0");
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            AssertSql(
                @"@__city_InstanceFieldValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_InstanceFieldValue_0",
                //
                @"@__city_InstanceFieldValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_InstanceFieldValue_0");
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            AssertSql(
                @"@__city_InstancePropertyValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_InstancePropertyValue_0",
                //
                @"@__city_InstancePropertyValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_InstancePropertyValue_0");
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            AssertSql(
                @"@__StaticFieldValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__StaticFieldValue_0",
                //
                @"@__StaticFieldValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__StaticFieldValue_0");
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            AssertSql(
                @"@__StaticPropertyValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__StaticPropertyValue_0",
                //
                @"@__StaticPropertyValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__StaticPropertyValue_0");
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            AssertSql(
                @"@__city_Nested_InstanceFieldValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_Nested_InstanceFieldValue_0",
                //
                @"@__city_Nested_InstanceFieldValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_Nested_InstanceFieldValue_0");
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            AssertSql(
                @"@__city_Nested_InstancePropertyValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_Nested_InstancePropertyValue_0",
                //
                @"@__city_Nested_InstancePropertyValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__city_Nested_InstancePropertyValue_0");
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            AssertSql(
                @"@__InstanceFieldValue_0='London'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__InstanceFieldValue_0",
                //
                @"@__InstanceFieldValue_0='Seattle'

SELECT c[""CustomerID""], c[""Address""], c[""City""], c[""CompanyName""], c[""ContactName""], c[""ContactTitle""], c[""Country""], c[""Fax""], c[""Phone""], c[""PostalCode""], c[""Region""]
FROM Customers c
WHERE c[""City""] = @__InstanceFieldValue_0");
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            AssertSql(
                @"@__reportsTo_0='2'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__reportsTo_0",
                //
                @"@__reportsTo_0='5'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__reportsTo_0",
                //
                @"@__reportsTo_0=''

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__reportsTo_0");
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            AssertSql(
                @"@__reportsTo_0=''

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__reportsTo_0",
                //
                @"@__reportsTo_0='5'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__reportsTo_0",
                //
                @"@__reportsTo_0='2'

SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""ReportsTo""] = @__reportsTo_0");
        }

        [ConditionalFact(Skip = "Convert to N+1.")]
        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            AssertSql(" ");
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            AssertSql(
                @"SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""Title""] = ""Sales Representative""");
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            AssertSql(
                @"SELECT c[""Title""]
FROM Employees c
WHERE c[""Title""] = ""Sales Representative""");
        }

        public override void Where_simple_shadow_projection_mixed()
        {
            base.Where_simple_shadow_projection_mixed();

            AssertSql(
                @"SELECT c[""EmployeeID""], c[""City""], c[""Country""], c[""FirstName""], c[""ReportsTo""], c[""Title""]
FROM Employees c
WHERE c[""Title""] = ""Sales Representative""");
        }

        public override void Where_simple_shadow_subquery()
        {
            base.Where_simple_shadow_subquery();

            AssertSql(" ");
        }

        public override void Where_shadow_subquery_FirstOrDefault()
        {
            base.Where_shadow_subquery_FirstOrDefault();

            AssertSql(" ");
        }

        public override void Where_client()
        {
            base.Where_client();

            AssertSql(" ");
        }

        public override void Where_subquery_correlated()
        {
            base.Where_subquery_correlated();

            AssertSql(" ");
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            AssertSql(" ");
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            AssertSql(" ");
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            AssertSql(" ");
        }

        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();

            AssertSql(" ");
        }

        public override void Where_comparison_to_nullable_bool()
        {
            base.Where_comparison_to_nullable_bool();

            AssertSql(" ");
        }

        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();

            AssertSql(" ");
        }

        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();

            AssertSql(" ");
        }

        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();

            AssertSql(" ");
        }

        public override void Where_bool_closure()
        {
            base.Where_bool_closure();

            AssertSql(" ");
        }

        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();

            AssertSql(" ");
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_true()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_true();

            AssertSql(" ");
        }

        public override void Where_navigation_contains()
        {
            base.Where_navigation_contains();

            AssertSql(" ");
        }
    }
}
