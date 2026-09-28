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
    /// CRUD APIs for Venue entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public VenuesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all venues.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
        {
            var items = await _dbContext.Venues.OrderByDescending(x => x.Id).AsNoTracking().ToListAsync();
            return Ok(items);
        }

        /// <summary>
        /// Get a venue by id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Venue>> GetVenue(int id)
        {
            var item = await _dbContext.Venues.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>
        /// Create a new venue.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Venue>> CreateVenue([FromBody] Venue venue   )
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _dbContext.Venues.Add(venue);
            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVenue), new { id = venue.Id }, venue);
        }

        /// <summary>
        /// Update an existing venue.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateVenue(int id, [FromBody] Venue venue)
        {
            if (id != venue.Id) return BadRequest();
            var existing = await _dbContext.Venues.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = venue.Name;
            existing.Address = venue.Address;
            existing.MapUrl = venue.MapUrl;
            existing.Status = venue.Status;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete a venue by id.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var existing = await _dbContext.Venues.FindAsync(id);
            if (existing == null) return NotFound();

            _dbContext.Venues.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return NoContent();
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

            var venues = new List<Venue>();

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
                        var address = row[1]?.ToString()?.Trim();
                        var status = row[2]?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(name))
                        {
                            venues.Add(new Venue
                            {
                                Name = name,
                                Address = address ?? "",
                                Status = string.IsNullOrEmpty(status) ? "Active" : status
                            });
                        }
                    }
                }
            }

            if (!venues.Any())
            {
                return BadRequest("File Excel does not contain the valid data.");
            }

            await _dbContext.Venues.AddRangeAsync(venues);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Imported successfully {venues.Count} venues.",
                count = venues.Count
            });
        }

        // Xóa nhiều
        [HttpPost("multi-delete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            var items = await _dbContext.Venues.Where(x => ids.Contains(x.Id)).ToListAsync();
            _dbContext.Venues.RemoveRange(items);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = $"Deleted {items.Count} items successfully!" });
        }
    }
}
