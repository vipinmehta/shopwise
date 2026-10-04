using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> Search([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _adminUserService.SearchAsync(search, page, pageSize));

    [HttpPut("{id}/active")]
    public async Task<IActionResult> SetActive(string id, [FromBody] bool isActive)
    {
        await _adminUserService.SetActiveAsync(id, isActive);
        return Ok(new { message = "Updated" });
    }
}
