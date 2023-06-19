using Caisy.Web.Features.Shared.Models;
using Caisy.Web.Features.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OpenAI_API.Chat;
using System.Text.Json;
using static Caisy.Web.Features.CodeConverter.ConvertCodeCommand;

namespace Caisy.Web.Features.CodeConverter;

public partial class CodeConverter : IDisposable
{
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IMediator Mediator { get; set; } = null!;
    [Inject] public CodeConverterState CodeConverterState { get; set; } = null!;
    [Inject] public IUser? User { get; set; }
    [Parameter] public string? ChatHistoryId { get; set; }

    private bool _isInProgress = false;
    private bool _anyCode = false;
    private readonly CancellationTokenSource _cts = new();
    private string _source = "SQL";
    private string _destination = "Entity Framework";

    protected override async Task OnInitializedAsync()
    {
        if (User == null) return;

        CodeConverterState.OnCodeConverterStateChanged += StateHasChanged;
        CodeConverterState.Conversation = await Mediator.Send(new GetCodeConverterConversationQuery(ChatHistoryId), _cts.Token);
    }

    private async Task OnSourceChangedAsync(string value)
    {
        if (_source == value) return;

        if (_conversation.Messages.Count > 0)
        {
            var chatDetail = new Infrastructure.Models.ChatHistory(_conversation.Messages.ToList<ChatMessage>(), _source, _destination); // Broken -- need to fix this functionality.
            var chatHistory = JsonSerializer.Serialize(chatDetail);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", chatDetail.Id.ToString(), chatHistory);
        }

        _source = value;
        StartConversation();
    }
    private async Task OnDestinationChangedAsync(string value)
    {
        if (_destination == value) return;

        if (_conversation.Messages.Count > 0)
        {
            var chatDetail = new Infrastructure.Models.ChatHistory(_conversation.Messages.ToList(), _source, _destination);  // Broken -- need to fix this functionality.
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

        var fileContent = await streamReader.ReadToEndAsync(_cts.Token);
        _request.Prompt = fileContent;
    }

    private async Task GetTestCaseResult()
    {
        _isInProgress = true;
        _conversation.AppendUserInput($"Write {_request.TestCaseFramework} test cases for above result.");
        await _conversation.GetResponseFromChatbotAsync();

        _isInProgress = false;
    }

    public void Dispose()
    {
        CodeConverterState.OnCodeConverterStateChanged -= StateHasChanged;
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class CodeConverterState
{
    public GetCodeConverterConversationResponse? Conversation { get; set; }

    public Action? OnCodeConverterStateChanged { get; set; }
}

public record ConvertCodeCommand(string Code, ConvertCodeOption Source, ConvertCodeOption Destination) : IRequest<ConvertCodeCommandResponse>
{
    public enum ConvertCodeOption
    {
        EntityFrameworkCore,
        Dapper,
        ADO,
        Sql
    }
};

public class ConvertCodeCommandHandler : IRequestHandler<ConvertCodeCommand, ConvertCodeCommandResponse>
{
    private readonly CodeConverterState _codeConverterState;
    private readonly OpenAIApiService _openAIApiService;
    private readonly IMapper _mapper;

    public ConvertCodeCommandHandler(CodeConverterState codeConverterState, OpenAIApiService openAIApiService, IMapper mapper)
    {
        _codeConverterState = codeConverterState;
        _mapper = mapper;
        _openAIApiService = openAIApiService;
    }

    public async Task<ConvertCodeCommandResponse> Handle(ConvertCodeCommand command, CancellationToken cancellationToken)
    {
        var prompt = "Convert code ";
    }
}

public class ConvertCodeCommandResponse
{
}

// todo: get test cases mq/r

public record GetCodeConverterConversationQuery(string? ChatHistoryId) : IRequest<GetCodeConverterConversationResponse>;

public class GetCodeConverterConversationHandler : IRequestHandler<GetCodeConverterConversationQuery, GetCodeConverterConversationResponse>
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;

    public GetCodeConverterConversationHandler(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
    }

    public async Task<GetCodeConverterConversationResponse> Handle(GetCodeConverterConversationQuery query, CancellationToken cancellationToken)
    {
        var messages = Array.Empty<ConversationBase.Message>();

        if (query.ChatHistoryId is not null)
        {
            var chatHistory = await _chatHistoryRepository.FindAsync(query.ChatHistoryId, cancellationToken);

            messages = _mapper.Map<ConversationBase.Message[]>(chatHistory!.Messages);
        }

        return new GetCodeConverterConversationResponse
        {
            Messages = messages
        };
    }
}

public class GetCodeConverterConversationResponse : ConversationBase { }

