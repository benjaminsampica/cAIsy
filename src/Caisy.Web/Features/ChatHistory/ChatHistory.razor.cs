using System.Collections.ObjectModel;
using static Caisy.Web.Features.ChatHistory.GetChatHistoryListResponse;

namespace Caisy.Web.Features.ChatHistory;

public partial class ChatHistory : IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private GetChatHistoryListResponse? _response;
    private readonly CancellationTokenSource _cts = new();
    private readonly string _title = "Chat History";

    protected override async Task OnInitializedAsync()
    {
        _response = await Mediator.Send(new GetChatHistoryListQuery(), _cts.Token);
    }

    public void NavigateToChat(long Id)
    {
        NavigationManager.NavigateTo(Id.ToString());
    }

    public async Task DeleteChatAsync(long Id)
    {
        await Mediator.Send(new DeleteChatHistoryCommand(Id));

        var existingChatHistory = _response!.ChatHistories.First(ch => ch.Id == Id);
        _response.ChatHistories.Remove(existingChatHistory);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}


public record GetChatHistoryListQuery : IRequest<GetChatHistoryListResponse>;

public class GetChatHistoryListHandler : IRequestHandler<GetChatHistoryListQuery, GetChatHistoryListResponse>
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;
    private readonly IMapper _mapper;

    public GetChatHistoryListHandler(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository, IMapper mapper)
    {
        _chatHistoryRepository = chatHistoryRepository;
        _mapper = mapper;
    }

    public async Task<GetChatHistoryListResponse> Handle(GetChatHistoryListQuery query, CancellationToken cancellationToken)
    {
        var chatHistories = await _chatHistoryRepository.GetAllAsync(cancellationToken);

        var chatHistoryItems = _mapper.Map<ObservableCollection<ChatHistoryItem>>(chatHistories);

        return new GetChatHistoryListResponse
        {
            ChatHistories = chatHistoryItems
        };
    }
}

public class GetChatHistoryListResponse
{
    public required ObservableCollection<ChatHistoryItem> ChatHistories { get; set; } = new ObservableCollection<ChatHistoryItem>();

    public class ChatHistoryItem
    {
        public required long Id { get; set; }
        public required DateTimeOffset CreatedOn { get; set; }
        public required string Summary { get; set; }

        public class ChatHistoryItemProfile : Profile
        {
            public ChatHistoryItemProfile()
            {
                CreateMap<Infrastructure.Models.ChatHistory, ChatHistoryItem>();
            }
        }
    };
}


public record DeleteChatHistoryCommand(long Id) : IRequest;

public class DeleteChatHistoryHandler : IRequestHandler<DeleteChatHistoryCommand>
{
    private readonly IRepository<Infrastructure.Models.ChatHistory> _chatHistoryRepository;

    public DeleteChatHistoryHandler(IRepository<Infrastructure.Models.ChatHistory> chatHistoryRepository)
    {
        _chatHistoryRepository = chatHistoryRepository;
    }

    public async Task Handle(DeleteChatHistoryCommand command, CancellationToken cancellationToken)
    {
        await _chatHistoryRepository.RemoveAsync(command.Id, cancellationToken);
    }
}
