﻿using Packt.Shared;
using System.Net;


namespace Northwind.Mvc.GraphQLClient.Models;

public class IndexViewModel
{
    public HttpStatusCode Code { get; set; }
    public string? RawResponseBody { get; set; }
    public Product[]? Products { get; set; }
    public Category[]? Categories { get; set; }
    public Error[]? Errors { get; set; }
}
