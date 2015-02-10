﻿using MVCGrid.Models;
using MVCGrid.Web;
using MVCGrid.Web.Data;
using MVCGrid.Web.Models;
using MVCGridExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCGridExample
{
    public class MVCGridConfig
    {
        public static void RegisterMappings()
        {
            GridConfiguration globalConfig = SetupGlobalConfiguration();

            var grid = new MVCGridBuilder<TestItem>()
                .AddColumn("Col1", "Col1", ((p, h) => p.Col1),
                    cellCssClassExpression: ((p, c) =>
                    {
                        if (p.Col1 == "Row3")
                        {
                            return "success";
                        }
                        return null;
                    }))
                .AddColumn("Col2", "Col2", ((p, h) => p.Col2))
                .AddColumn(
                    name: "Col3",
                    headerText: "Column3",
                    valueExpression: ((p, h) => String.Format("<a href='{1}'>{0}</a>", p.Col3, h.UrlHelper.Action("detail", "item", new { id = "test" }))),
                    enableSort: false,
                    htmlEncode: false,
                    plainTextValueExpression: ((p, c) => p.Col3))
                .WithRetrieveDataMethod(((options) =>
                {
                    TestItemRepository repo = new TestItemRepository();
                    int totalRecords;
                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(), options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    items = new List<TestItem>();
                    totalRecords = 0;

                    return new QueryResult<TestItem>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                }))
                .WithPreloadData(true)
                .WithRowCssClassExpression((p, h) =>
                    {
                        if (p.Col1 == "Row1")
                        {
                            return "success";
                        }
                        return null;
                    });
            MVCGridMappingTable.Add("TestMapping", grid);


            MVCGridMappingTable.Add("TestGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(true)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) =>
                        {
                            return String.Format("<a href='{0}'>{1}</a>",
                                c.UrlHelper.Action("detail", "demo", new { id = p.Id }), p.Id);
                        });
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add().WithColumnName("StartDate")
                        .WithHeaderText("Start Date")
                        .WithValueExpression((p, c) => p.StartDate.HasValue ? p.StartDate.Value.ToShortDateString() : "");
                    cols.Add().WithColumnName("Status")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive")
                        .WithCellCssClassExpression((p, c) => p.Active ? "success" : "danger"); ;
                    cols.Add().WithColumnName("Gender")
                        .WithValueExpression((p, c) => p.Gender);

                })
                .WithSorting(true)
                .WithDefaultSortColumn("Id")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithPreloadData(true)
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.SortColumn;
                    if (String.Compare(sortColumn, "status", true) == 0) sortColumn = "active";

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridMappingTable.Add("EmployeeGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithRetrieveDataMethod((options) =>
                {
                    var result = new QueryResult<Person>();

                    using (var db = new SampleDatabaseEntities())
                    {
                        result.Items = db.People.Where(p => p.Employee).ToList();
                    }

                    return result;
                })
            );

            MVCGridMappingTable.Add("SortableGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithRetrieveDataMethod((options) =>
                {
                    var result = new QueryResult<Person>();

                    using (var db = new SampleDatabaseEntities())
                    {
                        var query = db.People.Where(p => p.Employee);

                        if (!String.IsNullOrWhiteSpace(options.SortColumn))
                        {
                            switch (options.SortColumn.ToLower())
                            {
                                case "firstname":
                                    if (options.SortDirection == SortDirection.Asc)
                                        query = query.OrderBy(p => p.FirstName);
                                    else
                                        query = query.OrderByDescending(p => p.FirstName);
                                    break;
                                case "lastname":
                                    if (options.SortDirection == SortDirection.Asc)
                                        query = query.OrderBy(p => p.LastName);
                                    else
                                        query = query.OrderByDescending(p => p.LastName);
                                    break;
                            }
                        }

                        result.Items = query.ToList();
                    }

                    return result;
                })
            );

            MVCGridMappingTable.Add("PagingGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((options) =>
                {
                    var result = new QueryResult<Person>();

                    using (var db = new SampleDatabaseEntities())
                    {
                        var query = db.People.AsQueryable();

                        result.TotalRecords = query.Count();

                        if (!String.IsNullOrWhiteSpace(options.SortColumn))
                        {
                            switch (options.SortColumn.ToLower())
                            {
                                case "firstname":
                                    if (options.SortDirection == SortDirection.Asc)
                                        query = query.OrderBy(p => p.FirstName);
                                    else
                                        query = query.OrderByDescending(p => p.FirstName);
                                    break;
                                case "lastname":
                                    if (options.SortDirection == SortDirection.Asc)
                                        query = query.OrderBy(p => p.LastName);
                                    else
                                        query = query.OrderByDescending(p => p.LastName);
                                    break;
                            }
                        }

                        if (options.GetLimitOffset().HasValue)
                        {
                            query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
                        }

                        result.Items = query.ToList();
                    }

                    return result;
                })
            );

            MVCGridMappingTable.Add("DIGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridMappingTable.Add("FormattingGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add().WithColumnName("StartDate")
                        .WithHeaderText("Start Date")
                        .WithValueExpression((p, c) => p.StartDate.HasValue ? p.StartDate.Value.ToShortDateString() : "");
                    cols.Add().WithColumnName("ViewLink")
                        .WithSorting(false)
                        .WithHeaderText("")
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => {
                            return String.Format("<a href='{0}'>View</a>",
                                c.UrlHelper.Action("detail", "demo", new { id = p.Id }));
                            });
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridMappingTable.Add("StyledGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add().WithColumnName("StartDate")
                        .WithHeaderText("Start Date")
                        .WithValueExpression((p, c) => p.StartDate.HasValue ? p.StartDate.Value.ToShortDateString() : "");
                    cols.Add().WithColumnName("Status")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                    cols.Add().WithColumnName("Gender")
                        .WithValueExpression((p, c)=> p.Gender)
                        .WithCellCssClassExpression((p, c) => p.Gender == "Female" ? "danger" : "warning");
                    cols.Add().WithColumnName("ViewLink")
                        .WithSorting(false)
                        .WithHeaderText("")
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) =>
                        {
                            return String.Format("<a href='{0}'>View</a>",
                                c.UrlHelper.Action("detail", new { id = p.Id }));
                        });
                })
                .WithRowCssClassExpression((p, c) => p.Active ? "success" : "")
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridMappingTable.Add("Preloading", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithPreloadData(true)
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridMappingTable.Add("CustomLoading", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithClientSideLoadingMessageFunctionName("showLoading")
                .WithClientSideLoadingCompleteFunctionName("hideLoading")
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    // pause to test loading message
                    System.Threading.Thread.Sleep(1000);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridMappingTable.Add("Filtering", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName)
                        .WithFiltering(true);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName)
                        .WithFiltering(true);
                    cols.Add().WithColumnName("Status")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive")
                        .WithFiltering(true);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithFiltering(true)
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    bool? active = null;
                    string fa=options.GetFilterString("Status");
                    if (!String.IsNullOrWhiteSpace(fa))
                    {
                        if (String.Compare(fa, "active", true) == 0)
                        {
                            active = true;
                        }
                        else
                        {
                            active = false;
                        }
                    }

                    var items = repo.GetData(out totalRecords,
                        options.GetFilterString("FirstName"),
                        options.GetFilterString("LastName"),
                        active,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridMappingTable.Add("ExportGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add().WithColumnName("Id")
                        .WithSorting(false)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) =>
                        {
                            return String.Format("<a href='{0}'>{1}</a>",
                                c.UrlHelper.Action("detail", "demo", new { id = p.Id }), p.Id);
                        })
                        .WithPlainTextValueExpression((p, c) => p.Id.ToString());
                    cols.Add().WithColumnName("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add().WithColumnName("LastName")
                        .WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add().WithColumnName("Status")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithClientSideLoadingMessageFunctionName("showLoading")
                .WithClientSideLoadingCompleteFunctionName("hideLoading")
                .WithRetrieveDataMethod((options) =>
                {
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            //MVCGridMappingTable.Add DO NOT DELETE - Needed for demo code parsing
        }

        private static GridConfiguration SetupGlobalConfiguration()
        {
            GridConfiguration globalConfig = new GridConfiguration();
            globalConfig.ItemsPerPage = 10;
            globalConfig.TableCssClasses = new HashSet<string>() { "table", "table-striped", "table-bordered" };
            return globalConfig;
        }
    }
}