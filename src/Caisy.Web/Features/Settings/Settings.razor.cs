namespace Caisy.Web.Features.Settings;

public partial class Settings : IDisposable
{
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ApplicationState ApplicationState { get; set; } = null!;
    [Inject] public IIdentityProvider IdentityProvider { get; set; } = null!;

    private readonly CancellationTokenSource _cts = new();
    private readonly GetUserProfileQuery _model = new();

    private string GetThemeIcon() => _model.PrefersDarkMode ? Icons.Material.Filled.DarkMode : Icons.Material.Filled.LightMode;

    protected override async Task OnInitializedAsync()
    {
        var existingUserProfile = (await ProfileRepository.GetAllAsync(CancellationToken.None)).FirstOrDefault();
        if (existingUserProfile != null)
        {
            IdentityProvider!.User = existingUserProfile;
            ApplicationState.OnUserSettingsChanged?.Invoke();
        }

        _model.PrefersDarkMode = IdentityProvider?.User?.PrefersDarkMode ?? false;
    }

    private async Task UpdateSettingsAsync(bool value)
    {
        _model.PrefersDarkMode = value;

        if (IdentityProvider.User?.Id != null)
        {
            await ProfileRepository.RemoveAsync(IdentityProvider.User.Id, _cts.Token);
        }

        var newUserProfile = new UserProfile { PrefersDarkMode = _model.PrefersDarkMode };
        await ProfileRepository.AddAsync(newUserProfile, _cts.Token);

        IdentityProvider.User = newUserProfile;
        ApplicationState.OnUserSettingsChanged?.Invoke();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class GetUserProfileQuery
{
    public bool PrefersDarkMode { get; set; }
}

public class ApplicationState
{
    public Action? OnUserSettingsChanged { get; set; }
    public MudThemeProvider MudThemeProvider { get; set; } = null!;
}
