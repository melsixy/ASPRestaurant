using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPRestaurant.Data;
using Microsoft.AspNetCore.Authorization;

namespace ASPRestaurant.Controllers
{
    public class DrinksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DrinksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Drinks
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Drinks.Include(d => d.TypeOrders);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Drinks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await _context.Drinks
                .Include(d => d.TypeOrders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (drink == null)
            {
                return NotFound();
            }

            return View(drink);
        }

        // GET: Drinks/Create
        [Authorize(Roles="Admin")]
        public IActionResult Create()
        {
            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name");
            return View();
        }

        // POST: Drinks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Drink drink, IFormFile coverImageFile)
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

                    drink.CoverImage = "/images/" + fileName;
                }
                else
                {
                    drink.CoverImage = "/images/default.jpg";
                }

                if (string.IsNullOrWhiteSpace(drink.Description))
                {
                    drink.Description = "няма";
                }
                _context.Add(drink);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name", drink.TypeOrderId);
            return View(drink);
        }

        // GET: Drinks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await _context.Drinks.FindAsync(id);
            if (drink == null)
            {
                return NotFound();
            }
            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name", drink.TypeOrderId);
            return View(drink);
        }

        // POST: Drinks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Drink drink, IFormFile coverImageFile)
        {
            if (id != drink.Id)
                return NotFound();

            var existing = await _context.Drinks.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (ModelState.IsValid)
            {

                existing.Name = drink.Name;
                existing.Description = drink.Description;
                existing.Litre = drink.Litre;
                existing.Price = drink.Price;
                existing.TypeOrderId = drink.TypeOrderId;

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

            ViewData["TypeOrderId"] = new SelectList(_context.TypeOrders, "Id", "Name", drink.TypeOrderId);
            return View(drink);
        }

        // GET: Drinks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await _context.Drinks
                .Include(d => d.TypeOrders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (drink == null)
            {
                return NotFound();
            }

            return View(drink);
        }

        // POST: Drinks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var drink = await _context.Drinks.FindAsync(id);
            if (drink != null)
            {
                _context.Drinks.Remove(drink);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DrinkExists(int id)
        {
            return _context.Drinks.Any(e => e.Id == id);
        }
    }
}
