using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace practiceApi.Controllers;

public class DepartmentController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DepartmentController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("api/department")]
    public async Task<IActionResult> Create([FromBody] DeptDto department)
    {
        try
        {
            const string insertQuery = @"
                                    INSERT INTO Department (name, address,phone,email) 
                                    VALUES (@Name, @Address,@Phone,@Email);
                                    ";
            var connectionString = _configuration.GetConnectionString("Default");
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.ExecuteAsync(
                insertQuery, new
                {
                    Name = department.Name,
                    Address = department.Address,
                    Phone = department.Phone,
                    Email = department.Email
                });
            return Ok("created member in department successfully");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("/api/department/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            const string selectQuery = @"
                                    SELECT * FROM Department WHERE id = @Id;
                                    ";
            var connectionString = _configuration.GetConnectionString("Default");
            await using var connection = new NpgsqlConnection(connectionString);
            var department = await connection.QueryFirstOrDefaultAsync<Department>(selectQuery, new { Id = id });
            

            if (department == null)
            {
                return NotFound("Department not found");
            }

            return Ok(department);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}

public class DeptDto
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}