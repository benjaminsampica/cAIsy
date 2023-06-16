using Caisy.Web.Features.Profile;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Caisy.Web.Features.Home;

public partial class Home
{
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] public ProfileState ProfileState { get; set; } = null!;
    private OpenAIAPI OpenAiApi { get; set; }
    private OpenApiRequest _request = new();
    private Conversation _conversation;
    private bool _isInProgress = false;
    private bool _anyCode = false;
    private readonly CancellationTokenSource _cts = new();
    private string _source = "SQL";
    private string _destination = "Entity Framework";

    protected override async Task OnInitializedAsync()
    {
        if (ProfileState.ApiKey != null)
        {
            OpenAiApi = new OpenAIAPI(ProfileState.ApiKey);
        }

        _conversation = OpenAiApi.Chat.CreateConversation();
        _conversation.AppendSystemMessage($"Convert {_source} to {_destination}");
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
        _isInProgress = true;

        _conversation.AppendUserInput(_request.Prompt);

        await _conversation.GetResponseFromChatbotAsync();

        _isInProgress = false;
        _anyCode = true;
    }

    private async Task GetTestCaseResult()
    {
        _isInProgress = true;
        _conversation.AppendUserInput($"Get {_request.TestCaseFramework} test case for above result.");
        await _conversation.GetResponseFromChatbotAsync();

        _isInProgress = false;
    }
}

public class OpenApiRequest
{
    public string? Prompt { get; set; }
    public string? TestCaseFramework { get; set; }
}

public class OpenApiResponse
{
    public string? Response { get; set; }
}
