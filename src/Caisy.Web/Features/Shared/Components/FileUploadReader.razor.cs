using Microsoft.AspNetCore.Components.Forms;

namespace Caisy.Web.Features.Shared.Components;

public partial class FileUploadReader : IDisposable
{
    [Parameter] public string ReadContents { get; set; } = null!;
    [Parameter] public EventCallback<string> ReadContentsChanged { get; set; }
    [Parameter] public string? Class { get; set; }

    private readonly CancellationTokenSource _cts = new();

    private async Task OnFileUploadAsync(InputFileChangeEventArgs e)
    {
        if (e.FileCount == 0) return;

        var file = e.File;
        using var streamReader = new StreamReader(file.OpenReadStream());

        var fileContent = await streamReader.ReadToEndAsync(_cts.Token);

        await ReadContentsChanged.InvokeAsync(fileContent);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
