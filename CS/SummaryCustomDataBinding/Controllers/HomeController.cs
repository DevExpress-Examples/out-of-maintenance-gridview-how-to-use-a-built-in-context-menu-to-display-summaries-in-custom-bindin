using DevExpress.Data;
using DevExpress.Web.Mvc;
using SummaryCustomDataBinding.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
namespace SummaryCustomDataBinding.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult AdvancedCustomBindingPartial(string customCommand, string fieldName)
        {
            var viewModel = GridViewExtension.GetViewModel("gridView");

            if (viewModel == null)
                viewModel = CreateGridViewModelWithSummary();
            if (!String.IsNullOrWhiteSpace(customCommand) && !String.IsNullOrWhiteSpace(fieldName))
                ModifySummaries(viewModel, customCommand, fieldName);
            return AdvancedCustomBindingCore(viewModel);
        }
        public void ModifySummaries(GridViewModel viewModel, string command, string fieldName)
        {
            string shortCommand = Regex.Match(command, "Summary(.*)").Groups[1].Value;
            
            if (shortCommand == "None")
            {
                viewModel.TotalSummary.RemoveAll(item => item.FieldName == fieldName);
                return;
            }
            SummaryItemType result;
            if (Enum.TryParse<SummaryItemType>(shortCommand, out result))
            {
                if (!viewModel.TotalSummary.Exists(item => item.FieldName == fieldName && item.SummaryType == result))
                {
                    viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = fieldName, SummaryType = result });
                }
                else
                {
                    viewModel.TotalSummary.RemoveAll(item => item.FieldName == fieldName && item.SummaryType == result);
                }

            }
        }
        // Paging
        public ActionResult AdvancedCustomBindingPagingAction(GridViewPagerState pager)
        {
            var viewModel = GridViewExtension.GetViewModel("gridView");
            viewModel.ApplyPagingState(pager);
            return AdvancedCustomBindingCore(viewModel);
        }
        // Filtering
        public ActionResult AdvancedCustomBindingFilteringAction(GridViewFilteringState filteringState)
        {
            var viewModel = GridViewExtension.GetViewModel("gridView");
            viewModel.ApplyFilteringState(filteringState);
            return AdvancedCustomBindingCore(viewModel);
        }
        // Sorting
        public ActionResult AdvancedCustomBindingSortingAction(GridViewColumnState column, bool reset)
        {
            var viewModel = GridViewExtension.GetViewModel("gridView");
            viewModel.ApplySortingState(column, reset);
            return AdvancedCustomBindingCore(viewModel);
        }
        // Grouping
        public ActionResult AdvancedCustomBindingGroupingAction(GridViewColumnState column)
        {
            var viewModel = GridViewExtension.GetViewModel("gridView");
            viewModel.ApplyGroupingState(column);
            return AdvancedCustomBindingCore(viewModel);
        }

        PartialViewResult AdvancedCustomBindingCore(GridViewModel viewModel)
        {
            viewModel.ProcessCustomBinding(
                GridViewCustomBindingHandlers.GetDataRowCountAdvanced,
                GridViewCustomBindingHandlers.GetDataAdvanced,
                GridViewCustomBindingHandlers.GetSummaryValuesAdvanced,
                GridViewCustomBindingHandlers.GetGroupingInfoAdvanced,
                GridViewCustomBindingHandlers.GetUniqueHeaderFilterValuesAdvanced
            );
            return PartialView("_GridViewPartial", viewModel);
        }

        static GridViewModel CreateGridViewModelWithSummary()
        {
            var viewModel = new GridViewModel();
            viewModel.KeyFieldName = "OrderID";

            viewModel.Columns.Add("OrderID");
            viewModel.Columns.Add("EmployeeID");
            viewModel.Columns.Add("OrderDate");
            viewModel.Columns.Add("Freight");
            viewModel.Columns.Add("ShipName");
            viewModel.Columns.Add("ShipAddress");
            viewModel.Columns.Add("ShipCity");
            viewModel.Columns.Add("ShipCountry");

            viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = "OrderID", SummaryType = SummaryItemType.Sum });
            viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = "EmployeeID", SummaryType = SummaryItemType.Count });
            viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = "Freight", SummaryType = SummaryItemType.Average });
            viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = "ShipCity", SummaryType = SummaryItemType.Min });
            viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = "OrderDate", SummaryType = SummaryItemType.Min });
            viewModel.TotalSummary.Add(new GridViewSummaryItemState() { FieldName = "Freight", SummaryType = SummaryItemType.Count });
           
            return viewModel;
        }

    }

}
