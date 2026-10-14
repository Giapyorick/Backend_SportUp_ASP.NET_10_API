using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SportUp.Controllers
{
    /// <summary>
    /// CRUD APIs for Venue entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
        {
            var items = await _dbContext.Venues
                .OrderByDescending(x => x.Id)
                .AsNoTracking()
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// Get a venue by id.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<ActionResult<Venue>> GetVenue(int id)
        {
            var item = await _dbContext.Venues.FindAsync(id);
            if (item == null) return NotFound(new { message = $"Venue with Id {id} not found." });

            return Ok(item);
        }

        /// <summary>
        /// Create a new venue.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Venue>> CreateVenue([FromBody] Venue venue)
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateVenue(int id, [FromBody] Venue venue)
        {
            if (id != venue.Id) return BadRequest(new { message = "ID mismatch." });

            var existing = await _dbContext.Venues.FindAsync(id);
            if (existing == null) return NotFound(new { message = $"Venue with Id {id} not found." });

            existing.Name = venue.Name;
            existing.Address = venue.Address;
            existing.MapUrl = venue.MapUrl;
            existing.Status = venue.Status;

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Updated venue successfully!" });
        }

        /// <summary>
        /// Delete a venue by id.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var existing = await _dbContext.Venues.FindAsync(id);
            if (existing == null) return NotFound(new { message = $"Venue with Id {id} not found." });

            _dbContext.Venues.Remove(existing);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = $"Deleted venue {id} successfully." });
        }

        /// <summary>
        /// Import venues from an Excel file.
        /// </summary>
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "The file cannot be empty." });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
            {
                return BadRequest(new { message = "Only accept Excel files (.xlsx or .xls)." });
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
                return BadRequest(new { message = "Excel file does not contain valid data." });
            }

            await _dbContext.Venues.AddRangeAsync(venues);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"Imported successfully {venues.Count} venues.",
                count = venues.Count
            });
        }

        /// <summary>
        /// Multi-delete venues by a list of IDs.
        /// </summary>
        [HttpPost("multi-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new { message = "The ID list cannot be empty." });
            }

            var items = await _dbContext.Venues.Where(x => ids.Contains(x.Id)).ToListAsync();
            if (!items.Any())
            {
                return NotFound(new { message = "No matching venues found for deletion." });
            }

            _dbContext.Venues.RemoveRange(items);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = $"Deleted {items.Count} venues successfully!" });
        }
    }
}