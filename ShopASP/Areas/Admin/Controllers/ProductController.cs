using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopASP.Models;
using ShopASP.Models.Services;
using System.Diagnostics;

namespace ShopASP.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
	private readonly ILogger<ProductController> _logger;
	private readonly ShopContext _context;
	private readonly MyImageStorage _imageStorage;

	public ProductController(ILogger<ProductController> logger, ShopContext context, MyImageStorage imageStorage)
	{
		_logger = logger;
		_context = context;
		_imageStorage = imageStorage;
	}

	public async Task<IActionResult> Index()
	{
		return View(await _context.Products.Include(x=>x.Photo).Include(x => x.Category).ToListAsync());
	}

	[HttpGet]
	public async Task<IActionResult> Create()
	{
		ViewData["Categories"] = await _context.Categories.ToListAsync();
		return View(new Product());
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromForm] Product form, IFormFile? image)
	{
		if (image != null)
		{
			form.Photo = await _imageStorage.UploadAsync(image);
		}
		ViewData["Categories"] = await _context.Categories.ToListAsync();
		await _context.Products.AddAsync(form);
		await _context.SaveChangesAsync();
		return RedirectToAction("Index", "Product", new { area = "Admin" });
	}

	[HttpGet]
	public async Task<IActionResult> Edit(int id)
	{
		var product = await _context.Products.FirstAsync(x => x.Id == id);
		ViewData["Categories"] = await _context.Categories.ToListAsync();
		return View(product);
	}

	[HttpPost]
	public async Task<IActionResult> Edit([FromForm] Product form, IFormFile? image, int id)
	{
		var product = await _context.Products.Include(x => x.Photo).FirstAsync(x => x.Id == id);
		if (image != null)
		{
			if (product.Photo != null)
			{
				_imageStorage.RemoveFile(product.Photo);
				_context.Remove(product.Photo);
			}
			product.Photo = await _imageStorage.UploadAsync(image);
		}
		ViewData["Categories"] = await _context.Categories.ToListAsync();
		await _context.SaveChangesAsync();
		return RedirectToAction("Index", "Product", new { area = "Admin" });
	}
	[HttpPost]
	public async Task<IActionResult> Delete(int id)
	{
		var product = await _context.Products.Include(x => x.Photo).FirstAsync(x => x.Id == id);
		if (product.Photo != null)
		{
			_imageStorage.RemoveFile(product.Photo);
			_context.Remove(product.Photo);
		}
		_context.Remove(product);
		await _context.SaveChangesAsync();
		return RedirectToAction("Index", "Product", new { area = "Admin" });
	}
}

