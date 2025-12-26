using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GOKCafe.Infrastructure.Data;
using System.IO;

namespace GOKCafe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DatabaseController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("execute-sql-file")]
    public async Task<IActionResult> ExecuteSqlFile([FromQuery] string fileName)
    {
        try
        {
            var scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Scripts");
            var filePath = Path.Combine(scriptsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"SQL file not found: {fileName}");
            }

            var sqlContent = await System.IO.File.ReadAllTextAsync(filePath);

            // Split by GO statements
            var batches = sqlContent.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO\n", "\nGO\r\n" },
                StringSplitOptions.RemoveEmptyEntries);

            var results = new List<string>();

            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(batch);
                        results.Add($"Batch executed successfully");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"Batch failed: {ex.Message}");
                    }
                }
            }

            return Ok(new
            {
                success = true,
                fileName = fileName,
                batchesExecuted = batches.Length,
                results = results
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}
