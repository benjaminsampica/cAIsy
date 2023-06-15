using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.Profile;

public partial class Profile : IDisposable
{
    [Inject] public ProfileState ProfileState { get; set; } = null!;
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    private readonly CancellationTokenSource _cts = new();
    private readonly ProfileRequest _model = new();

    protected override async Task OnInitializedAsync()
    {
        _model.ApiKey = ProfileState?.ApiKey ?? string.Empty;
    }

    private async Task OnValidSubmitAsync()
    {
        var existingUserProfile = await ProfileRepository.FindAsync(_model.ApiKey, _cts.Token);

        if (existingUserProfile != null)
        {
            await ProfileRepository.RemoveAsync(existingUserProfile.Id, _cts.Token);
        }

        var newUserProfile = new UserProfile { ApiKey = _model.ApiKey };
        await ProfileRepository.AddAsync(newUserProfile, _cts.Token);

        ProfileState.ApiKey = newUserProfile.ApiKey;

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
}

public class ProfileState
{
    public string? ApiKey { get; set; }
}
