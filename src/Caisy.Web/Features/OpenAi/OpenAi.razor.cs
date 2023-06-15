using Caisy.Web.Features.Profile;
using OpenAI_API;

namespace Caisy.Web.Features.OpenAi;

public partial class OpenAi : IDisposable
{
    [Inject] public ProfileState ProfileState { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    private OpenAIAPI OpenAiApi { get; set; }
    private OpenApiRequest _request = new();
    private OpenApiResponse? _response;
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
        var result = await OpenAiApi.Completions.GetCompletion(_request.Prompt);
        _response = new OpenApiResponse
        {
            Response = result
        };
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
