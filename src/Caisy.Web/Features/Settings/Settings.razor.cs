using Caisy.Web.Features.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.Settings;

public partial class Settings : IDisposable
{
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] public UserProfileState UserProfileState { get; set; } = null!;

    private readonly CancellationTokenSource _cts = new();
    private readonly GetUserProfileQuery _model = new();

    private string GetThemeIcon() => _model.PrefersDarkMode ? Icons.Material.Filled.DarkMode : Icons.Material.Filled.LightMode;

    protected override async Task OnInitializedAsync()
    {
        _model.ApiKey = UserProfileState?.User?.ApiKey ?? string.Empty;
        _model.PrefersDarkMode = UserProfileState?.User?.PrefersDarkMode ?? false;
    }

    private async Task OnValidSubmitAsync()
    {
        if (UserProfileState.User?.Id != null)
        {
            await ProfileRepository.RemoveAsync(UserProfileState.User.Id, _cts.Token);
        }

        var newUserProfile = new UserProfile { ApiKey = _model.ApiKey, PrefersDarkMode = _model.PrefersDarkMode };
        await ProfileRepository.AddAsync(newUserProfile, _cts.Token);

        UserProfileState.User = newUserProfile;

        UserProfileState.OnUserSettingsChanged?.Invoke();

        Snackbar.Add("Successfully updated profile.", Severity.Success);
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

public class UserProfileState
{
    public UserProfileState(IUser user)
    {
        User = user;
    }

    public IUser? User { get; set; }
    public CodeBlockTheme CodeBlockTheme => User!.PrefersDarkMode ? CodeBlockTheme.AtomOneDark : CodeBlockTheme.AtomOneLight;
    public Action? OnUserSettingsChanged { get; set; }
    public MudThemeProvider MudThemeProvider { get; set; } = null!;
}
