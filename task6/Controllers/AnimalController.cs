using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using task6.Models;


namespace task6.Controllers
{
    [ApiController]
    public class AnimalController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public AnimalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/animals")]
        public IActionResult GetAnimals(string orderBy = "Name")
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            //Console.WriteLine(connectionString);  
            var query = "SELECT * FROM master.dbo.Animal ORDER BY " + orderBy; 

            var allowedOrderBys = new HashSet<string>{"name", "description", "category", "area"};
            if (!allowedOrderBys.Contains(orderBy.ToLower()))
            {
                return BadRequest("Invalid order by value");
            }

            List<Animal> animals = new List<Animal>();
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var animal = new Animal
                            {
                                IdAnimal = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Category = reader.GetString(3),
                                Area = reader.GetString(4),
                            };
                            animals.Add(animal);
                        }

                    }
                }
                catch (SqlException e)
                {
                    return StatusCode(500, "Database error: " + e.Message);
                }
            }

            return Ok(animals);
        }
        
        [HttpGet("api/animals/{id}")]
        public IActionResult GetAnimalById(int id)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            Animal animal = null;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("SELECT * FROM master.dbo.Animal WHERE IdAnimal = @IdAnimal", connection);
                command.Parameters.AddWithValue("@IdAnimal", id);

                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            animal = new Animal
                            {
                                IdAnimal = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Category = reader.GetString(3),
                                Area = reader.GetString(4),
                            };
                        }
                    }
                }
                catch (SqlException e)
                {
                    return StatusCode(500, "Database error: " + e.Message);
                }
            }

            if (animal == null)
            {
                // 404:
                return NotFound($"Animal with ID {id} is not found.");
            }

            return Ok(animal);
        }
        
        [HttpPost("api/animals")]
        public IActionResult AddAnimal([FromBody] Animal newAnimal)
        {
            if (newAnimal == null)
            {
                return BadRequest("Animal data is null.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("INSERT INTO master.dbo.Animal (IdAnimal, Name, Description, Category, Area) VALUES (@IdAnimal, @Name, @Description, @Category, @Area)", connection);
                command.Parameters.AddWithValue("@IdAnimal", newAnimal.IdAnimal);
                command.Parameters.AddWithValue("@Name", newAnimal.Name);
                command.Parameters.AddWithValue("@Description", newAnimal.Description);
                command.Parameters.AddWithValue("@Category", newAnimal.Category);
                command.Parameters.AddWithValue("@Area", newAnimal.Area);

                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result < 0)
                        return StatusCode(500, "Error inserting data into database.");

                }
                catch (SqlException e)
                {
                    return StatusCode(500, "Database error: " + e.Message);
                }
            }
            return CreatedAtAction(nameof(GetAnimalById), new { id = newAnimal.IdAnimal }, newAnimal);
        }

        [HttpPut("api/animals/{id}")]
        public IActionResult UpdateOrAddAnimal(int id, [FromBody] Animal animal)
        {
            if (animal == null)
            {
                return BadRequest("Animal data is null.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var checkCommand = new SqlCommand("SELECT COUNT(1) FROM master.dbo.Animal WHERE IdAnimal = @IdAnimal", connection);
                checkCommand.Parameters.AddWithValue("@IdAnimal", id);

                bool exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                SqlCommand command;
                if (!exists)
                {
                    command = new SqlCommand("INSERT INTO master.dbo.Animal (IdAnimal, Name, Description, Category, Area) VALUES (@IdAnimal, @Name, @Description, @Category, @Area)", connection);
                }
                else
                {
                    command = new SqlCommand("UPDATE master.dbo.Animal SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal", connection);
                }

                command.Parameters.AddWithValue("@IdAnimal", id);
                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Description", animal.Description ?? (object)DBNull.Value); 
                command.Parameters.AddWithValue("@Category", animal.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Area", animal.Area ?? (object)DBNull.Value);

                int result = command.ExecuteNonQuery();

                if (result <= 0)
                {
                    return StatusCode(500, "Error updating database.");
                }

                if (!exists)
                {
                    return CreatedAtAction(nameof(GetAnimalById), new { id = animal.IdAnimal }, animal);
                }

                return NoContent(); 
            }
        }

    }
}