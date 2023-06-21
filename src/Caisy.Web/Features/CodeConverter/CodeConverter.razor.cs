using Caisy.Web.Features.Shared;
using Caisy.Web.Features.Shared.Handlers;
using Caisy.Web.Features.Shared.Services;
using Caisy.Web.Features.Shared.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Caisy.Web.Features.CodeConverter;

public partial class CodeConverter : IDisposable
{
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IMediator Mediator { get; set; } = null!;
    [Inject] public CodeConverterState CodeConverterState { get; set; } = null!;
    [CascadingParameter] public IUser? User { get; set; }
    [Parameter] public Guid? ChatHistoryId { get; set; }

    private string _actualExistingChatHistoryId { get; set; } = string.Empty;
    private bool _isGenerating = false;
    private bool _hasGeneratedCode = false;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConvertCodeCommand _convertCodeModel = new();
    private readonly GenerateTestsCommand _generateTestsModel = new();

    protected override async Task OnInitializedAsync()
    {
        if (User == null) return;

        if (ChatHistoryId != null)
        {
            _actualExistingChatHistoryId = nameof(Infrastructure.Models.ChatHistory) + ChatHistoryId.ToString();
        }

        CodeConverterState.OnCodeConverterStateChanged += StateHasChanged;
        CodeConverterState.OnCodeConverterStateChanged += SaveToChatHistoryAsync;
        CodeConverterState.Conversation = await Mediator.Send(new GetCodeConverterConversationQuery(_actualExistingChatHistoryId), _cts.Token);
    }

    private async Task OnValidSubmitAsync()
    {
        _isGenerating = true;

        await Mediator.Send(_convertCodeModel);

        _isGenerating = false;
        _hasGeneratedCode = true;
    }

    private async Task OnFileUploadAsync(InputFileChangeEventArgs e)
    {
        if (e.FileCount == 0) return;

        var file = e.File;
        using var streamReader = new StreamReader(file.OpenReadStream());

        var fileContent = await streamReader.ReadToEndAsync(_cts.Token);
        _convertCodeModel.Code = fileContent;
    }

    private async void SaveToChatHistoryAsync()
    {
        _actualExistingChatHistoryId = await Mediator.Send(new SaveChatHistoryCommand { Conversation = CodeConverterState.Conversation, ExistingChatHistoryId = _actualExistingChatHistoryId }, _cts.Token);
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
        CodeConverterState.OnCodeConverterStateChanged -= StateHasChanged;
        _cts.Cancel();
        _cts.Dispose();
    }
}

public class CodeConverterState
{
    public GetCodeConverterConversationResponse Conversation { get; set; } = new GetCodeConverterConversationResponse();

    public Action? OnCodeConverterStateChanged { get; set; }
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

    public ConvertCodeCommandHandler(CodeConverterState codeConverterState, IOpenAIApiService openAIApiService)
    {
        _codeConverterState = codeConverterState;
        _openAIApiService = openAIApiService;
    }

    public async Task Handle(ConvertCodeCommand command, CancellationToken cancellationToken)
    {
        var requestMessage = new ConversationBase.Message { Content = command.FormattedContent, Role = ConversationBase.MessageRole.User };
        _codeConverterState.Conversation.Messages.Add(requestMessage);

        var responseMessage = await _openAIApiService.GetBestCompletionAsync(_codeConverterState.Conversation, cancellationToken);
        _codeConverterState.Conversation.Messages.Add(responseMessage);

        _codeConverterState.OnCodeConverterStateChanged?.Invoke();
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

    public GenerateTestsCommandHandler(CodeConverterState codeConverterState, IOpenAIApiService openAIApiService)
    {
        _codeConverterState = codeConverterState;
        _openAIApiService = openAIApiService;
    }

    public async Task Handle(GenerateTestsCommand command, CancellationToken cancellationToken)
    {
        var requestMessage = new ConversationBase.Message { Content = "Generate tests for the above code.", Role = ConversationBase.MessageRole.User };
        _codeConverterState.Conversation.Messages.Add(requestMessage);

        var responseMessage = await _openAIApiService.GetBestCompletionAsync(_codeConverterState.Conversation, cancellationToken);
        _codeConverterState.Conversation.Messages.Add(responseMessage);

        _codeConverterState.OnCodeConverterStateChanged?.Invoke();
    }
}

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
        var messages = new List<ConversationBase.Message>();

        if (query.ChatHistoryId is not null)
        {
            var chatHistory = await _chatHistoryRepository.FindAsync(query.ChatHistoryId, cancellationToken);

            messages = _mapper.Map<List<ConversationBase.Message>>(chatHistory!.Messages);
        }

        return new GetCodeConverterConversationResponse
        {
            Messages = messages
        };
    }
}

public class GetCodeConverterConversationResponse : ConversationBase { }

