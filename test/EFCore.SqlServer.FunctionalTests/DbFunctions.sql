IF object_id(N'[EmployeeOrderCount]', N'FN') IS NOT NULL
    DROP FUNCTION [EmployeeOrderCount]
GO

create function [dbo].[EmployeeOrderCount] (@employeeId int)
returns int
as
begin
	return (select count(orderId) from orders where employeeid = @employeeId);
end
GO


IF object_id(N'[IsTopEmployee]', N'FN') IS NOT NULL
    DROP FUNCTION [IsTopEmployee]
GO

create function [dbo].[IsTopEmployee] (@employeeId int)
returns bit
as
begin
	if(@employeeId = 4 or @employeeId = 5 or @employeeId = 8)
		return 1
		
	return 0
end
GO


IF object_id(N'[GetEmployeeWithMostOrdersAfterDate]', N'FN') IS NOT NULL
    DROP FUNCTION [GetEmployeeWithMostOrdersAfterDate]
GO

create function [dbo].[GetEmployeeWithMostOrdersAfterDate] (@searchDate Date)
returns int
as
begin
	return (select top 1 employeeId
			from orders
			where orderDate > @searchDate
			group by EmployeeID
			order by count(orderid) desc)
end
GO

IF object_id(N'[GetReportingPeriodStartDate]', N'FN') IS NOT NULL
    DROP FUNCTION [GetReportingPeriodStartDate]
GO

create function [dbo].[GetReportingPeriodStartDate] (@period int)
returns DateTime
as
begin
	return '1/1/1998'
end
GO


IF object_id(N'[StarValue]', N'FN') IS NOT NULL
    DROP FUNCTION [StarValue]
GO

create function [dbo].[StarValue] (@starCount int, @value nvarchar(max))
returns  nvarchar(max)
as
begin
	return replicate('*', @starCount) + @value
end
GO

IF object_id(N'[AddValues]', N'FN') IS NOT NULL
    DROP FUNCTION [AddValues]
GO

create function [dbo].[AddValues] (@a int, @b int)
returns  int
as
begin
	return @a + @b
end
GO

IF object_id(N'[GetBestYearEver]', N'FN') IS NOT NULL
    DROP FUNCTION [GetBestYearEver]
GO

create function [dbo].[GetBestYearEver] ()
returns datetime
as
begin
	return '1/1/1998'
end
GO


