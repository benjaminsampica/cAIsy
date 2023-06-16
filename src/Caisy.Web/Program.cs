using Blazored.LocalStorage;
using Caisy.Web.Features.Profile;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ProfileState>();

var app = builder.Build();

var repository = app.Services.GetRequiredService<IRepository<UserProfile>>();
var existingUserProfile = (await repository.GetAllAsync(CancellationToken.None)).FirstOrDefault();

var profileState = app.Services.GetRequiredService<ProfileState>();
profileState.ApiKey = existingUserProfile?.ApiKey;
profileState.Id = existingUserProfile?.Id;
profileState.PrefersDarkMode = existingUserProfile?.PrefersDarkMode ?? false;

await app.RunAsync();
