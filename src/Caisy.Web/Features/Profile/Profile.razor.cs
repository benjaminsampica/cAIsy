using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.Profile;

public partial class Profile : IDisposable
{
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    private readonly CancellationTokenSource _cts = new();
    private ProfileRequest _model = new();

    protected override async Task OnInitializedAsync()
    {
        var profile = (await ProfileRepository.GetAllAsync(_cts.Token)).FirstOrDefault();

        _model.ApiKey = profile?.ApiKey ?? string.Empty;
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
