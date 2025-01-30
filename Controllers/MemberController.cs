using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace practiceApi.Controllers
{
    public class MemberController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public static List<Member> Members = new List<Member>();

        public MemberController(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        [HttpPost("/api/members")]
        public async Task<IActionResult> Create([FromBody] MemberDto memberDto)
        {
            try
            {
                const string insertQuery = @"
                                        insert into members(first_name,email,phone_number,address)
                                         values(@firstName, @email, @phone_number, @address)
                                            ";
                var connectionString = _configuration.GetConnectionString("Default");
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.ExecuteAsync(insertQuery, new
                {
                    firstName = memberDto.FirstName,
                    email = memberDto.Email,
                    phone_number = memberDto.PhoneNumber,
                    address = memberDto.Address
                });
                return Ok("Member created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/api/members")]
        public IActionResult GetAll([FromQuery] MemberFilterDto filter)
        {
            try
            {
                var members = Members.Where(x =>
                    (string.IsNullOrEmpty(filter.FirstName) ||
                     x.FirstName.Contains(filter.FirstName, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(filter.Address) ||
                     x.Address.Contains(filter.Address, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/api/members/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                const string selectQuery = @"
                                        select * from members where id = @id
                                            ";
                
                var connectionString = _configuration.GetConnectionString("Default");
                await using var connection = new NpgsqlConnection(connectionString);
                var member = await connection.QuerySingleOrDefaultAsync<Member>(selectQuery, new { id });
                if (member == null)
                {
                    return NotFound("Member not found");
                }
                return Ok(member);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("/api/members/{id}")]
        public IActionResult Update(int id, [FromBody] MemberDto memberDto)
        {
            try
            {
                var member = Members.FirstOrDefault(m => m.Id == id);
                if (member == null)
                {
                    return NotFound($"Member with ID {id} not found.");
                }

                member.FirstName = memberDto.FirstName;
                member.Email = memberDto.Email;
                member.PhoneNumber = memberDto.PhoneNumber;
                member.Address = memberDto.Address;

                return Ok("Member updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("/api/members/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var member = Members.FirstOrDefault(m => m.Id == id);
                if (member == null)
                {
                    return NotFound($"Member with ID {id} not found.");
                }

                Members.Remove(member);
                return Ok("Member deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

public class Member
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

public class MemberDto
{
    public string FirstName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

public class MemberFilterDto
{
    public string FirstName { get; set; }
    public string Address { get; set; }
}