' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Option Compare Text
Imports Microsoft.EntityFrameworkCore.Diagnostics
Imports Microsoft.EntityFrameworkCore.TestModels.Northwind
Imports Xunit

Partial Public Class NorthwindQueryVisualBasicTest
    <ConditionalTheory>
    <MemberData(NameOf(IsAsyncData))>
    Public Function CompareString_Equals_Text(async As Boolean) As Task
        Return AssertTranslationFailedWithDetails(
            Function() AssertQuery(
                async,
                Function(ss) ss.Set(Of Customer).Where(Function(c) c.CustomerID = "ALFKI")),
            CoreStrings.QueryUnableToTranslateMethod("string", "Compare"))
    End Function

    <ConditionalTheory>
    <MemberData(NameOf(IsAsyncData))>
    Public Function CompareString_LessThanOrEqual_Text(async As Boolean) As Task
        Return AssertTranslationFailedWithDetails(
            Function() AssertQuery(
                async,
                Function(ss) ss.Set(Of Customer).Where(Function(c) c.CustomerID <= "ALFKI")),
            CoreStrings.QueryUnableToTranslateMethod("string", "Compare"))
    End Function
End Class
