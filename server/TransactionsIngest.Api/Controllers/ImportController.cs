using Microsoft.AspNetCore.Mvc;
using TransactionsIngest.Api.DTOs;
using TransactionsIngest.Api.Interfaces;

namespace TransactionsIngest.Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly ITransactionImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        ITransactionImportService importService,
        ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImportResultDto>> Import(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _importService.ImportAsync(cancellationToken);
            return Ok(result);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Import request was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during transaction import.");

            return Problem(
                title: "Transaction import failed",
                detail: "An error occurred while processing the import.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
