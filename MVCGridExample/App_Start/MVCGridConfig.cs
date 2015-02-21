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
        public static void RegisterGrids()
        {
            MVCGridDefinitionTable.Add("TestGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(true)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>{Model.Id}</a>")
                        .WithPlainTextValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("StartDate").WithHeaderText("Start Date")
                        .WithValueExpression((p, c) => p.StartDate.HasValue ? p.StartDate.Value.ToShortDateString() : "");
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive")
                        .WithCellCssClassExpression((p, c) => p.Active ? "success" : "danger");
                    cols.Add("Gender").WithValueExpression((p, c) => p.Gender);
                    cols.Add("Url").WithVisibility(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }));
                    //                    cols.Add().WithColumnName("Button")  //Templating demo
                    //                        .WithHtmlEncoding(false)
                    //                        .WithValueTemplate(@"
                    //<a class='btn btn-default' href='{Row.Url}' role='button'>
                    //    {Model.FirstName}
                    //</a>
                    //");

                })
                //.WithAdditionalSetting(MVCGrid.Rendering.BootstrapRenderingEngine.SettingNameTableClass, "notreal") // Excample of changing table css class
                .WithSorting(true)
                .WithDefaultSortColumn("Id")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithPreloadData(true)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("EmployeeGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
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

            MVCGridDefinitionTable.Add("SortableGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;
                    var result = new QueryResult<Person>();

                    using (var db = new SampleDatabaseEntities())
                    {
                        var query = db.People.Where(p => p.Employee);

                        if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                        {
                            switch (options.SortColumnName.ToLower())
                            {
                                case "firstname":
                                    query = query.OrderBy(p => p.FirstName, options.SortDirection);
                                    break;
                                case "lastname":
                                    query = query.OrderBy(p => p.LastName, options.SortDirection);
                                    break;
                            }
                        }

                        result.Items = query.ToList();
                    }

                    return result;
                })
            );

            MVCGridDefinitionTable.Add("PagingGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    var result = new QueryResult<Person>();

                    using (var db = new SampleDatabaseEntities())
                    {
                        var query = db.People.AsQueryable();

                        result.TotalRecords = query.Count();

                        if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                        {
                            switch (options.SortColumnName.ToLower())
                            {
                                case "firstname":
                                    query = query.OrderBy(p => p.FirstName, options.SortDirection);
                                    break;
                                case "lastname":
                                    query = query.OrderBy(p => p.LastName, options.SortDirection);
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

            MVCGridDefinitionTable.Add("DIGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumnName, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("FormattingGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("StartDate").WithHeaderText("Start Date")
                        .WithValueExpression((p, c) => p.StartDate.HasValue ? p.StartDate.Value.ToShortDateString() : "");
                    cols.Add("ViewLink").WithSorting(false)
                        .WithHeaderText("")
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>View</a>");
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumnName, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("StyledGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("StartDate").WithHeaderText("Start Date")
                        .WithValueExpression((p, c) => p.StartDate.HasValue ? p.StartDate.Value.ToShortDateString() : "");
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                    cols.Add("Gender").WithValueExpression((p, c) => p.Gender)
                        .WithCellCssClassExpression((p, c) => p.Gender == "Female" ? "danger" : "warning");
                    cols.Add().WithColumnName("ViewLink")
                        .WithSorting(false)
                        .WithHeaderText("")
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>View</a>");
                })
                .WithRowCssClassExpression((p, c) => p.Active ? "success" : "")
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("Preloading", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithPreloadData(false)
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.GetSortColumnData<string>(), options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("CustomLoading", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName")
                        .WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithClientSideLoadingMessageFunctionName("showLoading")
                .WithClientSideLoadingCompleteFunctionName("hideLoading")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.GetSortColumnData<string>(), options.SortDirection == SortDirection.Dsc);

                    // pause to test loading message
                    System.Threading.Thread.Sleep(1000);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("Filtering", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName)
                        .WithFiltering(true);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName)
                        .WithFiltering(true);
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive")
                        .WithFiltering(true);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithFiltering(true)
                .WithAllowChangingPageSize(true)
                .WithMaxItemsPerPage(100)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    bool? active = null;
                    string fa = options.GetFilterString("Status");
                    if (!String.IsNullOrWhiteSpace(fa))
                    {
                        active = (String.Compare(fa, "active", true) == 0);
                    }

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords,
                        options.GetFilterString("FirstName"),
                        options.GetFilterString("LastName"),
                        active,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("ExportGrid", new MVCGridBuilder<Person>()
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
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithClientSideLoadingMessageFunctionName("showLoading")
                .WithClientSideLoadingCompleteFunctionName("hideLoading")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("Multiple1", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>{Model.Id}</a>")
                        .WithPlainTextValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithQueryStringPrefix("grid1")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("Multiple2", new MVCGridBuilder<TestItem>()
                .AddColumns(cols =>
                {
                    cols.Add("Col1").WithValueExpression((p, c) => p.Col1);
                    cols.Add("Col2").WithValueExpression((p, c) => p.Col2);
                    cols.Add("Col3").WithValueExpression((p, c) => p.Col3);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("Col1")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithQueryStringPrefix("grid2")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    TestItemRepository repo = new TestItemRepository();
                    int totalRecords;
                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(), options.GetSortColumnData<string>(), options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<TestItem>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("CustomStyle", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>{Model.Id}</a>")
                        .WithPlainTextValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                })
                .WithRenderingEngine(typeof(CustomHtmlRenderingEngine))
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(20)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("CustomRazorView", new MVCGridBuilder<Person>()
                .WithRenderingMode(RenderingMode.Controller)
                .WithViewPath("~/Views/MVCGrid/_Custom.cshtml")
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>{Model.Id}</a>")
                        .WithPlainTextValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("Status").WithSortColumnData("Active")
                        .WithHeaderText("Status")
                        .WithValueExpression((p, c) => p.Active ? "Active" : "Inactive");
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(20)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("ValueTemplate", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithHtmlEncoding(false)
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}'>{Model.Id}</a>")
                        .WithPlainTextValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                    cols.Add("Edit").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}' class='btn btn-primary' role='button'>Edit</a>");
                    cols.Add("Delete").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => c.UrlHelper.Action("detail", "demo", new { id = p.Id }))
                        .WithValueTemplate("<a href='{Value}' class='btn btn-danger' role='button'>Delete</a>");
                    cols.Add("Example").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText("Example")
                        .WithValueExpression((p, c) => p.Active ? "label-success" : "label-danger")
                        .WithValueTemplate("You can access any of the item's properties: <strong>{Model.FirstName}</strong> <br />or the current column value: <span class='label {Value}'>{Model.Active}</span>");
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(20)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string sortColumn = options.GetSortColumnData<string>();

                    var items = repo.GetData(out totalRecords,
                        options.GetLimitOffset(), options.GetLimitRowcount(),
                        sortColumn, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );


            MVCGridDefinitionTable.Add("CustomErrorMessage", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithErrorMessageHtml(@"<div class=""alert alert-danger"" role=""alert"">OH NO!!!</div>")
                .WithSorting(true)
                .WithPaging(true)
                .WithDefaultSortColumn("LastName")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    var result = new QueryResult<Person>();

                    using (var db = new SampleDatabaseEntities())
                    {
                        var query = db.People.AsQueryable();

                        result.TotalRecords = query.Count();

                        if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                        {
                            switch (options.SortColumnName.ToLower())
                            {
                                case "firstname":
                                    throw new Exception("Test exception");
                                case "lastname":
                                    query = query.OrderBy(p => p.LastName, options.SortDirection);
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

            MVCGridDefinitionTable.Add("UsageExample", new MVCGridBuilder<YourModelItem>()
                .AddColumns(cols =>
                {
                    // Add your columns here
                    cols.Add("UniqueColumnName").WithHeaderText("Any Header")
                        .WithValueExpression((i, c) => i.YourProperty); // use the Value Expression to return the cell text for this column
                })
                .WithRetrieveDataMethod((context) =>
                {
                    // Query your data here. Obey Ordering, paging and filtering paramters given in the context.QueryOptions.
                    // Use Entity Framwork, a module from your IoC Container, or any other method.
                    // Return QueryResult object containing IEnumerable<YouModelItem>

                    return new QueryResult<YourModelItem>()
                    {
                        Items = new List<YourModelItem>(),
                        TotalRecords = 0 // if paging is enabled, return the total number of records of all pages
                    };

                })
            );

            MVCGridDefinitionTable.Add("GlobalSearchGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithAdditionalQueryOptionNames("GlobalSearch")
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    string globalSearch = options.GetAdditionalQueryOptionString("GlobalSearch");

                    var items = repo.GetData(out totalRecords, globalSearch, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumnName, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            MVCGridDefinitionTable.Add("PageSizeGrid", new MVCGridBuilder<Person>()
                .AddColumns(cols =>
                {
                    cols.Add("Id").WithSorting(false)
                        .WithValueExpression((p, c) => p.Id.ToString());
                    cols.Add("FirstName").WithHeaderText("First Name")
                        .WithValueExpression((p, c) => p.FirstName);
                    cols.Add("LastName").WithHeaderText("Last Name")
                        .WithValueExpression((p, c) => p.LastName);
                })
                .WithSorting(true)
                .WithDefaultSortColumn("LastName")
                .WithPaging(true)
                .WithItemsPerPage(10)
                .WithAllowChangingPageSize(true)
                .WithMaxItemsPerPage(100)
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IPersonRepository>();

                    var items = repo.GetData(out totalRecords, options.GetLimitOffset(), options.GetLimitRowcount(),
                        options.SortColumnName, options.SortDirection == SortDirection.Dsc);

                    return new QueryResult<Person>()
                    {
                        Items = items,
                        TotalRecords = totalRecords
                    };
                })
            );

            //MVCGridDefinitionTable.Add DO NOT DELETE - Needed for demo code parsing


            GridDefinition<YourModelItem> def = new GridDefinition<YourModelItem>();

            GridColumn<YourModelItem> column = new GridColumn<YourModelItem>();
            column.ColumnName = "UniqueColumnName";
            column.HeaderText = "Any Header";
            column.ValueExpression = (i, c) => i.YourProperty;
            def.AddColumn(column);

            def.RetrieveData = (options) =>
            {
                return new QueryResult<YourModelItem>()
                    {
                        Items = new List<YourModelItem>(),
                        TotalRecords = 0
                    };
            };
            MVCGridDefinitionTable.Add("NonFluentUsageExample", def);

            GridDefaults defaultSet1 = new GridDefaults()
            {
                Paging = true,
                ItemsPerPage = 20,
                Sorting = true,
                NoResultsMessage = "Sorry, no results were found"
            };

            MVCGridDefinitionTable.Add("DefaultsExample",
                new MVCGridBuilder<YourModelItem>(defaultSet1) // pass in defauls object to ctor
                .AddColumns(cols =>
                {
                    // add columns
                })
                .WithDefaultSortColumn("Test")
                .WithRetrieveDataMethod((context) =>
                {
                    //get data
                    return new QueryResult<YourModelItem>();
                })
            );


            var docsReturnTypeColumn = new GridColumn<MethodDocItem>()
            {
                ColumnName = "ReturnType",
                HeaderText = "Return Type",
                HtmlEncode = false,
                ValueExpression = (p, c) => String.Format("<code>{0}</code>", HttpUtility.HtmlEncode(p.Return))
            };
            var docsNameColumn = new GridColumn<MethodDocItem>()
            {
                ColumnName = "Name",
                HtmlEncode = false,
                ValueExpression = (p, c) => String.Format("<code>{0}</code>", HttpUtility.HtmlEncode(p.Name))
            };
            var docsDescriptionColumn = new GridColumn<MethodDocItem>()
            {
                ColumnName = "Description",
                ValueExpression = (p, c) => p.Description
            };

            Func<GridContext, QueryResult<MethodDocItem>> docsLoadData = (context) =>
            {
                var result = new QueryResult<MethodDocItem>();

                DocumentationRepository repo = new DocumentationRepository();
                result.Items = repo.GetData(context.GridName);

                return result;
            };

            MVCGridDefinitionTable.Add("GridDefinition", new MVCGridBuilder<MethodDocItem>()
                .AddColumn(docsReturnTypeColumn)
                .AddColumn(docsNameColumn)
                .AddColumn(docsDescriptionColumn)
                .WithRetrieveDataMethod(docsLoadData)
            );

            MVCGridDefinitionTable.Add("GridColumn", new MVCGridBuilder<MethodDocItem>()
                .AddColumn(docsReturnTypeColumn)
                .AddColumn(docsNameColumn)
                .AddColumn(docsDescriptionColumn)
                .WithRetrieveDataMethod(docsLoadData)
            );

            MVCGridDefinitionTable.Add("QueryOptions", new MVCGridBuilder<MethodDocItem>()
                .AddColumn(docsReturnTypeColumn)
                .AddColumn(docsNameColumn)
                .AddColumn(docsDescriptionColumn)
                .WithRetrieveDataMethod(docsLoadData)
            );

        }


        
    }

}