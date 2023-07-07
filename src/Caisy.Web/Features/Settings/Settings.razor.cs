using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.Settings;

public partial class Settings : IDisposable
{
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public ApplicationState ApplicationState { get; set; } = null!;
    [Inject] public IIdentityProvider IdentityProvider { get; set; } = null!;


    private readonly CancellationTokenSource _cts = new();
    private readonly GetUserProfileQuery _model = new();
    private readonly string _title = "Settings";

    private string GetThemeIcon() => _model.PrefersDarkMode ? Icons.Material.Filled.DarkMode : Icons.Material.Filled.LightMode;

    protected override async Task OnInitializedAsync()
    {
        _model.ApiKey = IdentityProvider?.User?.ApiKey ?? string.Empty;
        _model.PrefersDarkMode = IdentityProvider?.User?.PrefersDarkMode ?? false;
    }

    private async Task OnValidSubmitAsync()
    {
        if (IdentityProvider.User?.Id != null)
        {
            await ProfileRepository.RemoveAsync(IdentityProvider.User.Id, _cts.Token);
        }

        var newUserProfile = new UserProfile { ApiKey = _model.ApiKey, PrefersDarkMode = _model.PrefersDarkMode };
        await ProfileRepository.AddAsync(newUserProfile, _cts.Token);

        IdentityProvider.User = newUserProfile;
        ApplicationState.OnUserSettingsChanged?.Invoke();

        Snackbar.Add("Successfully updated settings.", Severity.Success);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class GetUserProfileQuery
{
    [Required] public string ApiKey { get; set; } = null!;
    public bool PrefersDarkMode { get; set; }
}

public class ApplicationState
{
    public Action? OnUserSettingsChanged { get; set; }
    public MudThemeProvider MudThemeProvider { get; set; } = null!;
}
