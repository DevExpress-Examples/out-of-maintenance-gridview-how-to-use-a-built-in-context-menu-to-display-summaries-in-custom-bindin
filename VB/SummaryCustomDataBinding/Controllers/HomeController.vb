Imports Microsoft.VisualBasic
Imports DevExpress.Data
Imports DevExpress.Web.Mvc
Imports SummaryCustomDataBinding.Models
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc
Imports System.Text.RegularExpressions
Namespace SummaryCustomDataBinding.Controllers
	Public Class HomeController
		Inherits Controller
		'
		' GET: /Home/

		Public Function Index() As ActionResult
			Return View()
		End Function
		Public Function AdvancedCustomBindingPartial(ByVal customCommand As String, ByVal fieldName As String) As ActionResult
			Dim viewModel = GridViewExtension.GetViewModel("gridView")

			If viewModel Is Nothing Then
				viewModel = CreateGridViewModelWithSummary()
			End If
			If (Not String.IsNullOrWhiteSpace(customCommand)) AndAlso (Not String.IsNullOrWhiteSpace(fieldName)) Then
				ModifySummaries(viewModel, customCommand, fieldName)
			End If
			Return AdvancedCustomBindingCore(viewModel)
		End Function
		Public Sub ModifySummaries(ByVal viewModel As GridViewModel, ByVal command As String, ByVal fieldName As String)
			Dim shortCommand As String = Regex.Match(command, "Summary(.*)").Groups(1).Value

			If shortCommand = "None" Then
				viewModel.TotalSummary.RemoveAll(Function(item) item.FieldName = fieldName)
				Return
			End If
			Dim result As SummaryItemType
			If System.Enum.TryParse(Of SummaryItemType)(shortCommand, result) Then
                If (Not viewModel.TotalSummary.Exists(Function(item) item.FieldName = fieldName AndAlso item.SummaryType = result)) Then
                    viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = fieldName, .SummaryType = result})
                Else
                    viewModel.TotalSummary.RemoveAll(Function(item) item.FieldName = fieldName AndAlso item.SummaryType = result)
                End If

			End If
		End Sub
		' Paging
		Public Function AdvancedCustomBindingPagingAction(ByVal pager As GridViewPagerState) As ActionResult
			Dim viewModel = GridViewExtension.GetViewModel("gridView")
			viewModel.ApplyPagingState(pager)
			Return AdvancedCustomBindingCore(viewModel)
		End Function
		' Filtering
		Public Function AdvancedCustomBindingFilteringAction(ByVal filteringState As GridViewFilteringState) As ActionResult
			Dim viewModel = GridViewExtension.GetViewModel("gridView")
			viewModel.ApplyFilteringState(filteringState)
			Return AdvancedCustomBindingCore(viewModel)
		End Function
		' Sorting
		Public Function AdvancedCustomBindingSortingAction(ByVal column As GridViewColumnState, ByVal reset As Boolean) As ActionResult
			Dim viewModel = GridViewExtension.GetViewModel("gridView")
			viewModel.ApplySortingState(column, reset)
			Return AdvancedCustomBindingCore(viewModel)
		End Function
		' Grouping
		Public Function AdvancedCustomBindingGroupingAction(ByVal column As GridViewColumnState) As ActionResult
			Dim viewModel = GridViewExtension.GetViewModel("gridView")
			viewModel.ApplyGroupingState(column)
			Return AdvancedCustomBindingCore(viewModel)
		End Function

		Private Function AdvancedCustomBindingCore(ByVal viewModel As GridViewModel) As PartialViewResult
            viewModel.ProcessCustomBinding(AddressOf GridViewCustomBindingHandlers.GetDataRowCountAdvanced, AddressOf GridViewCustomBindingHandlers.GetDataAdvanced, AddressOf GridViewCustomBindingHandlers.GetSummaryValuesAdvanced, AddressOf GridViewCustomBindingHandlers.GetGroupingInfoAdvanced, AddressOf GridViewCustomBindingHandlers.GetUniqueHeaderFilterValuesAdvanced)
			Return PartialView("_GridViewPartial", viewModel)
		End Function

		Private Shared Function CreateGridViewModelWithSummary() As GridViewModel
			Dim viewModel = New GridViewModel()
			viewModel.KeyFieldName = "OrderID"

			viewModel.Columns.Add("OrderID")
			viewModel.Columns.Add("EmployeeID")
			viewModel.Columns.Add("OrderDate")
			viewModel.Columns.Add("Freight")
			viewModel.Columns.Add("ShipName")
			viewModel.Columns.Add("ShipAddress")
			viewModel.Columns.Add("ShipCity")
			viewModel.Columns.Add("ShipCountry")

			viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = "OrderID", .SummaryType = SummaryItemType.Sum})
			viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = "EmployeeID", .SummaryType = SummaryItemType.Count})
			viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = "Freight", .SummaryType = SummaryItemType.Average})
			viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = "ShipCity", .SummaryType = SummaryItemType.Min})
			viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = "OrderDate", .SummaryType = SummaryItemType.Min})
			viewModel.TotalSummary.Add(New GridViewSummaryItemState() With {.FieldName = "Freight", .SummaryType = SummaryItemType.Count})

			Return viewModel
		End Function

	End Class

End Namespace
