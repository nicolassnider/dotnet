using System.Collections.Concurrent;
using DotNetMastery.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetMastery.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, UserResponse> UsersStore = new();
    private static int _nextId = 1;

    static UsersController()
    {
        // Initial mock users
        UsersStore[1] = new(1, "Alice Smith", "alice@example.com", 28, DateTime.UtcNow);
        UsersStore[2] = new(2, "Bob Jones", "bob@example.com", 35, DateTime.UtcNow);
        _nextId = 3;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserResponse>> GetAll()
    {
        return Ok(UsersStore.Values);
    }

    [HttpGet("{id:int}")]
    public ActionResult<UserResponse> GetById(int id)
    {
        if (UsersStore.TryGetValue(id, out var user))
        {
            return Ok(user);
        }

        return NotFound(new { error = $"User with ID {id} not found." });
    }

    [HttpPost]
    public ActionResult<UserResponse> Create([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        int newId = Interlocked.Increment(ref _nextId);
        var createdUser = new UserResponse(newId, request.Name, request.Email, request.Age, DateTime.UtcNow);
        UsersStore[newId] = createdUser;

        return CreatedAtAction(nameof(GetById), new { id = newId }, createdUser);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (UsersStore.TryRemove(id, out _))
        {
            return NoContent();
        }

        return NotFound(new { error = $"User with ID {id} not found." });
    }
}
