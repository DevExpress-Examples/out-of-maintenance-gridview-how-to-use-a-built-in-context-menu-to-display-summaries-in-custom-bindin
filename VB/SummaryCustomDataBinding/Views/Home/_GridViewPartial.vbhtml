@Code
    Dim grid = Html.DevExpress().GridView(Sub(settings)
    
        settings.Name = "gridView"
        settings.CallbackRouteValues = new with { .Controller = "Home", .Action = "AdvancedCustomBindingPartial" }

        settings.CustomBindingRouteValuesCollection.Add(
           GridViewOperationType.Paging,
           new With { .Controller = "Home", .Action = "AdvancedCustomBindingPagingAction" }
       )
        settings.CustomBindingRouteValuesCollection.Add(
            GridViewOperationType.Filtering,
            new With { .Controller = "Home", .Action = "AdvancedCustomBindingFilteringAction" }
        )
        settings.CustomBindingRouteValuesCollection.Add(
            GridViewOperationType.Sorting,
            new  With { .Controller = "Home", .Action = "AdvancedCustomBindingSortingAction" }
        )
        settings.CustomBindingRouteValuesCollection.Add(
            GridViewOperationType.Grouping,
            new With { .Controller = "Home", .Action = "AdvancedCustomBindingGroupingAction" }
        )
        settings.KeyFieldName = "OrderID"

        settings.SettingsPager.Visible = true
        settings.Settings.ShowGroupPanel = true
        settings.Settings.ShowFilterRow = true
        settings.SettingsBehavior.AllowSelectByRowClick = true
        settings.ClientSideEvents.ContextMenuItemClick = "OnContextMenuItemClick"
        settings.Columns.Add("OrderID")
        settings.Columns.Add("EmployeeID")
        settings.Columns.Add(Sub(col)

            col.FieldName = "OrderDate"
            col.PropertiesEdit.DisplayFormatString = "MM/dd/yyyy"
        End Sub)
        settings.Columns.Add(Sub(col)

            col.FieldName = "Freight"
            col.PropertiesEdit.DisplayFormatString = "N2"
        End Sub)
        settings.Columns.Add("ShipName")
        settings.Columns.Add("ShipAddress")
        settings.Columns.Add("ShipCity")
        settings.Columns.Add("ShipCountry")
        settings.Settings.ShowHeaderFilterButton = true
        settings.Settings.ShowFooter = true
       
        settings.SettingsContextMenu.Enabled = true
        settings.SettingsContextMenu.EnableRowMenu = DefaultBoolean.False
    End Sub)
End Code
@grid.BindToCustomData(Model).GetHtml()