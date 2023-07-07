using Caisy.Web.Features.CodeConverter;

namespace Caisy.Web.Features.CodeReader;

public partial class CodeReader : IDisposable
{
    [Inject] public IMediator Mediator { get; set; } = null!;
    [Inject] public CodeReaderState CodeReaderState { get; set; } = null!;
    [CascadingParameter] public IUser? User { get; set; }
    [CascadingParameter] public ErrorHandler ErrorHandler { get; set; } = null!;
    [Parameter] public long? ChatHistoryId { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private readonly ReadCodeCommand _model = new();
    private readonly string _title = "Code Reader";
    private bool _isGenerating = false;
    private bool _isOptionsOpen;

    protected override async Task OnInitializedAsync()
    {
        if (User == null) return;

        CodeReaderState.ChatHistoryId = ChatHistoryId;
        CodeReaderState.Conversation = await Mediator.Send(new GetCodeReaderConversationQuery(ChatHistoryId), _cts.Token);
    }

    private async Task OnValidSubmitAsync()
    {
        try
        {
            _isGenerating = true;
            await Mediator.Send(_model);
        }
        catch (FailedOpenAIApiRequestException ex)
        {
            ErrorHandler.ProcessError(ex);
        }
        finally
        {
            _isGenerating = false;
        }
    }

    private void ToggleOptions() => _isOptionsOpen = !_isOptionsOpen;

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class CodeReaderState : IArchivingState
{
    public ConversationBase Conversation { get; set; } = new GetCodeConverterConversationQueryResponse();
    public long? ChatHistoryId { get; set; }
}

public class ReadCodeCommand : IRequest
{
    public string Code { get; set; } = string.Empty;
    public ReaderTemperament Temperament { get; set; } = ReaderTemperament.Nice;

    public string FormattedContent
    {
        get
        {
            var formattedContent = $"Document the code provided. ";

            formattedContent += Temperament switch
            {
                ReaderTemperament.Rude => "Be as rude as possible.",
                ReaderTemperament.Bro => "Explain in bro-speak.",
                _ => "Explain in simple terms."
            };

            formattedContent += $"\n\n```{Code}```";

            return formattedContent;
        }
    }

    public enum ReaderTemperament
    {
        Nice,
        Rude,
        Bro
    }
}

public class ReadCodeCommandHandler : IRequestHandler<ReadCodeCommand>
{
    private readonly CodeReaderState _codeReaderState;
    private readonly IOpenAIApiService _openAIApiService;
    private readonly IMediator _mediator;

    public ReadCodeCommandHandler(CodeReaderState codeReaderState, IOpenAIApiService openAIApiService, IMediator mediator)
    {
        _codeReaderState = codeReaderState;
        _openAIApiService = openAIApiService;
        _mediator = mediator;
    }

    public async Task Handle(ReadCodeCommand command, CancellationToken cancellationToken)
    {
        _codeReaderState.Conversation.AddUserMessage(command.FormattedContent);

        var response = await _openAIApiService.GetBestCompletionAsync(_codeReaderState.Conversation, cancellationToken);
        _codeReaderState.Conversation.AddCaisyMessage(response);

        await _mediator.Publish(new SaveChatHistoryCommand<CodeReaderState>(), cancellationToken);
    }
}

public record GetCodeReaderConversationQuery(long? ChatHistoryId) : IRequest<GetCodeReaderConversationQueryResponse>;

public class GetCodeReaderConversationQueryHandler : IRequestHandler<GetCodeReaderConversationQuery, GetCodeReaderConversationQueryResponse>
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;

    public GetCodeReaderConversationQueryHandler(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
    }

    public async Task<GetCodeReaderConversationQueryResponse> Handle(GetCodeReaderConversationQuery query, CancellationToken cancellationToken)
    {
        var messages = new List<Message>();

        if (query.ChatHistoryId is not null)
        {
            var chatHistory = await _chatHistoryRepository.FindAsync(query.ChatHistoryId.Value, cancellationToken);

            messages = _mapper.Map<List<Message>>(chatHistory!.Messages);
        }

        return new GetCodeReaderConversationQueryResponse
        {
            Messages = messages
        };
    }
}

public class GetCodeReaderConversationQueryResponse : ConversationBase { }
