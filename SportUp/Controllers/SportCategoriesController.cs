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
    /// CRUD APIs for sportCategory entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SportCategoriesController : Controller
    {
        private readonly AppDbContext _dbContext;
        public SportCategoriesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        /// <summary>
        /// Get all sport categories.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SportCategory>>> GetSportCategories()
        {
            var items = await _dbContext.SportCategories.AsNoTracking().ToListAsync();
            return Ok(items);
        }
        /// <summary>
        /// Get a sport category by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SportCategory>> GetSportCategory(int id)
        {
            var item = await _dbContext.SportCategories.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        /// <summary>
        /// Create a new sport category.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SportCategory>> CreateSportCategory([FromBody] SportCategory sportCategory)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _dbContext.SportCategories.Add(sportCategory);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSportCategory), new { id = sportCategory.Id }, sportCategory);
        }
        /// <summary>
        /// update an existing sport category.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSportCategory(int id, [FromBody] SportCategory sportCategory)
        {
            if (id != sportCategory.Id) return BadRequest();
            var existing = await _dbContext.SportCategories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = sportCategory.Name;
            existing.Description = sportCategory.Description;
            existing.Status = sportCategory.Status;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Delete a sport category.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSportCategory(int id)
        {
            var existing = await _dbContext.SportCategories.FindAsync(id);
            if (existing == null) return NotFound();

            _dbContext.SportCategories.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        /// <summary>
        /// Delete multiple sport categories by a list of IDs.
        /// </summary>
        [HttpPost("multi-delete")] 
        public async Task<IActionResult> MultiDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("The ID list cannot be empty.");
            }

            var itemsToDelete = await _dbContext.SportCategories
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (!itemsToDelete.Any())
            {
                return NotFound("No matching data found to delete.");
            }

            _dbContext.SportCategories.RemoveRange(itemsToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Deleted successfully {itemsToDelete.Count} sportcategories.",
                deletedIds = ids
            });
        }
        /// <summary>
        /// Import sport categories from an Excel file.
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

            var categories = new List<SportCategory>();

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
                            categories.Add(new SportCategory
                            {
                                Name = name,
                                Description = description ?? "",
                                Status = string.IsNullOrEmpty(status) ? "Active" : status
                            });
                        }
                    }
                }
            }

            if (!categories.Any())
            {
                return BadRequest("File Excel does not contain the valid date.");
            }

            await _dbContext.SportCategories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Imported successfully {categories.Count} sport category.",
                count = categories.Count
            });
        }

    }
}
