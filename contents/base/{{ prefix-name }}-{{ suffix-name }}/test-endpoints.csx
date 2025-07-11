#!/usr/bin/env dotnet-script
using System;
using System.Net.Http;
using System.Threading.Tasks;

var client = new HttpClient();

Console.WriteLine("Testing endpoints on http://localhost:5031:");
Console.WriteLine();

// Test root
var rootResponse = await client.GetAsync("http://localhost:5031/");
Console.WriteLine($"GET /: {rootResponse.StatusCode}");
if (rootResponse.IsSuccessStatusCode)
{
    Console.WriteLine($"  Content: {await rootResponse.Content.ReadAsStringAsync()}");
}

// Test health
var healthResponse = await client.GetAsync("http://localhost:5031/health");
Console.WriteLine($"GET /health: {healthResponse.StatusCode}");

// Test swagger
var swaggerResponse = await client.GetAsync("http://localhost:5031/swagger");
Console.WriteLine($"GET /swagger: {swaggerResponse.StatusCode}");

// Test auth endpoint with POST
var authResponse = await client.PostAsync("http://localhost:5031/api/auth/token", 
    new StringContent("{\"clientId\":\"admin-client\",\"clientSecret\":\"admin-secret\"}", 
    System.Text.Encoding.UTF8, "application/json"));
Console.WriteLine($"POST /api/auth/token: {authResponse.StatusCode}");
if (!authResponse.IsSuccessStatusCode)
{
    Console.WriteLine($"  Content: {await authResponse.Content.ReadAsStringAsync()}");
}

// Test {{ PrefixName }}{{ SuffixName }} endpoint
var apiResponse = await client.GetAsync("http://localhost:5031/api/{{ PrefixName }}{{ SuffixName }}");
Console.WriteLine($"GET /api/{{ PrefixName }}{{ SuffixName }}: {apiResponse.StatusCode}");