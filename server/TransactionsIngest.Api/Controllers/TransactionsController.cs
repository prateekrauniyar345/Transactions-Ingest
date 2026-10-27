using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Data;
using TransactionsIngest.Api.DTOs;

namespace TransactionsIngest.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        AppDbContext dbContext,
        ILogger<TransactionsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactions(
        CancellationToken cancellationToken)
    {
        try
        {
            // Return both active and revoked records without EF navigation properties.
            var transactions = await _dbContext.Transactions
                .AsNoTracking()
                .OrderBy(t => t.SourceSystem)
                .ThenBy(t => t.TransactionId)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    SourceSystem = t.SourceSystem,
                    TransactionId = t.TransactionId,
                    CardNumber = t.CardNumber,
                    LocationCode = t.LocationCode,
                    ProductName = t.ProductName,
                    Amount = t.Amount,
                    // SQLite does not retain DateTime.Kind; stored times are UTC.
                    TransactionTime = DateTime.SpecifyKind(t.TransactionTime, DateTimeKind.Utc),
                    Status = t.Status.ToString()
                })
                .ToListAsync(cancellationToken);

            return Ok(transactions);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not retrieve transactions.");
            return Problem(
                title: "Could not retrieve transactions",
                detail: "An error occurred while retrieving transactions.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
