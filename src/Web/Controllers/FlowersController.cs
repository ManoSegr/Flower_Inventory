using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlowerShop.Application;
using FlowerShop.Domain;
using FlowerShop.Infrastructure;
using FlowerShop.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Web.Controllers
{
    public class FlowersController : Controller
    {
        private readonly FlowerShopDbContext _context;
        private readonly IFlowerService _search;
        private readonly string _imageFolder;
        private readonly string _imageRequestBase = "/images";

        public FlowersController(FlowerShopDbContext context, IWebHostEnvironment env, IFlowerService search)
        {
            _context = context;
            _search = search;

            _imageFolder = Path.Combine(env.WebRootPath, "images");
            if (!Directory.Exists(_imageFolder))
                Directory.CreateDirectory(_imageFolder);
        }

        private async Task<SelectList> BuildCategoryOptionsAsync(int? selectedId = null)
        {
            var cats = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync();

            return new SelectList(cats, "CategoryId", "Name", selectedId);
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file is not { Length: > 0 }) return null;

            var ext = Path.GetExtension(file.FileName);
            var fname = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(_imageFolder, fname);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"{_imageRequestBase}/{fname}";
        }

        private void DeletePhysicalImageIfExists(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            var name = Path.GetFileName(imageUrl);
            var path = Path.Combine(_imageFolder, name);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        public async Task<IActionResult> Index(
            string? q, int? categoryId, decimal? minPrice, decimal? maxPrice,
            string sort = "name", bool desc = false, int page = 1, int pageSize = 10)
        {
            var (items, total) = await _search.SearchAsync(q, categoryId, minPrice, maxPrice, sort, desc, page, pageSize);

            var vm = new FlowerListVm
            {
                Items = items,
                Total = total,
                Q = q,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort,
                Desc = desc,
                Page = page,
                PageSize = pageSize,
                CategoryOptions = await BuildCategoryOptionsAsync(categoryId)
            };

            return View("~/Views/Flowers/Index.cshtml", vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var flower = await _context.Flowers
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.FlowerId == id);

            if (flower == null) return NotFound();
            return View("~/Views/Flowers/Details.cshtml", flower);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryOptions = await BuildCategoryOptionsAsync();
            return View("~/Views/Flowers/Create.cshtml", new Flower { Active = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flower model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
                return View("~/Views/Flowers/Create.cshtml", model);
            }

            var imageUrl = await SaveImageAsync(imageFile);
            if (!string.IsNullOrEmpty(imageUrl))
                model.ImageUrl = imageUrl;

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.Flowers.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = model.FlowerId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var flower = await _context.Flowers.FindAsync(id);
            if (flower == null) return NotFound();

            ViewBag.CategoryOptions = await BuildCategoryOptionsAsync(flower.CategoryId);
            return View("~/Views/Flowers/Edit.cshtml", flower);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Flower model, IFormFile? imageFile)
        {
            if (id != model.FlowerId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
                return View("~/Views/Flowers/Edit.cshtml", model);
            }

            var dbFlower = await _context.Flowers.FirstOrDefaultAsync(f => f.FlowerId == id);
            if (dbFlower == null) return NotFound();

            _context.Entry(dbFlower).Property(x => x.RowVersion).OriginalValue = model.RowVersion;

            dbFlower.Name = model.Name;
            dbFlower.SKU = model.SKU;
            dbFlower.Type = model.Type;
            dbFlower.CategoryId = model.CategoryId;
            dbFlower.Price = model.Price;
            dbFlower.QuantityInStock = model.QuantityInStock;
            dbFlower.Color = model.Color;
            dbFlower.StemLengthCm = model.StemLengthCm;
            dbFlower.Active = model.Active;
            dbFlower.UpdatedAt = DateTime.UtcNow;

            if (imageFile is { Length: > 0 })
            {
                DeletePhysicalImageIfExists(dbFlower.ImageUrl);

                var newUrl = await SaveImageAsync(imageFile);
                if (!string.IsNullOrEmpty(newUrl))
                    dbFlower.ImageUrl = newUrl;
            }

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] = "Changes saved.";
                return RedirectToAction(nameof(Edit), new { id = dbFlower.FlowerId });
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Το στοιχείο τροποποιήθηκε από άλλον. Ελέγξτε τις τρέχουσες τιμές και ξαναπροσπαθήστε.");
               
                var fresh = await _context.Flowers.AsNoTracking().FirstOrDefaultAsync(f => f.FlowerId == id);
                ViewBag.CategoryOptions = await BuildCategoryOptionsAsync(model.CategoryId);
                return View("~/Views/Flowers/Edit.cshtml", fresh ?? model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var flower = await _context.Flowers.FindAsync(id);
            if (flower == null) return NotFound();

            if (!string.IsNullOrEmpty(flower.ImageUrl))
            {
                DeletePhysicalImageIfExists(flower.ImageUrl);
                flower.ImageUrl = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Image removed.";
            }

            return RedirectToAction(nameof(Edit), new { id });
        }


        public async Task<IActionResult> Delete(int id)
        {
            var flower = await _context.Flowers
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.FlowerId == id);

            if (flower == null) return NotFound();
            return View("~/Views/Flowers/Delete.cshtml", flower);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flower = await _context.Flowers.FindAsync(id);
            if (flower != null)
            {
                DeletePhysicalImageIfExists(flower.ImageUrl);

                _context.Flowers.Remove(flower);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Flower deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
