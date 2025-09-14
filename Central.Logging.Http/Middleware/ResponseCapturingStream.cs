using System.Text;

namespace Central.Logging.Http.Middleware;

/// <summary>
/// Custom stream wrapper for capturing response content while writing to the original stream
/// </summary>
public class ResponseCapturingStream(Stream originalStream, int maxCaptureSize) : Stream
{
    private readonly Stream _originalStream =
        originalStream ?? throw new ArgumentNullException(nameof(originalStream));
    private readonly MemoryStream _capturedContent = new();
    private readonly int _maxCaptureSize = maxCaptureSize;
    private bool _disposed;

    public string GetCapturedContent()
    {
        if (_capturedContent.Length == 0)
            return string.Empty;

        try
        {
            var bytes = _capturedContent.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public override async Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    )
    {
        // Always write to original stream first
        await _originalStream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

        // Capture for logging if within size limits and not disposed
        if (!_disposed && _capturedContent.Length + count <= _maxCaptureSize)
        {
            try
            {
                await _capturedContent.WriteAsync(
                    buffer.AsMemory(offset, count),
                    cancellationToken
                );
            }
            catch
            {
                // Ignore capture errors - don't let them affect the response
            }
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        // Always write to original stream first
        _originalStream.Write(buffer, offset, count);

        // Capture for logging if within size limits and not disposed
        if (!_disposed && _capturedContent.Length + count <= _maxCaptureSize)
        {
            try
            {
                _capturedContent.Write(buffer, offset, count);
            }
            catch
            {
                // Ignore capture errors - don't let them affect the response
            }
        }
    }

    // Forward all other operations to the original stream
    public override bool CanRead => _originalStream.CanRead;
    public override bool CanSeek => _originalStream.CanSeek;
    public override bool CanWrite => _originalStream.CanWrite;
    public override long Length => _originalStream.Length;

    public override long Position
    {
        get => _originalStream.Position;
        set => _originalStream.Position = value;
    }

    public override void Flush() => _originalStream.Flush();

    public override async Task FlushAsync(CancellationToken cancellationToken) =>
        await _originalStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) =>
        _originalStream.Read(buffer, offset, count);

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    ) => await _originalStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) =>
        _originalStream.Seek(offset, origin);

    public override void SetLength(long value) => _originalStream.SetLength(value);

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _capturedContent?.Dispose();
            // Note: We don't dispose the original stream as it's owned by the HTTP context
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_capturedContent != null)
                await _capturedContent.DisposeAsync();

            // Note: We don't dispose the original stream as it's owned by the HTTP context
        }

        GC.SuppressFinalize(this);
        await base.DisposeAsync();
    }
}
