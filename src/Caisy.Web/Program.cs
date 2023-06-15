using Blazored.LocalStorage;
using Caisy.Web.Features.Profile;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using OpenAI_API;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<ProfileState>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

await builder.Build().RunAsync();
