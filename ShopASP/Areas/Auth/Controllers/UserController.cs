using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopASP.Areas.Auth.Models;
using ShopASP.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ShopASP.Areas.Auth.Controllers;

[Area("Auth")]
[Authorize(Roles ="Admin")]
public class UserController : Controller
{
	private readonly ILogger<UserController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

	public UserController(ILogger<UserController> logger, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager)
	{
		_logger = logger;
		_userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
	}

	[HttpGet]
    public async Task<IActionResult> Index()
	{
		return View(await _userManager.Users.ToListAsync());
	}

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        var form = new UserEditForm();
        form.UserRoles = (await _userManager.GetRolesAsync(user)).ToList();
        ViewData["user"] = user;
        ViewData["roles"] = _roleManager.Roles.ToList();
        return View(form);
    }

	[HttpPost]
	public async Task<IActionResult> Edit(int id, [FromForm] UserEditForm form)
    {
		var user = await _userManager.FindByIdAsync(id.ToString());
		ViewData["user"] = user;
		ViewData["roles"] = _roleManager.Roles.ToList();
        if (form.Password != null)
        {
            if (form.Password != form.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(form.ConfirmPassword), "Passwords are not matching");
                return View(form);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, form.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(form.Password),
                    string.Join("; ", result.Errors.ToList().Select(x => x.Description))
                    );
                return View(form);
            }
        }
        var userOldRoles = await _userManager.GetRolesAsync(user);
		foreach (var role in userOldRoles)
		{
			if (form.UserRoles==null || !form.UserRoles.Contains(role))
			{
				await _userManager.RemoveFromRoleAsync(user, role);
			}
		}
		if (form.UserRoles != null)
		{
            foreach (var role in form.UserRoles)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }
		return RedirectToAction("Index");
	}

}