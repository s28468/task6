using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task6.Models;
using task6.Data;
using System.Linq.Expressions;

namespace task6.Controllers
{
    [ApiController]
    public class AnimalController : ControllerBase
    {
        private readonly AnimalContext _context;
        
        public AnimalController(AnimalContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("api/animals")]
        public async Task<IActionResult> GetAnimals(string orderBy = "Name")
        {
            var allowedOrderBys = new HashSet<string>{"name", "description", "category", "area"};
            if (!allowedOrderBys.Contains(orderBy.ToLower()))
            {
                return BadRequest("Invalid order by value");
            }

            var animals = await _context.Animals.OrderBy(GetOrderByExpression(orderBy)).ToListAsync();
            return Ok(animals);
        }

        [HttpGet("api/animals/{id}")]
        public async Task<IActionResult> GetAnimalById(int id)
        {
            var animal = await _context.Animals.FindAsync(id);

            if (animal == null)
            {
                return NotFound($"Animal with ID {id} is not found.");
            }

            return Ok(animal);
        }

        
        [HttpPost("api/animals")]
        public async Task<IActionResult> AddAnimal([FromBody] Animal newAnimal)
        {
            if (newAnimal == null)
            {
                return BadRequest("Animal data is null.");
            }

            _context.Animals.Add(newAnimal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnimalById), new { id = newAnimal.IdAnimal }, newAnimal);
        }

        [HttpPut("api/animals/{id}")]
        public async Task<IActionResult> UpdateOrAddAnimal(int id, [FromBody] Animal animal)
        {
            if (animal == null)
            {
                return BadRequest("Animal data is null.");
            }

            var existingAnimal = await _context.Animals.FindAsync(id);
            if (existingAnimal == null)
            {
                _context.Animals.Add(animal);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetAnimalById), new { id = animal.IdAnimal }, animal);
            }

            existingAnimal.Name = animal.Name;
            existingAnimal.Description = animal.Description ?? existingAnimal.Description;
            existingAnimal.Category = animal.Category ?? existingAnimal.Category;
            existingAnimal.Area = animal.Area ?? existingAnimal.Area;

            _context.Animals.Update(existingAnimal);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }

        private static Expression<Func<Animal, object>> GetOrderByExpression(string orderBy)
        {
            switch (orderBy.ToLower())
            {
                case "name":
                    return animal => animal.Name;
                case "description":
                    return animal => animal.Description;
                case "category":
                    return animal => animal.Category;
                case "area":
                    return animal => animal.Area;
                default:
                    throw new ArgumentException("Invalid order by value");
            }
        }
    }
}
