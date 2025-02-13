using ShopASP.Models;
using static System.Net.Mime.MediaTypeNames;

namespace ShopASP.Models.Services;

public class MyImageStorage
{
	private IWebHostEnvironment _webHostEnvironment;

	public MyImageStorage(IWebHostEnvironment webHostEnvironment)
	{
		_webHostEnvironment = webHostEnvironment;
	}

	public async Task<Photo> UploadAsync(IFormFile file)
	{
		// Get full uploads directory path
		var uploadRootPath = Path.Combine(_webHostEnvironment.WebRootPath, "photos");

		var guid = Guid.NewGuid();
		// Generate unique file name
		var filename = guid.ToString().ToLower() + Path.GetExtension(file.FileName);

		var localFileName = Path.Combine(uploadRootPath, filename);

		using (var localFile = System.IO.File.OpenWrite(localFileName))
		{
			await file.CopyToAsync(localFile);
		}
		return new Photo() { FileName = filename };
	}

	public void RemoveFile(Photo image)
	{
		// Get full uploads directory path
		var uploadRootPath = Path.Combine(_webHostEnvironment.WebRootPath, "photos");
		var localFileName = Path.Combine(uploadRootPath, image.FileName);
		File.Delete(localFileName);
	}
}


