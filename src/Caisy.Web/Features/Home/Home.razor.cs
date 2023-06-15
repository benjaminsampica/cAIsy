using Caisy.Web.Features.Profile;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Caisy.Web.Features.Home;

public partial class Home
{
    [Inject] public ProfileState ProfileState { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    private OpenAIAPI OpenAiApi { get; set; }
    private OpenApiRequest _request = new();
    private OpenApiResponse _response = new();
    private Conversation _conversation;
    private bool _isInProgress = false;
    private readonly CancellationTokenSource _cts = new();
    private string _source;
    private string _destination;

    protected override async Task OnInitializedAsync()
    {
        if (ProfileState.ApiKey != null)
        {
            OpenAiApi = new OpenAIAPI(ProfileState.ApiKey);
        }
        else
        {
            Snackbar.Add("No profile found.", Severity.Error);
        }

        _conversation = OpenAiApi.Chat.CreateConversation();
    }

    private async Task OnSourceChangedAsync(string value)
    {
        if (_source == value) return;
        _source = value;
        _conversation = OpenAiApi.Chat.CreateConversation();
        _conversation.AppendSystemMessage($"Convert {_source} to {_destination}");
    }
    private async Task OnDestinationChangedAsync(string value)
    {
        if (_destination == value) return;
        _destination = value;
        _conversation = OpenAiApi.Chat.CreateConversation();
        _conversation.AppendSystemMessage($"Convert {_source} to {_destination}");
    }

    private async Task OnValidSubmitAsync()
    {
        if (ProfileState.ApiKey == null)
        {
            Snackbar.Add("No ApiKey found.  Please set up your Profile first.", Severity.Error);
            return;
        }

        _isInProgress = true;

        _conversation.AppendUserInput(_request.Prompt);

        await _conversation.GetResponseFromChatbotAsync();

        _response.Response = string.Empty;
        foreach (var msg in _conversation.Messages)
        {
            if (msg.Role == ChatMessageRole.System) continue;
            _response.Response += $"{Environment.NewLine} {msg.Role}: {msg.Content}";
        }

        _isInProgress = false;
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
