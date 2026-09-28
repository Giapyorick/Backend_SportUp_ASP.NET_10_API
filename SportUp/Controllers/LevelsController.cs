using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportUp.Controllers
{
    /// <summary>
    /// CRUD APIs for Level entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LevelsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public LevelsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all levels.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Level>>> GetLevels()
        {
            var items = await _dbContext.Levels.OrderByDescending(x => x.Id).AsNoTracking().ToListAsync();
            return Ok(items);
        }

        /// <summary>
        /// Get a level by id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Level>> GetLevel(int id)
        {
            var item = await _dbContext.Levels.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Create a new level.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Level>> CreateLevel([FromBody] Level level)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _dbContext.Levels.Add(level);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLevel), new { id = level.Id }, level);
        }

        /// <summary>
        /// Update an existing level.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateLevel(int id, [FromBody] Level level)
        {
            if (id != level.Id) return BadRequest();
            var existing = await _dbContext.Levels.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = level.Name;
            existing.Description = level.Description;
            existing.Status = level.Status;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete a single level by id.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLevel(int id)
        {
            var existing = await _dbContext.Levels.FindAsync(id);
            if (existing == null) return NotFound();

            _dbContext.Levels.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// import levels from an Excel file.
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("The file cannot be empty.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest("Only accept file (.xlsx or .xls).");
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var levels = new List<Level>();

            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var table = result.Tables[0]; // take first sheet

                    // Skip the header row (0) and start processing from row 1
                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];

                        var name = row[0]?.ToString()?.Trim();
                        var description = row[1]?.ToString()?.Trim();
                        var status = row[2]?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(name))
                        {
                            levels.Add(new Level
                            {
                                Name = name,
                                Description = description ?? "",
                                Status = string.IsNullOrEmpty(status) ? "Active" : status
                            });
                        }
                    }
                }
            }

            if (!levels.Any())
            {
                return BadRequest("File Excel does not contain the valid date.");
            }

            await _dbContext.Levels.AddRangeAsync(levels);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Imported successfully {levels.Count} levels.",
                count = levels.Count
            });
        }


        /// <summary>
        /// multi-delete levels by a list of ids.
        /// </summary>

        [HttpPost("multi-delete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            var items = await _dbContext.Levels.Where(x => ids.Contains(x.Id)).ToListAsync();
            _dbContext.Levels.RemoveRange(items);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = $"Deleted {items.Count} items successfully!" });
        }
    }
}
