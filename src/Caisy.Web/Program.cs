using Blazored.LocalStorage;
using Caisy.Web.Features.CodeConverter;
using Caisy.Web.Features.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<App>());
builder.Services.AddAutoMapper(typeof(App));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<UserProfileState>();
builder.Services.AddScoped<IUser, UserProfile>();
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
builder.Services.AddTransient<CodeConverterState>();

var app = builder.Build();

var repository = app.Services.GetRequiredService<IRepository<UserProfile>>();
var existingUserProfile = (await repository.GetAllAsync(CancellationToken.None)).FirstOrDefault();

var user = app.Services.GetRequiredService<IUser>();
user = existingUserProfile;

await app.RunAsync();
