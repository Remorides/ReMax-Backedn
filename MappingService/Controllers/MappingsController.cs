﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MappingService.Data;
using MappingService.Models;
using Microsoft.AspNetCore.Authorization;
using MappingService.Enums;

namespace MappingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MappingsController : ControllerBase
    {
        private readonly MappingDbContext _context;

        public MappingsController(MappingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntityMapping>>> GetMappings(
            [FromQuery] bool? isActive = null,
            [FromQuery] EntityType? entityType = null)
        {
            var query = _context.Mappings.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            if (entityType.HasValue)
            {
                query = query.Where(m => m.Type == entityType.Value);
            }

            var mappings = await query.ToListAsync();
            mappings.ForEach(m => m.DeserializeComplexData());
            return mappings;
        }

        [HttpPost]
        public async Task<ActionResult<EntityMapping>> PostMappingEntry([FromBody] EntityMapping mapping)
        {
            mapping.SerializeComplexData();

            _context.Mappings.Add(mapping);
            await _context.SaveChangesAsync();

            mapping.DeserializeComplexData();

            return CreatedAtAction("GetMapping", new { guid = mapping.Guid }, mapping);
        }

        [HttpGet("{guid}")]
        public async Task<ActionResult<EntityMapping>> GetMapping(Guid guid)
        {
            var mapping = await _context.Mappings.FindAsync(guid);

            if (mapping == null)
            {
                return NotFound();
            }
            mapping.DeserializeComplexData();
            return mapping;
        }

        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteMapping(Guid guid)
        {
            var user = await _context.Mappings.FindAsync(guid);
            if (user == null)
            {
                return NotFound();
            }

            _context.Mappings.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}