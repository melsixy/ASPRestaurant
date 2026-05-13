using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPRestaurant.Data;

namespace ASPRestaurant.Controllers
{
    public class MealsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MealsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Meals
        public async Task<IActionResult> Index()
        {
            var meals = _context.Meals.Include(m => m.TypeOrders);
            return View(await meals.ToListAsync());

        }

        // GET: Meals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meal = await _context.Meals
                .Include(m => m.TypeOrders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (meal == null)
            {
                return NotFound();
            }

            return View(meal);
        }

        // GET: Meals/Create
        public IActionResult Create()
        {
            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name");
            return View();
        }

        // POST: Meals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Meal meal, IFormFile coverImageFile)
        {
            if (ModelState.IsValid)
            {
                if (coverImageFile != null && coverImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImageFile.FileName);
                    var path = Path.Combine("wwwroot/images", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await coverImageFile.CopyToAsync(stream);
                    }

                    meal.CoverImage = "/images/" + fileName;
                }
                else
                {   
                    meal.CoverImage = "/images/default.jpg";
                }

                if (string.IsNullOrWhiteSpace(meal.Alergens))
                {
                    meal.Alergens = "няма";
                }
                _context.Add(meal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name", meal.TypeOrderId);
            return View(meal);
        }

        // GET: Meals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meal = await _context.Meals.FindAsync(id);
            if (meal == null)
            {
                return NotFound();
            }
            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name", meal.TypeOrderId);
            return View(meal);
        }

        // POST: Meals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Meal meal, IFormFile coverImageFile)
        {
            if (id != meal.Id)
                return NotFound();

            var existing = await _context.Meals.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existing.Name = meal.Name;
                existing.Price = meal.Price;
                existing.Grammage = meal.Grammage;
                existing.Alergens = meal.Alergens;
                existing.TypeOrderId = meal.TypeOrderId;

                if (coverImageFile != null && coverImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImageFile.FileName);
                    var path = Path.Combine("wwwroot/images", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await coverImageFile.CopyToAsync(stream);
                    }

                    existing.CoverImage = "/images/" + fileName;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name", meal.TypeOrderId);
            return View(meal);
        }

        // GET: Meals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meal = await _context.Meals
                .Include(m => m.TypeOrders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (meal == null)
            {
                return NotFound();
            }

            return View(meal);
        }

        // POST: Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal != null)
            {
                _context.Meals.Remove(meal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MealExists(int id)
        {
            return _context.Meals.Any(e => e.Id == id);
        }
    }
}
