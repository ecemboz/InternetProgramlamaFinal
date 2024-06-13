using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.IO;
using WebApplication3.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace WebApplication3.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

		public async Task<IActionResult> Index(int? categoryId, int? subCategoryId)
		{
			var productsQuery = _context.Products
										.Include(p => p.Category)
										.Include(p => p.SubCategory)
										.Include(p => p.Brand)
										.AsQueryable();

			if (categoryId.HasValue)
			{
				productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
			}

			if (subCategoryId.HasValue)
			{
				productsQuery = productsQuery.Where(p => p.SubCategoryId == subCategoryId.Value);
			}

			if (!User.Identity.IsAuthenticated)
			{
				productsQuery = productsQuery.Where(p => p.StockStatus > 0);
			}

			var products = await productsQuery.ToListAsync();
			ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
			ViewBag.SubCategories = categoryId.HasValue ? new SelectList(_context.SubCategories.Where(sc => sc.CategoryId == categoryId.Value), "Id", "Name") : new SelectList(Enumerable.Empty<SelectListItem>());
			return View(products);
		}

		[HttpPost]
		public JsonResult GetSubCategories(int categoryId)
		{
			var subCategories = _context.SubCategories
				.Where(sc => sc.CategoryId == categoryId)
				.Select(sc => new SelectListItem
				{
					Value = sc.Id.ToString(),
					Text = sc.Name
				}).ToList();

			return Json(subCategories);
		}

		[HttpGet]
        public IActionResult Create()
        {
            SetViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,StockStatus,CategoryId,SubCategoryId,BrandId, UrunPhoto, ImageFile")] Product product)
        {
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string wwwrootpath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName);
                string extension = Path.GetExtension(product.ImageFile.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                product.UrunPhoto = fileName;
                string path = Path.Combine(wwwrootpath + "/Image/", fileName);

                using (var filestream = new FileStream(path, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(filestream);
                }
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            SetViewBags(product);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,StockStatus,CategoryId,SubCategoryId,BrandId, UrunPhoto, ImageFile")] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.StockStatus = product.StockStatus;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.SubCategoryId = product.SubCategoryId;
            existingProduct.BrandId = product.BrandId;

            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string wwwrootpath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName);
                string extension = Path.GetExtension(product.ImageFile.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                existingProduct.UrunPhoto = fileName;
                string path = Path.Combine(wwwrootpath + "/Image/", fileName);

                using (var filestream = new FileStream(path, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(filestream);
                }
            }

            _context.Update(existingProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.Category)
                                        .Include(p => p.SubCategory)
                                        .Include(p => p.Brand)
                                        .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReduceStock(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (product.StockStatus > 0)
            {
                product.StockStatus--;
                _context.Update(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.Category)
                                        .Include(p => p.SubCategory)
                                        .Include(p => p.Brand)
                                        .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        private void SetViewBags(Product product = null)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product?.CategoryId);
            ViewBag.SubCategories = new SelectList(_context.SubCategories, "Id", "Name", product?.SubCategoryId);
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", product?.BrandId);
        }

        // Kategoriye göre ürünleri filtreleme
        public IActionResult FilterByCategory(int categoryId)
        {
            var products = _context.Products
                                   .Include(p => p.Category)
                                   .Include(p => p.SubCategory)
                                   .Where(p => p.CategoryId == categoryId)
                                   .ToList();
            return View("Index", products);
        }

        // Alt kategoriye göre ürünleri filtreleme
        public IActionResult FilterBySubCategory(int subCategoryId)
        {
            var products = _context.Products
                                   .Include(p => p.Category)
                                   .Include(p => p.SubCategory)
                                   .Where(p => p.SubCategoryId == subCategoryId)
                                   .ToList();
            return View("Index", products);
        }
    }
}
