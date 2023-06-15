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
    private List<string> _options = new();

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

        //TESTING options. This will ideally come in through the UI (checkboxes?):   
        _options.Add("Prefer C#");
        _options.Add("Prefer EF Core");
    }

    private async Task OnValidSubmitAsync()
    {
        _conversation.AppendSystemMessage(String.Join(", ", _options));

        _conversation.AppendUserInput(_request.Prompt); 
        _isInProgress = true;

        await _conversation.GetResponseFromChatbotAsync();

        _response.Response = string.Empty;
        foreach (var msg in _conversation.Messages)
        {
            if (msg.Role == ChatMessageRole.System) continue;
            _response.Response += $"{Environment.NewLine} {msg.Role}: {msg.Content}";
        }

        if (_request.IncludeTestCase)
        {
            await GetTestCaseResult();
        }
        else
        {
            _isInProgress = false;
        }
    }

    private async Task GetTestCaseResult()
    {
        _conversation.AppendUserInput($"Get {_request.TestCaseType} test case for above result.");
        await _conversation.GetResponseFromChatbotAsync();

        _request.IncludeTestCase = false;
        _isInProgress = false;
        
        _response.Response = string.Empty;
        foreach (var msg in _conversation.Messages)
        {
            if (msg.Role == ChatMessageRole.System) continue;
            _response.Response += $"{Environment.NewLine} {msg.Role}: {msg.Content}";
        }
    }
}

public class OpenApiRequest
{
    public string? Prompt { get; set; }
    public bool IncludeTestCase { get; set; }
    public string? TestCaseType { get; set; }
}

public class OpenApiResponse
{
    public string? Response { get; set; }
}
