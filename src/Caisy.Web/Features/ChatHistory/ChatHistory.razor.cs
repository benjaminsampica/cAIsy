using static Caisy.Web.Features.ChatHistory.GetChatHistoryListResponse;

namespace Caisy.Web.Features.ChatHistory;

public partial class ChatHistory : IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = null!;

    private GetChatHistoryListResponse? _response;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _response = await Mediator.Send(new GetChatHistoryListQuery(), _cts.Token);
    }

    public void NavigateToExistingChat(long Id)
    {

        NavigationManager.NavigateTo("/" + Id);
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

        var chatHistoryItems = _mapper.Map<ChatHistoryItem[]>(chatHistories);

        return new GetChatHistoryListResponse
        {
            ChatHistories = chatHistoryItems
        };
    }
}

public class GetChatHistoryListResponse
{
    public required IEnumerable<ChatHistoryItem> ChatHistories { get; set; } = new HashSet<ChatHistoryItem>();

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