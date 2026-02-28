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
    public class TypeOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TypeOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TypeOrders
        public async Task<IActionResult> Index()
        {
            return View(await _context.TypeOrders.ToListAsync());
        }

        // GET: TypeOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeOrder = await _context.TypeOrders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeOrder == null)
            {
                return NotFound();
            }

            return View(typeOrder);
        }

        // GET: TypeOrders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TypeOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,RegisterOn")] TypeOrder typeOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(typeOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(typeOrder);
        }

        // GET: TypeOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeOrder = await _context.TypeOrders.FindAsync(id);
            if (typeOrder == null)
            {
                return NotFound();
            }
            return View(typeOrder);
        }

        // POST: TypeOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RegisterOn")] TypeOrder typeOrder)
        {
            if (id != typeOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typeOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TypeOrderExists(typeOrder.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(typeOrder);
        }

        // GET: TypeOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeOrder = await _context.TypeOrders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeOrder == null)
            {
                return NotFound();
            }

            return View(typeOrder);
        }

        // POST: TypeOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var typeOrder = await _context.TypeOrders.FindAsync(id);
            if (typeOrder != null)
            {
                _context.TypeOrders.Remove(typeOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypeOrderExists(int id)
        {
            return _context.TypeOrders.Any(e => e.Id == id);
        }
    }
}
