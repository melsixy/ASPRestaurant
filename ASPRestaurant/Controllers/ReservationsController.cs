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

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(r => r.ClientId == userId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Create
        public IActionResult Create(int tableId)
        {
            ViewData["TableId"] = new SelectList(_context.Tables, "Id", "TableNumber");

            return View(new Reservation
            {
                TableId = tableId,
                Date = DateTime.Now
            });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (!ModelState.IsValid)
                return View(reservation);

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (reservation.TableId == 0)
            {
                ModelState.AddModelError("", "Моля избери маса!");
                return View(reservation);
            }

            if (reservation.Time < new TimeSpan(10, 0, 0) ||
                reservation.Time > new TimeSpan(23, 59, 0))
            {
                ModelState.AddModelError("Time",
                    "Ресторантът работи от 10:00 до 00:00");

                return View(reservation);
            }

            reservation.RegisterOn = DateTime.Now;
            reservation.ClientId = userId;

            var table = await _context.Tables.FindAsync(reservation.TableId);

            if (table == null)
            {
                ModelState.AddModelError("", "Няма такава маса!");
                return View(reservation);
            }

            if (reservation.NumberOfPeople > table.Count)
            {
                ModelState.AddModelError("", "Масата няма достатъчно места.");
                return View(reservation);
            }

            var start = reservation.Date.Add(reservation.Time);
            var end = start.AddHours(2);

            var reservations = await _context.Reservations.ToListAsync();

            bool isTaken = reservations.Any(r =>
            {
                var rStart = r.Date.Add(r.Time);
                var rEnd = rStart.AddHours(2);

                return r.TableId == reservation.TableId &&
                       start < rEnd &&
                       end > rStart;
            });

            if (isTaken)
            {
                ModelState.AddModelError("", "Масата е заета!");
                return View(reservation);
            }

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == userId));

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(reservation);

            var userId = _userManager.GetUserId(User);

            var existing = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existing == null)
                return NotFound();

            if (!User.IsInRole("Admin") && existing.ClientId != userId)
                return Unauthorized();

            var table = await _context.Tables.FindAsync(reservation.TableId);

            if (table == null)
            {
                ModelState.AddModelError("", "Избраната маса не съществува!");
                return View(reservation);
            }

            var start = reservation.Date.Add(reservation.Time);
            var end = start.AddHours(2);

            var reservations = await _context.Reservations.ToListAsync();

            bool isTaken = reservations.Any(r =>
                r.Id != id &&
                r.TableId == reservation.TableId &&
                start < r.Date.Add(r.Time).AddHours(2) &&
                end > r.Date.Add(r.Time)
            );

            if (isTaken)
            {
                ModelState.AddModelError("", "Масата е заета за този час!");
                return View(reservation);
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickReserve(int tableId)
        {
            var now = DateTime.Now;

            var reservations = await _context.Reservations
                .Where(r => r.TableId == tableId)
                .ToListAsync();

            bool isBusy = reservations.Any(r =>
            {
                var start = r.Date.Date.Add(r.Time);
                var end = start.AddHours(2).AddMinutes(10);

                return now >= start && now <= end;
            });

            if (isBusy)
            {
                TempData["ErrorTableId"] = tableId;
                TempData["ErrorMessage"] = "❌ Масата е заета в момента!";
                return RedirectToAction("Index", "Tables");
            }

            var reservation = new Reservation
            {
                TableId = tableId,
                Date = DateTime.Today,
                Time = DateTime.Now.TimeOfDay,
                NumberOfPeople = 2,
                RegisterOn = DateTime.Now,
                ClientId = _userManager.GetUserId(User)
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessTableId"] = tableId;
            TempData["SuccessMessage"] = "✅ Успешна резервация!";

            return RedirectToAction("Index", "Tables");
        }
    }
}
