using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.Entities;

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
            var items = await _dbContext.Levels.AsNoTracking().ToListAsync();
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

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete a level by id.
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
    }
}
