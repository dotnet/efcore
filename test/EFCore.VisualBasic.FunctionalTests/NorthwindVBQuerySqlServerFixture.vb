' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Imports Microsoft.EntityFrameworkCore.Infrastructure
Imports Microsoft.EntityFrameworkCore.Query

Public Class NorthwindVBQuerySqlServerFixture(Of TModelCustomizer As {ITestModelCustomizer, New})
    Inherits NorthwindQuerySqlServerFixture(Of TModelCustomizer)

    Protected Overrides ReadOnly Property StoreName As String
        Get
            Return "NorthwindVB"
        End Get
    End Property
End Class
