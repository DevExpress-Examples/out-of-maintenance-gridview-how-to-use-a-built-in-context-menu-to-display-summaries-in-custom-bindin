Imports Microsoft.VisualBasic
Imports DevExpress.Data
Imports DevExpress.Data.Filtering
Imports DevExpress.Data.Linq
Imports DevExpress.Data.Linq.Helpers

Imports DevExpress.Web.Mvc
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Linq.Expressions

Namespace SummaryCustomDataBinding.Models
	Public NotInheritable Class GridViewCustomBindingHandlers
		Private Sub New()
		End Sub
		Private Shared ReadOnly Property Model() As IQueryable
			Get
				Return New NorthwindDataClassesDataContext().Orders
			End Get
		End Property

		Public Shared Sub GetDataRowCountSimple(ByVal e As GridViewCustomBindingGetDataRowCountArgs)
			e.DataRowCount = Model.Count()
		End Sub
		Public Shared Sub GetDataSimple(ByVal e As GridViewCustomBindingGetDataArgs)
			e.Data = Model.ApplySorting(e.State.SortedColumns).Skip(e.StartDataRowIndex).Take(e.DataRowCount)
		End Sub

		Public Shared Sub GetDataRowCountAdvanced(ByVal e As GridViewCustomBindingGetDataRowCountArgs)
			e.DataRowCount = Model.ApplyFilter(e.FilterExpression).Count()
		End Sub
		Public Shared Sub GetUniqueHeaderFilterValuesAdvanced(ByVal e As GridViewCustomBindingGetUniqueHeaderFilterValuesArgs)
			e.Data = Model.ApplyFilter(e.FilterExpression).UniqueValuesForField(e.FieldName)
		End Sub
		Public Shared Sub GetGroupingInfoAdvanced(ByVal e As GridViewCustomBindingGetGroupingInfoArgs)
			e.Data = Model.ApplyFilter(e.State.AppliedFilterExpression).ApplyFilter(e.GroupInfoList).GetGroupInfo(e.FieldName, e.SortOrder)
		End Sub
		Public Shared Sub GetDataAdvanced(ByVal e As GridViewCustomBindingGetDataArgs)
			e.Data = Model.ApplyFilter(e.State.AppliedFilterExpression).ApplyFilter(e.GroupInfoList).ApplySorting(e.State.SortedColumns).Skip(e.StartDataRowIndex).Take(e.DataRowCount)
		End Sub
		Public Shared Sub GetSummaryValuesAdvanced(ByVal e As GridViewCustomBindingGetSummaryValuesArgs)
			Dim query = Model.ApplyFilter(e.State.AppliedFilterExpression).ApplyFilter(e.GroupInfoList)
			Dim list = New ArrayList()
			For Each item In e.SummaryItems

				Select Case item.SummaryType
					Case SummaryItemType.Count
						list.Add(query.Count())
					Case Else
						Dim summaryString As String = System.Enum.GetName(GetType(SummaryItemType), item.SummaryType)
						list.Add(query.CountSummary(item.FieldName, summaryString))
				End Select
			Next item
			e.Data = list
		End Sub
	End Class

	Public Module GridViewCustomOperationDataHelper
       
        Private ReadOnly Property Converter() As ICriteriaToExpressionConverter
            Get
                Return New CriteriaToExpressionConverter()
            End Get
        End Property

        <System.Runtime.CompilerServices.Extension> _
        Public Function [Select](ByVal query As IQueryable, ByVal fieldName As String) As IQueryable
            Return query.MakeSelect(Converter, New OperandProperty(fieldName))
        End Function

        <System.Runtime.CompilerServices.Extension> _
        Public Function ApplySorting(ByVal query As IQueryable, ByVal sortedColumns As IEnumerable(Of GridViewColumnState)) As IQueryable
            Dim orderDescriptors() As ServerModeOrderDescriptor = sortedColumns.Select(Function(c) New ServerModeOrderDescriptor(New OperandProperty(c.FieldName), c.SortOrder = ColumnSortOrder.Descending)).ToArray()
            Return query.MakeOrderBy(Converter, orderDescriptors)
        End Function

        <System.Runtime.CompilerServices.Extension> _
        Public Function ApplyFilter(ByVal query As IQueryable, ByVal groupInfoList As IList(Of GridViewGroupInfo)) As IQueryable
            Dim criteria = GroupOperator.And(groupInfoList.Select(Function(i) New BinaryOperator(i.FieldName, i.KeyValue, BinaryOperatorType.Equal)))
            Return query.ApplyFilter(CriteriaOperator.ToString(criteria))
        End Function
        <System.Runtime.CompilerServices.Extension> _
        Public Function ApplyFilter(ByVal query As IQueryable, ByVal filterExpression As String) As IQueryable
            Return query.AppendWhere(Converter, CriteriaOperator.Parse(filterExpression))
        End Function

        <System.Runtime.CompilerServices.Extension> _
        Public Function GetGroupInfo(ByVal query As IQueryable, ByVal fieldName As String, ByVal order As ColumnSortOrder) As IEnumerable(Of GridViewGroupInfo)
            Dim rowType = query.ElementType
            query = query.MakeGroupBy(Converter, New OperandProperty(fieldName))
            Dim result As Boolean = order = ColumnSortOrder.Descending
            query = query.MakeOrderBy(Converter, New ServerModeOrderDescriptor(New OperandProperty("Key"), result))
            query = ApplyExpression(query, rowType, "Key", "Count")

            Dim list = New List(Of GridViewGroupInfo)()
            For Each item In query
                Dim obj = CType(item, Object())
                list.Add(New GridViewGroupInfo() With {.KeyValue = obj(0), .DataRowCount = CInt(Fix(obj(1)))})
            Next item
            Return list
        End Function
        Private Function ApplyExpression(ByVal query As IQueryable, ByVal rowType As Type, ByVal ParamArray names() As String) As IQueryable
            Dim parameter = Expressions.Expression.Parameter(query.ElementType, String.Empty)
            Dim myExpressions = names.Select(Function(n) query.GetExpression(n, rowType, parameter))
            Dim arrayExpressions = Expressions.Expression.NewArrayInit(GetType(Object), myExpressions.Select(Function(expr) Expressions.Expression.Convert(expr, GetType(Object))).ToArray())
            Dim lambda = Expressions.Expression.Lambda(arrayExpressions, parameter)

            Dim expression = Expressions.Expression.Call(GetType(Queryable), "Select", New Type() {query.ElementType, lambda.Body.Type}, query.Expression, Expressions.Expression.Quote(lambda))
            Return query.Provider.CreateQuery(expression)
        End Function
        <System.Runtime.CompilerServices.Extension> _
        Private Function GetExpression(ByVal query As IQueryable, ByVal commandName As String, ByVal rowType As Type, ByVal parameter As ParameterExpression) As Expression
            Select Case commandName
                Case "Key"
                    Return Expression.Property(parameter, "Key")
                Case "Count"
                    Return Expression.Call(GetType(Enumerable), "Count", New Type() {rowType}, parameter)
            End Select
            Return Nothing
        End Function

        <System.Runtime.CompilerServices.Extension> _
        Public Function CountSummary(ByVal query As IQueryable, ByVal fieldName As String, ByVal summaryType As String) As Object
            If query.Count() = 0 Then
                Return 0
            End If

            Dim parameter = Expressions.Expression.Parameter(query.ElementType, String.Empty)
            Dim propertyInfo = query.ElementType.GetProperty(fieldName)
            Dim propertyAccess = Expressions.Expression.MakeMemberAccess(parameter, propertyInfo)
            Dim propertyAccessExpression = Expressions.Expression.Lambda(propertyAccess, parameter)
            Dim expression As MethodCallExpression = Nothing
            If summaryType = "Min" OrElse summaryType = "Max" Then
                expression = Expressions.Expression.Call(GetType(Queryable), summaryType, New Type() {query.ElementType, propertyAccessExpression.Body.Type}, query.Expression, propertyAccessExpression)
            Else
                expression = Expressions.Expression.Call(GetType(Queryable), summaryType, New Type() {query.ElementType}, query.Expression, Expressions.Expression.Quote(propertyAccessExpression))
            End If
            Return query.Provider.Execute(expression)
        End Function

        <System.Runtime.CompilerServices.Extension> _
        Public Function UniqueValuesForField(ByVal query As IQueryable, ByVal fieldName As String) As IQueryable
            query = query.Select(fieldName)
            Dim expression = Expressions.Expression.Call(GetType(Queryable), "Distinct", New Type() {query.ElementType}, query.Expression)
            Return query.Provider.CreateQuery(expression)
        End Function
	End Module
End Namespace