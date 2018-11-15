<!-- default file list -->
*Files to look at*:

* **[HomeController.cs](./CS/SummaryCustomDataBinding/Controllers/HomeController.cs) (VB: [HomeController.vb](./VB/SummaryCustomDataBinding/Controllers/HomeController.vb))**
* [CustomBindingModel.cs](./CS/SummaryCustomDataBinding/Models/CustomBindingModel.cs) (VB: [CustomBindingModel.vb](./VB/SummaryCustomDataBinding/Models/CustomBindingModel.vb))
* [_GridViewPartial.cshtml](./CS/SummaryCustomDataBinding/Views/Home/_GridViewPartial.cshtml)
* [Index.cshtml](./CS/SummaryCustomDataBinding/Views/Home/Index.cshtml)
<!-- default file list end -->
# GridView - How to use a built-in Context Menu to display summaries in Custom Binding mode


<p>Having researched this scenario, we found that it's impossible to support it out of the box, since it requires implementing a separate action for the ContextMenu features. This example illustrates how to implement this scenario using custom callbacks: <br />1) Handle the <a href="https://documentation.devexpress.com/#AspNet/DevExpressWebASPxGridView_ContextMenuItemClicktopic">ASPxGridView.ContextMenuItemClick</a> event to intercept the default logic and send a custom callback using <a href="https://documentation.devexpress.com/#AspNet/DevExpressWebMVCScriptsMVCxClientGridView_PerformCallbacktopic">MVCxClientGridView.PerformCallback</a> : </p>


```js
function OnContextMenuItemClick(s, e) {
        if (e.objectType == "footer") {            
            e.handled = true;
            gridView.PerformCallback({ fieldName: s.GetColumn(e.elementIndex).fieldName, customCommand: e.item.name });            
        }
 }

```


<p> </p>
<p>2) Read callback parameters (see <a href="https://documentation.devexpress.com/#AspNet/CustomDocument9941">Passing Values to a Controller Action through Callbacks</a>) and manually modify the summary collection at the <a href="https://documentation.devexpress.com/#AspNet/clsDevExpressWebMvcGridViewModeltopic">GridViewModel</a> level using the <a href="https://documentation.devexpress.com/#AspNet/DevExpressWebMvcGridViewModel_TotalSummarytopic">GridViewModel.TotalSummary</a> property. <br /><br />3) Modify the <a href="https://documentation.devexpress.com/#AspNet/DevExpressWebMvcGridViewCustomBindingGetSummaryValuesHandlertopic">GridViewCustomBindingGetSummaryValuesHandler</a> method's implementation to support all summary types ("Min", "Max" and "Average") <br /><br /></p>
<p>Note that this approach will work for versions 14.2.8 and later (see <a href="https://www.devexpress.com/Support/Center/p/T238082">GridView - It's impossible to change the summary state on a custom callback in Custom Binding mode</a>).</p>

<br/>


