using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Data.Linq;
using DevExpress.Data.Linq.Helpers;

using DevExpress.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SummaryCustomDataBinding.Models
{
    public static class GridViewCustomBindingHandlers
    {
        static IQueryable Model { get { return new NorthwindDataClassesDataContext().Orders; } }

        public static void GetDataRowCountSimple(GridViewCustomBindingGetDataRowCountArgs e)
        {
            e.DataRowCount = Model.Count();
        }
        public static void GetDataSimple(GridViewCustomBindingGetDataArgs e)
        {
            e.Data = Model
                .ApplySorting(e.State.SortedColumns)
                .Skip(e.StartDataRowIndex)
                .Take(e.DataRowCount);
        }

        public static void GetDataRowCountAdvanced(GridViewCustomBindingGetDataRowCountArgs e)
        {
            e.DataRowCount = Model
                .ApplyFilter(e.FilterExpression)
                .Count();
        }
        public static void GetUniqueHeaderFilterValuesAdvanced(GridViewCustomBindingGetUniqueHeaderFilterValuesArgs e)
        {
            e.Data = Model
                .ApplyFilter(e.FilterExpression)
                .UniqueValuesForField(e.FieldName);
        }
        public static void GetGroupingInfoAdvanced(GridViewCustomBindingGetGroupingInfoArgs e)
        {
            e.Data = Model
                .ApplyFilter(e.State.AppliedFilterExpression)
                .ApplyFilter(e.GroupInfoList)
                .GetGroupInfo(e.FieldName, e.SortOrder);
        }
        public static void GetDataAdvanced(GridViewCustomBindingGetDataArgs e)
        {
            e.Data = Model
                .ApplyFilter(e.State.AppliedFilterExpression)
                .ApplyFilter(e.GroupInfoList)
                .ApplySorting(e.State.SortedColumns)
                .Skip(e.StartDataRowIndex)
                .Take(e.DataRowCount);
        }
        public static void GetSummaryValuesAdvanced(GridViewCustomBindingGetSummaryValuesArgs e)
        {
            var query = Model
                .ApplyFilter(e.State.AppliedFilterExpression)
                .ApplyFilter(e.GroupInfoList);
            var list = new ArrayList();
            foreach (var item in e.SummaryItems)
            {

                switch (item.SummaryType)
                {
                    case SummaryItemType.Count:
                        list.Add(query.Count());
                        break;
                    default:
                        string summaryString = Enum.GetName(typeof(SummaryItemType), item.SummaryType);
                        list.Add(query.CountSummary(item.FieldName, summaryString));
                        break;
                }
            }
            e.Data = list;
        }
    }

    public static class GridViewCustomOperationDataHelper
    {
        static ICriteriaToExpressionConverter Converter { get { return new CriteriaToExpressionConverter(); } }

        public static IQueryable Select(this IQueryable query, string fieldName)
        {
            return query.MakeSelect(Converter, new OperandProperty(fieldName));
        }

        public static IQueryable ApplySorting(this IQueryable query, IEnumerable<GridViewColumnState> sortedColumns)
        {
            ServerModeOrderDescriptor[] orderDescriptors = sortedColumns
                .Select(c => new ServerModeOrderDescriptor(new OperandProperty(c.FieldName), c.SortOrder == ColumnSortOrder.Descending))
                .ToArray();
            return query.MakeOrderBy(Converter, orderDescriptors);
        }

        public static IQueryable ApplyFilter(this IQueryable query, IList<GridViewGroupInfo> groupInfoList)
        {
            var criteria = GroupOperator.And(
                groupInfoList.Select(i => new BinaryOperator(i.FieldName, i.KeyValue, BinaryOperatorType.Equal))
            );
            return query.ApplyFilter(CriteriaOperator.ToString(criteria));
        }
        public static IQueryable ApplyFilter(this IQueryable query, string filterExpression)
        {
            return query.AppendWhere(Converter, CriteriaOperator.Parse(filterExpression));
        }

        public static IEnumerable<GridViewGroupInfo> GetGroupInfo(this IQueryable query, string fieldName, ColumnSortOrder order)
        {
            var rowType = query.ElementType;
            query = query.MakeGroupBy(Converter, new OperandProperty(fieldName));
            query = query.MakeOrderBy(Converter, new ServerModeOrderDescriptor(new OperandProperty("Key"), order == ColumnSortOrder.Descending));
            query = ApplyExpression(query, rowType, "Key", "Count");

            var list = new List<GridViewGroupInfo>();
            foreach (var item in query)
            {
                var obj = (object[])item;
                list.Add(new GridViewGroupInfo() { KeyValue = obj[0], DataRowCount = (int)obj[1] });
            }
            return list;
        }
        static IQueryable ApplyExpression(IQueryable query, Type rowType, params string[] names)
        {
            var parameter = Expression.Parameter(query.ElementType, string.Empty);
            var expressions = names.Select(n => query.GetExpression(n, rowType, parameter));
            var arrayExpressions = Expression.NewArrayInit(
                typeof(object),
                expressions.Select(expr => Expression.Convert(expr, typeof(object))).ToArray()
            );
            var lambda = Expression.Lambda(arrayExpressions, parameter);

            var expression = Expression.Call(
                typeof(Queryable),
                "Select",
                new Type[] { query.ElementType, lambda.Body.Type },
                query.Expression,
                Expression.Quote(lambda)
            );
            return query.Provider.CreateQuery(expression);
        }
        static Expression GetExpression(this IQueryable query, string commandName, Type rowType, ParameterExpression parameter)
        {
            switch (commandName)
            {
                case "Key":
                    return Expression.Property(parameter, "Key");
                case "Count":
                    return Expression.Call(typeof(Enumerable), "Count", new Type[] { rowType }, parameter);
            }
            return null;
        }

        public static object CountSummary(this IQueryable query, string fieldName, string summaryType)
        {
            if (query.Count() == 0)
                return 0;

            var parameter = Expression.Parameter(query.ElementType, string.Empty);
            var propertyInfo = query.ElementType.GetProperty(fieldName);
            var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
            var propertyAccessExpression = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression expression = null;
            if (summaryType == "Min" || summaryType == "Max")
                expression = Expression.Call(typeof(Queryable), summaryType,
                    new Type[] { query.ElementType, propertyAccessExpression.Body.Type },
                    query.Expression,
                    propertyAccessExpression);
            else
                expression = Expression.Call(
                   typeof(Queryable),
                   summaryType,
                   new Type[] { query.ElementType },
                   query.Expression,
                   Expression.Quote(propertyAccessExpression)
               );
            return query.Provider.Execute(expression);
        }

        public static IQueryable UniqueValuesForField(this IQueryable query, string fieldName)
        {
            query = query.Select(fieldName);
            var expression = Expression.Call(
                typeof(Queryable),
                "Distinct",
                new Type[] { query.ElementType },
                query.Expression
            );
            return query.Provider.CreateQuery(expression);
        }
    }
}