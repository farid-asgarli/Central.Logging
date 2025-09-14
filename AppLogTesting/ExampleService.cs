using Central.Logging.Abstractions;

namespace AppLogTesting;

public class ExampleService(IBaseLogger logger)
{
    private readonly IBaseLogger _logger = logger;

    public void DoWork()
    {
        _logger.LogInformation("BusinessLogic", "Starting work operation");

        try
        {
            // Simulate some work
            Thread.Sleep(100);
            _logger.LogDebug("BusinessLogic", "Work operation in progress");

            // Simulate an error condition
            if (DateTime.Now.Millisecond % 2 == 0)
            {
                throw new InvalidOperationException("Sample exception for demonstration");
            }

            _logger.LogInformation("BusinessLogic", "Work operation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("BusinessLogic", ex, "Work operation failed: {Error}", ex.Message);
        }
    }
}
