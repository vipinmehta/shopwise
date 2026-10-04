using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Common;

public static class ControllerExtensions
{
    public static string GetUserId(this ControllerBase controller) =>
        controller.User.FindFirstValue(JwtRegisteredClaimNamesSub) ?? controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User id claim missing");

    private const string JwtRegisteredClaimNamesSub = "sub";

    public static string GetUserName(this ControllerBase controller) =>
        controller.User.FindFirstValue(ClaimTypes.Name) ?? "User";
}
