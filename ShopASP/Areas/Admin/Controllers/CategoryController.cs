using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopASP.Models;
using ShopASP.Models.Services;
using System.Diagnostics;

namespace ShopASP.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryController : Controller
{
	private readonly ILogger<CategoryController> _logger;
	private readonly ShopContext _context;
	private readonly MyImageStorage _imageStorage;

	public CategoryController(ILogger<CategoryController> logger, ShopContext context, MyImageStorage imageStorage)
	{
		_logger = logger;
		_context = context;
		_imageStorage = imageStorage;
	}

	public async Task<IActionResult> Index()
	{
		return View(await _context.Categories.Include(x=>x.Photo).ToListAsync());
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View(new Category());
	}
	
	[HttpPost]
	public async Task<IActionResult> Create([FromForm] Category form, IFormFile? image)
	{
		if (!ModelState.IsValid)
		{
			return View(form);
		}
		if(image != null) 
		{ 
	
			form.Photo = await _imageStorage.UploadAsync(image);
		}
		await _context.Categories.AddAsync(form);
		await _context.SaveChangesAsync();
		return RedirectToAction("Index", "Category", new {area = "Admin"});
	}

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        return View(
            await _context
            .Categories
            .FirstAsync(x => x.Id == id)
            );
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [FromForm] Category form, IFormFile? image)
    {
        var category = await _context
            .Categories
            .Include(x => x.Photo)
            .FirstAsync(x => x.Id == id);

        if (image != null)
        {
            if (category.Photo != null)
            {
                _imageStorage.RemoveFile(category.Photo);
                _context.Remove(category.Photo);
            }

            category.Photo = await _imageStorage.UploadAsync(image);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Category", new { area = "Admin" });
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.Include(x => x.Photo).FirstAsync(x => x.Id == id);
        if (category.Photo != null)
        {
            _imageStorage.RemoveFile(category.Photo);
            _context.Remove(category.Photo);
        }
		_context.Remove(category);
		await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Category", new { area = "Admin" });
    }
    public IActionResult Products(int categoryId)
    {
        var category = _context.Categories.Include(x => x.Photo).FirstOrDefault(x => x.Id == categoryId);
        if (category == null)
        {
            return NotFound();
        }

        var products = _context.Products.Include(x => x.Photo).Where(x => x.CategoryId == categoryId).ToList();
        ViewBag.Category = category;
        return View(products);
    }

    [HttpGet]
    public IActionResult ProductDetails(int productId)
    {
        var product = _context.Products.Include(x => x.Photo).FirstOrDefault(x => x.Id == productId);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}

