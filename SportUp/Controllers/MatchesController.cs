using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.DTO;
using SportUp.Entities;

namespace SportUp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MatchesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all matches with relations included.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Match>>> GetMatches()
        {
            var matches = await _dbContext.Matches
                .Include(m => m.SportCategory)
                .Include(m => m.Venue)
                .Include(m => m.TargetLevel)
                .OrderByDescending(m => m.Id)
                .AsNoTracking()
                .Select(m => new Match
                {
                    Id = m.Id,
                    Title = m.Title,
                    SportCategoryId = m.SportCategoryId,
                    SportCategory = m.SportCategory != null ? m.SportCategory : null,
                    VenueId = m.VenueId,
                    Venue = m.Venue != null ? m.Venue : null,
                    TargetLevelId = m.TargetLevelId,
                    TargetLevel = m.TargetLevel != null ? m.TargetLevel : null,
                    StartTime = m.StartTime,
                    EndTime = m.EndTime,
                    TotalSlots = m.TotalSlots,
                    AvailableSlots = m.AvailableSlots,
                    PricePerSlot = m.PricePerSlot,
                    Note = m.Note,
                    CreatedByUserId = m.CreatedByUserId,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(matches);
        }

        /// <summary>
        /// Get match details by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Match>> GetMatch(int id)
        {
            var m = await _dbContext.Matches
                .Include(x => x.SportCategory)
                .Include(x => x.Venue)
                .Include(x => x.TargetLevel)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null) return NotFound(new { message = $"Match with Id {id} not found." });

            var items = new Match
            {
                Id = m.Id,
                Title = m.Title,
                SportCategoryId = m.SportCategoryId,
                SportCategory = m.SportCategory ?? new SportCategory { Name = "Null" },
                VenueId = m.VenueId,
                Venue = m.Venue ?? new Venue { Name = "Null" },
                TargetLevelId = m.TargetLevelId,
                TargetLevel = m.TargetLevel ?? new Level { Name = "Null" },
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                TotalSlots = m.TotalSlots,
                AvailableSlots = m.AvailableSlots,
                PricePerSlot = m.PricePerSlot,
                Note = m.Note,
                CreatedByUserId = m.CreatedByUserId,
                CreatedAt = m.CreatedAt
            };

            return Ok(items);
        }

        /// <summary>
        /// Create a new match.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Match>> CreateMatch([FromBody] Match item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var match = new Match
            {
                Title = item.Title,
                SportCategoryId = item.SportCategoryId,
                VenueId = item.VenueId,
                TargetLevelId = item.TargetLevelId,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                TotalSlots = item.TotalSlots,
                AvailableSlots = item.AvailableSlots > 0 ? item.AvailableSlots : item.TotalSlots,
                PricePerSlot = item.PricePerSlot,
                Note = item.Note,
                CreatedByUserId = "SYSTEM_MOCK_USER",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Matches.Add(match);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
        }

        /// <summary>
        /// Update an existing match.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMatch(int id, [FromBody] Match item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var match = await _dbContext.Matches.FindAsync(id);
            if (match == null) return NotFound(new { message = $"Match with Id {id} not found." });

            match.Title = item.Title;
            match.SportCategoryId = item.SportCategoryId;
            match.VenueId = item.VenueId;
            match.TargetLevelId = item.TargetLevelId;
            match.StartTime = item.StartTime;
            match.EndTime = item.EndTime;
            match.TotalSlots = item.TotalSlots;
            match.AvailableSlots = item.AvailableSlots;
            match.PricePerSlot = item.PricePerSlot;
            match.Note = item.Note;

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Updated successfully!" });
        }

        /// <summary>
        /// Delete a single match by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var match = await _dbContext.Matches.FindAsync(id);
            if (match == null) return NotFound(new { message = $"Match with Id {id} not found." });

            _dbContext.Matches.Remove(match);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = $"Deleted match {id} successfully." });
        }

        /// <summary>
        /// Multi-delete matches.
        /// </summary>
        [HttpPost("multi-delete")]
        public async Task<IActionResult> DeleteMultipleMatches([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest(new { message = "No IDs provided." });

            var items = await _dbContext.Matches.Where(x => ids.Contains(x.Id)).ToListAsync();
            if (!items.Any()) return NotFound(new { message = "No matching items found." });

            _dbContext.Matches.RemoveRange(items);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = $"Deleted {items.Count} matches successfully!" });
        }

        // DROPDOWN OPTIONS APIS (Id, Name)

        /// <summary>
        /// Get categories options for dropdown select.
        /// </summary>
        [HttpGet("options/categories")]
        public async Task<ActionResult<IEnumerable<DropdownOptionDto>>> GetCategoryOptions()
        {
            var options = await _dbContext.SportCategories
                .AsNoTracking()
                .Select(c => new DropdownOptionDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(options);
        }

        /// <summary>
        /// Get venues options for dropdown select.
        /// </summary>
        [HttpGet("options/venues")]
        public async Task<ActionResult<IEnumerable<DropdownOptionDto>>> GetVenueOptions()
        {
            var options = await _dbContext.Venues
                .AsNoTracking()
                .Select(v => new DropdownOptionDto
                {
                    Id = v.Id,
                    Name = v.Name
                })
                .ToListAsync();

            return Ok(options);
        }

        /// <summary>
        /// Get levels options for dropdown select.
        /// </summary>
        [HttpGet("options/levels")]
        public async Task<ActionResult<IEnumerable<DropdownOptionDto>>> GetLevelOptions()
        {
            var options = await _dbContext.Levels
                .AsNoTracking()
                .Select(l => new DropdownOptionDto
                {
                    Id = l.Id,
                    Name = l.Name
                })
                .ToListAsync();

            return Ok(options);
        }
    }
}