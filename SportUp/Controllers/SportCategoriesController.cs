using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.Entities;

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

    }
}
