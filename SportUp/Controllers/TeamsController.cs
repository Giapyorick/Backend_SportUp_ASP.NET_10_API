using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.DTO;
using SportUp.Entities;

namespace SportUp.Controllers
{
    /// <summary>
    /// CRUD APIs for teams entity.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : Controller
    {
        private readonly AppDbContext _dbContext;
        public TeamsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        /// <summary>
        /// Get all teams
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            var items = await _dbContext.Teams.OrderByDescending(x => x.Id).AsNoTracking().ToListAsync();
            return Ok(items);
        }
        /// <summary>
        /// Get team by id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            var item = await _dbContext.Teams.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        /// <summary>
        /// Create a new team with optional image upload. 
        /// The image will be saved to the server and its URL will be stored in the database. 
        /// If any error occurs during the process, the transaction will be rolled back and any uploaded image will be deleted.
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Team>> CreateTeam([FromForm] TeamCreateDto teams)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Init Database Transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            string? savedFilePath = null;

            try
            {
                var team = new Team
                {
                    Name = teams.Name,
                    Description = teams.Description,
                    CreatedAt = DateTime.UtcNow,
                    Status = teams.Status
                };

                // 3. Process Image Upload (if provided)
                if (teams.LogoUrl != null && teams.LogoUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "teams");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{teams.LogoUrl.FileName}";
                    savedFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(savedFilePath, FileMode.Create))
                    {
                        await teams.LogoUrl.CopyToAsync(stream);
                    }

                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    team.LogoUrl = $"{baseUrl}/uploads/teams/{uniqueFileName}";
                }

                // 4. Add to Database
                _dbContext.Teams.Add(team);
                await _dbContext.SaveChangesAsync();

                // 5. Commit Transaction if everything is successful
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
            }
            catch (Exception ex)
            {
                // 6. Rollback Database Transaction
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(savedFilePath) && System.IO.File.Exists(savedFilePath))
                {
                    System.IO.File.Delete(savedFilePath);
                }

                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        /// <summary>
        /// Update an existing team by ID with optional image upload.
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateTeam(int id, [FromForm] TeamCreateDto dto)
        {
            var team = await _dbContext.Teams.FindAsync(id);
            if (team == null)
                return NotFound($"Not Found team Id = {id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            string? newSavedFilePath = null;
            string? oldFilePathToDelete = null;

            try
            {
                // 1. Update fields from DTO to Entity
                team.Name = dto.Name;
                team.Description = dto.Description;
                team.Status = dto.Status;

                // 2. If a new image is uploaded, process it
                if (dto.LogoUrl != null && dto.LogoUrl.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "teams");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{dto.LogoUrl.FileName}";
                    newSavedFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(newSavedFilePath, FileMode.Create))
                    {
                        await dto.LogoUrl.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(team.LogoUrl))
                    {
                        var oldFileName = Path.GetFileName(team.LogoUrl);
                        oldFilePathToDelete = Path.Combine(uploadsFolder, oldFileName);
                    }

                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    team.LogoUrl = $"{baseUrl}/uploads/teams/{uniqueFileName}";
                }

                // 3. Update Database
                _dbContext.Teams.Update(team);
                await _dbContext.SaveChangesAsync();

                // 4. Commit Transaction
                await transaction.CommitAsync();

                // 5. After successful commit, delete the old image file if it exists
                if (!string.IsNullOrEmpty(oldFilePathToDelete) && System.IO.File.Exists(oldFilePathToDelete))
                {
                    System.IO.File.Delete(oldFilePathToDelete);
                }

                return NoContent(); // 204 Success
            }
            catch (Exception ex)
            {
                // Rollback DB
                await transaction.RollbackAsync();

                // delete the new uploaded file if error occurs
                if (!string.IsNullOrEmpty(newSavedFilePath) && System.IO.File.Exists(newSavedFilePath))
                {
                    System.IO.File.Delete(newSavedFilePath);
                }

                return StatusCode(500, $"Error updating team: {ex.Message}");
            }
        }
        /// <summary>
        /// Delete a team by ID. 
        /// If the team has an associated logo image, it will also be deleted from the server after the database transaction is successfully committed.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _dbContext.Teams.FindAsync(id);
            if (team == null)
                return NotFound($"Not Found team Id = {id}");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Find the file path to delete (if any)
                string? filePathToDelete = null;
                if (!string.IsNullOrEmpty(team.LogoUrl) && !team.LogoUrl.Contains("default_image.png"))
                {
                    var fileName = Path.GetFileName(team.LogoUrl);
                    filePathToDelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "teams", fileName);
                }

                // 2. Delete the team from the database
                _dbContext.Teams.Remove(team);
                await _dbContext.SaveChangesAsync();

                // 3. Commit Transaction
                await transaction.CommitAsync();

                // 4. After successfully deleting from DB, delete the image file from disk
                if (!string.IsNullOrEmpty(filePathToDelete) && System.IO.File.Exists(filePathToDelete))
                {
                    System.IO.File.Delete(filePathToDelete);
                }

                return NoContent(); // 204 Success
            }
            catch (Exception ex)
            {
                // Rollback when error occurs
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error when deleting team: {ex.Message}");
            }
        }
        /// <summary>
        /// Delete multiple teams by their IDs.
        /// </summary>
        [HttpPost("multi-delete")]
        public async Task<IActionResult> MultiDeleteTeams([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("IDs cannot be null or empty.");
            }

            // 1. Get all teams that match the provided IDs
            var teamsToDelete = await _dbContext.Teams
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (!teamsToDelete.Any())
            {
                return NotFound("No matching teams found for deletion.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 2. Take note of the file paths to delete after DB commit
                var filesToDelete = new List<string>();
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "teams");

                foreach (var team in teamsToDelete)
                {
                    if (!string.IsNullOrEmpty(team.LogoUrl) && !team.LogoUrl.Contains("default_image.png"))
                    {
                        var fileName = Path.GetFileName(team.LogoUrl);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        filesToDelete.Add(filePath);
                    }
                }

                // 3. Delete the teams from the database
                _dbContext.Teams.RemoveRange(teamsToDelete);
                await _dbContext.SaveChangesAsync();

                // 4. Commit Transaction
                await transaction.CommitAsync();

                // 5. After successful commit, delete the old image files if they exist
                foreach (var filePath in filesToDelete)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                var deletedIds = teamsToDelete.Select(t => t.Id).ToList();

                return Ok(new
                {
                    message = $"Successfully deleted {deletedIds.Count} teams.",
                    deletedCount = deletedIds.Count,
                    deletedIds = deletedIds
                });
            }
            catch (Exception ex)
            {
                // Rollback when error occurs
                await transaction.RollbackAsync();
                return StatusCode(500, $"System error when deleting multiple teams: {ex.Message}");
            }
        }
        /// <summary>
        /// Import teams from an Excel file. 
        /// The Excel file should have the following columns: Name, Description, Status. 
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel([FromForm] TeamCreateDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var defaultLogoUrl = $"{baseUrl}/uploads/teams/default_image.png";

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var teamsToAdd = new List<Team>();

            using (var stream = new MemoryStream())
            {
                await dto.File.CopyToAsync(stream);
                stream.Position = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    if (!reader.Read())
                    {
                        return BadRequest("File Excel empty.");
                    }

                    while (reader.Read())
                    {
                        var name = reader.GetValue(0)?.ToString()?.Trim();
                        var description = reader.GetValue(1)?.ToString()?.Trim() ?? string.Empty;
                        var status = reader.GetValue(2)?.ToString()?.Trim() ?? "active";

                        if (string.IsNullOrEmpty(name)) continue;

                        var team = new Team
                        {
                            Name = name,
                            Description = description,
                            Status = status,
                            LogoUrl = defaultLogoUrl, 
                            CreatedAt = DateTime.UtcNow 
                        };

                        teamsToAdd.Add(team);
                    }
                }
            }

            if (teamsToAdd.Count == 0)
            {
                return BadRequest("Not found valid data in the Excel file.");
            }

            await _dbContext.Teams.AddRangeAsync(teamsToAdd);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = $"Imported successfully {teamsToAdd.Count} teams!", count = teamsToAdd.Count });
        }
    }
}
