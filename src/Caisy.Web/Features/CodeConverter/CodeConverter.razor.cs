using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.CodeConverter;

public partial class CodeConverter : IDisposable
{
    [Inject] public IMediator Mediator { get; set; } = null!;
    [Inject] public CodeConverterState CodeConverterState { get; set; } = null!;
    [CascadingParameter] public IUser? User { get; set; }
    [CascadingParameter] public Error Error { get; set; } = null!;
    [Parameter] public long? ChatHistoryId { get; set; }

    private bool _isGenerating = false;
    private bool _hasGeneratedCode = false;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConvertCodeCommand _convertCodeModel = new();
    private readonly GenerateTestsCommand _generateTestsModel = new();

    protected override async Task OnInitializedAsync()
    {
        if (User == null) return;

        CodeConverterState.ChatHistoryId = ChatHistoryId;
        CodeConverterState.Conversation = await Mediator.Send(new GetCodeConverterConversationQuery(ChatHistoryId), _cts.Token);
    }

    private async Task OnValidSubmitAsync()
    {
        try
        {
            _isGenerating = true;
            await Mediator.Send(_convertCodeModel);
            _hasGeneratedCode = true;
        }
        catch (FailedOpenAIApiRequestException ex)
        {
            Error.ProcessError(ex);
        }
        finally
        {
            _isGenerating = false;
        }
    }

    private async Task OnFileUploadAsync(InputFileChangeEventArgs e)
    {
        if (e.FileCount == 0) return;

        var file = e.File;
        using var streamReader = new StreamReader(file.OpenReadStream());

        var fileContent = await streamReader.ReadToEndAsync(_cts.Token);
        _convertCodeModel.Code = fileContent;
    }

    private async Task GenerateTests()
    {
        _isGenerating = true;

        await Mediator.Send(_generateTestsModel);

        _isGenerating = false;
    }

    private bool GenerateTestsButtonIsDisabled => _isGenerating || !_hasGeneratedCode;

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class CodeConverterState : IArchivingState
{
    public ConversationBase Conversation { get; set; } = new GetCodeConverterConversationQueryResponse();
    public long? ChatHistoryId { get; set; }
}

public class ConvertCodeCommand : IRequest
{
    public string Code { get; set; } = string.Empty;
    public ConvertCodeOption Source { get; set; } = ConvertCodeOption.SQL;
    public ConvertCodeOption Destination { get; set; } = ConvertCodeOption.EntityFrameworkCore;

    public string FormattedContent => $"Convert {Source.GetDisplayName()} to {Destination.GetDisplayName()}.\n\n```{Code}```";

    public enum ConvertCodeOption
    {
        [Display(Name = "Entity Framework Core")]
        EntityFrameworkCore,
        Dapper,
        ADO,
        SQL
    }
}

public class ConvertCodeCommandHandler : IRequestHandler<ConvertCodeCommand>
{
    private readonly CodeConverterState _codeConverterState;
    private readonly IOpenAIApiService _openAIApiService;
    private readonly IMediator _mediator;

    public ConvertCodeCommandHandler(CodeConverterState codeConverterState, IOpenAIApiService openAIApiService, IMediator mediator)
    {
        _codeConverterState = codeConverterState;
        _openAIApiService = openAIApiService;
        _mediator = mediator;
    }

    public async Task Handle(ConvertCodeCommand command, CancellationToken cancellationToken)
    {
        _codeConverterState.Conversation.AddUserMessage(command.FormattedContent);

        var response = await _openAIApiService.GetBestCompletionAsync(_codeConverterState.Conversation, cancellationToken);
        _codeConverterState.Conversation.AddCaisyMessage(response);

        await _mediator.Publish(new SaveChatHistoryCommand<CodeConverterState>(), cancellationToken);
    }
}

public class GenerateTestsCommand : IRequest
{
    public TestFramework Framework { get; set; } = TestFramework.XUnit;

    public enum TestFramework
    {
        XUnit,
        NUnit,
        MSTest
    }
};

public class GenerateTestsCommandHandler : IRequestHandler<GenerateTestsCommand>
{
    private readonly CodeConverterState _codeConverterState;
    private readonly IOpenAIApiService _openAIApiService;
    private readonly IMediator _mediator;

    public GenerateTestsCommandHandler(CodeConverterState codeConverterState, IOpenAIApiService openAIApiService, IMediator mediator)
    {
        _codeConverterState = codeConverterState;
        _openAIApiService = openAIApiService;
        _mediator = mediator;
    }

    public async Task Handle(GenerateTestsCommand command, CancellationToken cancellationToken)
    {
        _codeConverterState.Conversation.AddUserMessage("Generate tests for the above code.");

        var responseMessage = await _openAIApiService.GetBestCompletionAsync(_codeConverterState.Conversation, cancellationToken);
        _codeConverterState.Conversation.AddCaisyMessage(responseMessage);

        await _mediator.Publish(new SaveChatHistoryCommand<CodeConverterState>(), cancellationToken);
    }
}

public record GetCodeConverterConversationQuery(long? ChatHistoryId) : IRequest<GetCodeConverterConversationQueryResponse>;

public class GetCodeConverterConversationQueryHandler : IRequestHandler<GetCodeConverterConversationQuery, GetCodeConverterConversationQueryResponse>
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;

    public GetCodeConverterConversationQueryHandler(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
    }

    public async Task<GetCodeConverterConversationQueryResponse> Handle(GetCodeConverterConversationQuery query, CancellationToken cancellationToken)
    {
        var messages = new List<Message>();

        if (query.ChatHistoryId is not null)
        {
            var chatHistory = await _chatHistoryRepository.FindAsync(query.ChatHistoryId.Value, cancellationToken);

            messages = _mapper.Map<List<Message>>(chatHistory!.Messages);
        }

        return new GetCodeConverterConversationQueryResponse
        {
            Messages = messages
        };
    }
}

public class GetCodeConverterConversationQueryResponse : ConversationBase { }

