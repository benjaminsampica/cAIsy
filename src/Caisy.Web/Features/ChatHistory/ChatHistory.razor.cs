namespace Caisy.Web.Features.ChatHistory;

public partial class ChatHistory : IDisposable
{
    [Inject] private IRepository<ChatDetail> ChatDetailRepository { get; set; } = null!;

    private IEnumerable<ChatDetail> _details = Array.Empty<ChatDetail>();
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _details = await ChatDetailRepository.GetAllAsync(_cts.Token);
    }

    public void NavigateToHome(CellContext<ChatDetail> chatDetail)
    {
        NavigationManager.NavigateTo("/" + chatDetail.Item.Id);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
