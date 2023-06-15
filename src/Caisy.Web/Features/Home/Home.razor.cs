using Caisy.Web.Features.Profile;
using OpenAI_API;

namespace Caisy.Web.Features.Home;

public partial class Home
{
    [Inject] public ProfileState ProfileState { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    private OpenAIAPI OpenAiApi { get; set; }
    private OpenApiRequest _request = new();
    private OpenApiResponse? _response;
    private bool _isInProgress = false;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        if (ProfileState.ApiKey != null)
        {
            OpenAiApi = new OpenAIAPI(ProfileState.ApiKey);
        }
    }

    private async Task OnValidSubmitAsync()
    {
        if (ProfileState.ApiKey == null)
        {
            //TODO:  snackbar or validation message?
            Snackbar.Add("No ApiKey found.  Please set up your Profile first.", Severity.Error);
            return;
        }

        _isInProgress = true;

        var result = await OpenAiApi.Completions.GetCompletion(_request.Prompt);
        _response = new OpenApiResponse
        {
            Response = result
        };

        _isInProgress = false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class OpenApiRequest
{
    public string? Prompt { get; set; }
}

public class OpenApiResponse
{
    public string? Response { get; set; }
}
