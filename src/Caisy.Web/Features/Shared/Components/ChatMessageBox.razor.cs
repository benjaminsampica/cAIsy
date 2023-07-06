namespace Caisy.Web.Features.Shared.Components;
public partial class ChatMessageBox : ComponentBase
{
    [Parameter] public string? MessageButtonText { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public EventCallback<bool> LoadingChanged { get; set; }
    [Parameter] public string? Message { get; set; }
    [Parameter] public EventCallback<string?> MessageChanged { get; set; }
    [Parameter] public RenderFragment? ToolbarContent { get; set; }
}
