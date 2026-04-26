using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPRestaurant.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
namespace ASPRestaurant.Controllers

{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var query = _context.Reservations
                .Include(r => r.Tables)
                .Include(r => r.Clients)
                .AsQueryable();

            // ако НЕ е админ → вижда само неговите
            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(r => r.ClientId == userId);
            }

            var reservations = await query.ToListAsync();
            return View(reservations);
        }

        // GET: Details (само свои или admin)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .Include(r => r.Tables)
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == userId));

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

        // GET: Create
        public IActionResult Create(int tableId)
        {
            var reservation = new Reservation
            {
                TableId = tableId,
                Date = DateTime.Now
            };

            return View(reservation);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            reservation.RegisterOn = DateTime.Now;
            reservation.ClientId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
                return View(reservation);

            var start = reservation.Date.Date.Add(reservation.Time);
            var end = start.AddHours(2);

            bool isTaken = await _context.Reservations.AnyAsync(r =>
                r.TableId == reservation.TableId &&
                start < r.Date.Date.Add(r.Time).AddHours(2) &&
                end > r.Date.Date.Add(r.Time)
            );

            if (isTaken)
            {
                ModelState.AddModelError("", "Масата е заета");
                return View(reservation);
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit (само свои или admin)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == userId));

            if (reservation == null)
                return NotFound();

            ViewData["TableId"] = new SelectList(_context.Tables, "Id", "Description", reservation.TableId);

            return View(reservation);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.Id)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var existing = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == userId));

            if (existing == null)
                return NotFound();

            existing.NumberOfPeople = reservation.NumberOfPeople;
            existing.Date = reservation.Date;
            existing.Time = reservation.Time;
            existing.TableId = reservation.TableId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .Include(r => r.Tables)
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == userId));

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == userId));

            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        // Table status (OK)
        public async Task<IActionResult> UpdateTableStatuses()
        {
            var now = DateTime.Now;

            var tables = await _context.Tables
                .Include(t => t.Reservations)
                .ToListAsync();

            var model = tables.Select(t => new Table
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Description = t.Description,
                Count = t.Count,
                IsAvailable = !t.Reservations.Any(r =>
                {
                    var start = r.Date.Add(r.Time);
                    var end = start.AddHours(2);

                    return now >= start && now <= end;
                })
            }).ToList();

            return View(model);
        }
    }
}
