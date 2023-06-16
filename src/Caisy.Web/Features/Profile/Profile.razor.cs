using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.Profile;

public partial class Profile : IDisposable
{
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] public ProfileState ProfileState { get; set; } = null!;

    private readonly CancellationTokenSource _cts = new();
    private readonly ProfileRequest _model = new();

    private string GetThemeIcon() => _model.PrefersDarkMode ? Icons.Material.Filled.DarkMode : Icons.Material.Filled.LightMode;

    protected override async Task OnInitializedAsync()
    {
        _model.ApiKey = ProfileState?.ApiKey ?? string.Empty;
        _model.PrefersDarkMode = ProfileState?.PrefersDarkMode ?? false;
    }

    private async Task OnValidSubmitAsync()
    {
        if (ProfileState.Id != null)
        {
            await ProfileRepository.RemoveAsync(ProfileState.Id, _cts.Token);
        }

        var newUserProfile = new UserProfile { ApiKey = _model.ApiKey, PrefersDarkMode = _model.PrefersDarkMode };
        await ProfileRepository.AddAsync(newUserProfile, _cts.Token);

        ProfileState.Id = newUserProfile.Id;
        ProfileState.ApiKey = newUserProfile.ApiKey;
        ProfileState.PrefersDarkMode = newUserProfile.PrefersDarkMode;

        ProfileState.OnUserSettingsChanged?.Invoke();

        Snackbar.Add("Successfully updated profile.", Severity.Success);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class ProfileRequest
{
    [Required] public string ApiKey { get; set; } = null!;
    public bool PrefersDarkMode { get; set; }
}

public class ProfileState
{
    public string? ApiKey { get; set; }
    public string? Id { get; set; }
    public bool PrefersDarkMode { get; set; }
    public CodeBlockTheme CodeBlockTheme => PrefersDarkMode ? CodeBlockTheme.AtomOneDark : CodeBlockTheme.AtomOneLight;
    public Action? OnUserSettingsChanged { get; set; }
    public MudThemeProvider MudThemeProvider { get; set; } = null!;
}