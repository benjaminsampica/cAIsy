﻿using Caisy.Web.Features.Profile;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Text.Json;

namespace Caisy.Web.Features.Home;

public partial class Home
{
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = null!;
    [CascadingParameter] public ProfileState ProfileState { get; set; } = null!;
    [Parameter] public int? Id { get; set; }
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

        StartConversation();

        if (Id != null)
        {
            var chatHistory = await JSRuntime.InvokeAsync<string>("localStorage.getItem", Id.ToString());
            if (chatHistory != null)
            {
                var detail = JsonSerializer.Deserialize<ChatDetail>(chatHistory);
                if (detail != null)
                {
                    _source = detail.Source;
                    _destination = detail.Destination;

                    foreach (var message in detail.Messages.Where(x => x.Role.ToLower() != "system"))
                    {
                        if (message.Role.ToLower() == "user")
                        {
                            _conversation.AppendMessage(new ChatMessage(ChatMessageRole.User, message.Content));
                        }
                        else
                        {
                            _conversation.AppendMessage(new ChatMessage(ChatMessageRole.Assistant, message.Content));
                        }

                    }
                }
            }
        }
    }

    private void StartConversation()
    {
        _conversation = OpenAiApi.Chat.CreateConversation();
        _conversation.AppendSystemMessage($"Convert {_source} to {_destination}");
    }

    private async Task OnSourceChangedAsync(string value)
    {
        if (_source == value) return;

        if (_conversation.Messages.Count > 1)
        {
            var chatDetail = new ChatDetail(_conversation.Messages.ToList(), _source, _destination); // Broken -- need to fix this functionality.
            var chatHistory = JsonSerializer.Serialize(chatDetail);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", chatDetail.Id.ToString(), chatHistory);
        }

        _source = value;
        StartConversation();
    }
    private async Task OnDestinationChangedAsync(string value)
    {
        if (_destination == value) return;

        if (_conversation.Messages.Count > 1)
        {
            var chatDetail = new ChatDetail(_conversation.Messages.ToList(), _source, _destination); // Broken -- need to fix this functionality.
            var chatHistory = JsonSerializer.Serialize(chatDetail);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", DateTime.Now.ToString(), chatHistory);
        }

        _destination = value;
        StartConversation();
    }

    private async Task OnValidSubmitAsync()
    {
        _isInProgress = true;

        _conversation.AppendUserInput(_request.Prompt);

        await _conversation.GetResponseFromChatbotAsync();

        _isInProgress = false;
        _anyCode = true;
    }

    private async Task OnFileUploadAsync(InputFileChangeEventArgs e)
    {
        if (e.FileCount == 0) return;

        var file = e.File;
        using var streamReader = new StreamReader(file.OpenReadStream());

        var fileContent = await streamReader.ReadToEndAsync();
        _request.Prompt = fileContent;
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
