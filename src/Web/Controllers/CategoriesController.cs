using System.Threading.Tasks;
using FlowerShop.Application;
using FlowerShop.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _svc;
        public CategoriesController(ICategoryService svc) => _svc = svc;

        public async Task<IActionResult> Index()
        {
            var items = await _svc.ListAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();
            var item = await _svc.GetWithFlowersAsync(id.Value);
            return View(item);
        }

        public IActionResult Create() => View(new Category());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var id = await _svc.CreateAsync(model);
                TempData["Success"] = "Category created.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
               
                ModelState.AddModelError(nameof(Category.Name), "Name must be unique.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();
            var item = await _svc.GetAsync(id.Value);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.CategoryId) return NotFound();
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _svc.UpdateAsync(model, model.RowVersion);
                TempData["Success"] = "Saved.";
                return RedirectToAction(nameof(Details), new { id = model.CategoryId });
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "This category was changed by someone else. Review and try again.");
                var fresh = await _svc.GetAsync(id);
                return View(fresh);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                ModelState.AddModelError(nameof(Category.Name), "Name must be unique.");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();
            var item = await _svc.GetWithFlowersAsync(id.Value);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _svc.GetWithFlowersAsync(id);
            var count = item.Flowers?.Count ?? 0;
            if (count > 0)
            {
                TempData["Error"] = $"Cannot delete. This category has {count} flower(s).";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _svc.DeleteAsync(id);
            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
