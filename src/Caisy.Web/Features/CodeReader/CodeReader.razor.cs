using Caisy.Web.Features.Profile;
using Microsoft.AspNetCore.Components.Forms;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Caisy.Web.Features.CodeReader;

public partial class CodeReader
{
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] public ProfileState ProfileState { get; set; } = null!;
    private OpenAIAPI OpenAiApi { get; set; }
    private OpenApiRequest _request = new();
    private Conversation _conversation;
    private bool _isInProgress = false;
    private string _temperament = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (ProfileState.ApiKey != null)
        {
            OpenAiApi = new OpenAIAPI(ProfileState.ApiKey);
        }

        _conversation = OpenAiApi.Chat.CreateConversation();
        _conversation.AppendSystemMessage("Document the code provided. Explain in simple terms.");
    }

    private async Task OnTemperamentChangedAsync(string value)
    {
        if (_temperament == value) return;
        _temperament = value;
        _conversation = OpenAiApi.Chat.CreateConversation();
        _conversation.AppendSystemMessage("Document the code provided. Explain in simple terms. " + value);
    }

    private async Task OnValidSubmitAsync()
    {
        _isInProgress = true;

        _conversation.AppendUserInput(_request.Prompt);

        await _conversation.GetResponseFromChatbotAsync();

        _isInProgress = false;
    }

    private async Task OnFileUploadAsync(InputFileChangeEventArgs e)
    {
        if (e.FileCount == 0) return;

        var file = e.File;
        using (var streamReader = new StreamReader(file.OpenReadStream()))
        {
            var fileContent = await streamReader.ReadToEndAsync();
            _request.Prompt = fileContent;
        }
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
