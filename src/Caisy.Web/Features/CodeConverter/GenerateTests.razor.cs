namespace Caisy.Web.Features.CodeConverter;

public partial class GenerateTests : IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = null!;
    [CascadingParameter] public ErrorHandler ErrorHandler { get; set; } = null!;
    [Parameter] public bool IsGenerating { get; set; }
    [Parameter] public EventCallback<bool> IsGeneratingChanged { get; set; }

    private readonly GenerateTestsCommand _model = new();
    private readonly CancellationTokenSource _cts = new();

    private async Task OnClickAsync()
    {
        try
        {
            await IsGeneratingChanged.InvokeAsync(true);
            await Mediator.Send(_model);
        }
        catch (FailedOpenAIApiRequestException ex)
        {
            ErrorHandler.ProcessError(ex);
        }
        finally
        {
            await IsGeneratingChanged.InvokeAsync(false);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
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
        _codeConverterState.Conversation.AddUserMessage($"Generate tests for the above code using the {command.Framework.GetDisplayName()} framework.");

        var responseMessage = await _openAIApiService.GetBestCompletionAsync(_codeConverterState.Conversation, cancellationToken);
        _codeConverterState.Conversation.AddCaisyMessage(responseMessage);

        await _mediator.Publish(new SaveChatHistoryCommand<CodeConverterState>(), cancellationToken);
    }
}
