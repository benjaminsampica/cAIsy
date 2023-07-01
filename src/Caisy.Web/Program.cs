using Blazored.LocalStorage;
using Caisy.Web.Features.CodeConverter;
using Caisy.Web.Features.CodeReader;
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
builder.Services.AddTransient<INotificationHandler<SaveChatHistoryCommand<CodeConverterState>>, SaveChatHistoryCommandHandler<CodeConverterState>>();
// TODO: Make chat history work with both code conversion & code reading.
//builder.Services.AddTransient<INotificationHandler<SaveChatHistoryCommand<CodeReaderState>>, SaveChatHistoryCommandHandler<CodeReaderState>>();
builder.Services.AddAutoMapper(typeof(App));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ApplicationState>();
builder.Services.AddScoped<IIdentityProvider, IdentityProvider>();
builder.Services.AddTransient<IOpenAIApiService, OpenAIApiService>();
builder.Services.AddScoped<CodeConverterState>();
builder.Services.AddScoped<CodeReaderState>();

var app = builder.Build();

var userProfileRepository = app.Services.GetRequiredService<IRepository<UserProfile>>();
var existingUserProfile = (await userProfileRepository.GetAllAsync(CancellationToken.None)).FirstOrDefault();
var identityProvider = app.Services.GetRequiredService<IIdentityProvider>();
identityProvider.User = existingUserProfile;

await app.RunAsync();
