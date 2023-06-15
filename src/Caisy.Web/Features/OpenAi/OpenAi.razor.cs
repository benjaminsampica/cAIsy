using OpenAI_API;
using OpenAI_API.Chat;

namespace Caisy.Web.Features.OpenAi;

public partial class OpenAi
{
    [Inject] public IRepository<UserProfile> ProfileRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    private OpenAIAPI OpenAiApi { get; set; }
    private OpenApiRequest _request = new();
    private OpenApiResponse _response = new();
    private Conversation _conversation;
    private readonly CancellationTokenSource _cts = new();
    private List<string> _options = new();

    protected override async Task OnInitializedAsync()
    {
        var profile = (await ProfileRepository.GetAllAsync(_cts.Token)).FirstOrDefault();

        if (profile != null)
        {
            OpenAiApi = new OpenAIAPI(profile.ApiKey);
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

        await _conversation.GetResponseFromChatbotAsync();

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
}

public class OpenApiResponse
{
    public string? Response { get; set; }
}
