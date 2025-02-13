using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopASP.Areas.Auth.Models;
using ShopASP.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace ShopASP.Areas.Auth.Controllers;

[Area("Auth")]
public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ShopContext _context;

    public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager, ShopContext context)
	{
		_logger = logger;
		_userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

	[HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl)
	{
        await IntRoles();
		return View(new LoginForm());
	}

    [HttpGet]
    public async Task<IActionResult> AccesDenied(string? returnUrl)
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task <IActionResult> Login([FromForm] LoginForm form, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }
        var user = await _userManager.FindByEmailAsync(form.Login);
        if (user == null)
        {
            ModelState.AddModelError(nameof(form.Login), "This user does not exists, please register");
            return View(form);
        }
        if (!await _userManager.CheckPasswordAsync(user,form.Password))
        {
            ModelState.AddModelError(nameof(form.Password), "Incorrect password");
            return View(form);
        }
        await SignInAsync(user);
        if (returnUrl != null)
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home", new { area = ""});
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Register()
    {
		await IntRoles();
		return View(new RegisterForm());
    }

    protected async Task IntRoles()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin" });
        }
		if (!await _roleManager.RoleExistsAsync("User"))
		{
			await _roleManager.CreateAsync(new IdentityRole<int> { Name = "User" });
		}
	}

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromForm] RegisterForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }
        if (form.Password != form.ConfirmPassword ) 
        {
            ModelState.AddModelError(nameof(form.ConfirmPassword), "The passwords do not match");
            return View(form);
        }
        var user = await _userManager.FindByEmailAsync(form.Login);
        if (user != null) 
        {
            ModelState.AddModelError(nameof(form.Login), "User already exists");
            return View(form);
        }
        user = new User
        {
            Email = form.Login,
            UserName = form.Login,
            EmailConfirmed = true,
        };
        var result = await _userManager.CreateAsync(user, form.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(form.Password), string.Join("; ", result.Errors.ToList().Select(x=>x.Description)));
            return View(form);
        }
        user = await _userManager.FindByEmailAsync(form.Login);
        var cart = new Cart { UserId = user.Id};
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        await _userManager.AddToRoleAsync(user, "User");
        if (_userManager.Users.Count() == 1)
        {
			await _userManager.AddToRoleAsync(user, "Admin");
		}
        await SignInAsync(user);
		return RedirectToAction("Index", "Home", new {area=""});
	}
	protected async Task SignInAsync(User user)
	{
		var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
		identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
		identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
		identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
		(await _userManager.GetRolesAsync(user)).ToList().ForEach(role =>
		{
			identity.AddClaim(new Claim(ClaimTypes.Role, role));
		});
		await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));
	}

	[HttpGet]
	[Authorize]
	public async Task<IActionResult> LogOut()
	{
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    public IActionResult Cart()
    {
        return View();
    }
    public IActionResult Checkout()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return View("Error");
        }
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return View("Error");
        }
        var userCart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (userCart == null)
        {
            return View("Error");
        }
        var existingCartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
        if (existingCartItem != null)
        {
            existingCartItem.ProductQuantity++;
        }
        else
        {
            var newCartItem = new CartItem
            {
                Id = productId,
                ProductTitle = product.Title,
                ProductPrice = product.Price,
                ProductQuantity = 1,
                ProductPhoto = product.Photo,
                Product = product
            };

            userCart.CartItems.Add(newCartItem);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction("Cart", "Home", new { area = "" });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateCartItemQuantity(int productId, int newQuantity)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Error");
        }
        var userCart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (userCart == null)
        {
            return RedirectToAction("Error");
        }
        var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
        if (cartItem != null)
        {
            cartItem.ProductQuantity = newQuantity;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Cart");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Error");
        }
        var userCart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (userCart == null)
        {
            return RedirectToAction("Error");
        }
        var cartItemToRemove = userCart.CartItems.FirstOrDefault(ci => ci.Id == productId);
        if (cartItemToRemove != null)
        {
            userCart.CartItems.Remove(cartItemToRemove);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Cart");
    }

    [HttpPost]
    [Authorize]
    public async Task<JsonResult> ClearCart()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }
        var userCart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (userCart != null)
        {
            userCart.CartItems.Clear();
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cart cleared successfully" });
        }
        return Json(new { success = false, message = "Cart not found" });
    }

}