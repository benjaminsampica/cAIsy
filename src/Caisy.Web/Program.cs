using Blazored.LocalStorage;
using Caisy.Web.Features.CodeConverter;
using Caisy.Web.Features.CodeReader;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(config => config.RootDirectory = "/Features");
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<App>());
builder.Services.AddTransient<INotificationHandler<SaveChatHistoryCommand<CodeConverterState>>, SaveChatHistoryCommandHandler<CodeConverterState>>();
builder.Services.AddTransient<INotificationHandler<SaveChatHistoryCommand<CodeReaderState>>, SaveChatHistoryCommandHandler<CodeReaderState>>();
builder.Services.AddAutoMapper(typeof(App));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ApplicationState>();
builder.Services.AddScoped<IIdentityProvider, IdentityProvider>();
builder.Services.AddTransient<IOpenAIApiService, OpenAIApiService>();
builder.Services.AddScoped<CodeConverterState>();
builder.Services.AddScoped<CodeReaderState>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//var userProfileRepository = app.Services.GetRequiredService<IRepository<UserProfile>>();
//var existingUserProfile = (await userProfileRepository.GetAllAsync(CancellationToken.None)).FirstOrDefault();
//var identityProvider = app.Services.GetRequiredService<IIdentityProvider>();
//identityProvider.User = existingUserProfile;

await app.RunAsync();
