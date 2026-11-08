using System.Net;
using System.Security.Cryptography;
using System.Net.Http.Json;
using Celd.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Celd.Api.Tests;

public sealed class TestApp : WebApplicationFactory<Program>
{
    public string TestPassword { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    private readonly string dbPath=Path.Combine(Path.GetTempPath(),"celd-test-"+Guid.NewGuid().ToString("N")+".db");
    private static string FindRoot()
    {
        var dir=new DirectoryInfo(AppContext.BaseDirectory);
        while(dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName,"server","Celd.Api","Celd.Api.csproj"))) return dir.FullName;
            dir=dir.Parent;
        }
        throw new InvalidOperationException("Could not find repository root for tests.");
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(Path.Combine(FindRoot(),"server","Celd.Api"));
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_,configuration)=>
        {
            configuration.AddInMemoryCollection(new Dictionary<string,string?>
            {
                ["ConnectionStrings:Celd"]=$"Data Source={dbPath};Cache=Shared;Default Timeout=15",
                ["Seed:Student1Password"]=TestPassword,
                ["Seed:Student2Password"]=TestPassword,
                ["Seed:StaffPassword"]=TestPassword
            });
        });
    }
    public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions { HandleCookies=true });
    public CeldDbContext Database()
    {
        var opts=new DbContextOptionsBuilder<CeldDbContext>()
            .UseSqlite($"Data Source={dbPath};Cache=Shared;Default Timeout=15").Options;
        return new CeldDbContext(opts);
    }
    public async Task<HttpClient> Login(string email)
    {
        var client=Client();
        using var response=await Post(client,"/api/auth/login",new { email, password=TestPassword });
        if(response.StatusCode!=HttpStatusCode.OK)
            throw new InvalidOperationException($"Test setup login failed: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return client;
    }
    public Task<HttpClient> Student1()=>Login("student1@celd.local");
    public Task<HttpClient> Student2()=>Login("student2@celd.local");
    public Task<HttpClient> Staff()=>Login("staff@celd.local");

    public static async Task<HttpResponseMessage> Post(HttpClient c,string path,object? data=null)
    {
        var csrf=await c.GetFromJsonAsync<TokenResponse>("/api/auth/csrf");
        using var request=new HttpRequestMessage(HttpMethod.Post,path)
            {Content=JsonContent.Create(data??new { })};
        request.Headers.Add("X-CSRF-TOKEN",csrf!.RequestToken);
        return await c.SendAsync(request);
    }
    public sealed record TokenResponse(string RequestToken);
}
