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
        // показва всички резервации (admin) или само на потребителя
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
        // показва форма за нова резервация
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
        // записва нова резервация
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            if (!ModelState.IsValid)
                return View(reservation);

            reservation.RegisterOn = DateTime.Now;
            reservation.ClientId = _userManager.GetUserId(User);

            var table = await _context.Tables
                .Include(t => t.Reservations)
                .FirstOrDefaultAsync(t => t.Id == reservation.TableId);

            if (table == null)
            {
                ModelState.AddModelError("", "Няма такава маса");
                return View(reservation);
            }

            if (reservation.NumberOfPeople > table.Count)
            {
                ModelState.AddModelError("", "Масата няма достатъчно места");
                return View(reservation);
            }

            var start = reservation.Date.Date.Add(reservation.Time);
            var end = start.AddHours(2);

            bool isTaken = table.Reservations.Any(r =>
            {
                var rStart = r.Date.Date.Add(r.Time);
                var rEnd = rStart.AddHours(2);

                return start < rEnd && end > rStart;
            });

            if (isTaken)
            {
                ModelState.AddModelError("", "Масата е заета за този час");
                return View(reservation);
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        // показва форма за редакция
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

            ViewData["TableId"] = new SelectList(_context.Tables, "Id", "TableNumber", reservation.TableId);

            return View(reservation);
        }

        // POST: Edit
        // обновява резервация
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

            var tableExists = await _context.Tables
                .AnyAsync(t => t.Id == reservation.TableId);

            if (!tableExists)
            {
                ModelState.AddModelError("", "Избраната маса не съществува");
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
        // показва страница за изтриване
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
        // изтрива резервация
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    (User.IsInRole("Admin") || r.ClientId == _userManager.GetUserId(User)));

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

            var reservation = new Reservation
            {
                TableId = tableId,
                Date = DateTime.Today,
                Time = DateTime.Now.TimeOfDay,
                NumberOfPeople = 2,
                RegisterOn = DateTime.Now,
                ClientId = _userManager.GetUserId(User)
            };

            var reservations = await _context.Reservations
    .Where(r => r.TableId == tableId)
    .ToListAsync();

            var isBusy = reservations.Any(r =>
            {
                var start = r.Date.Date.Add(r.Time);
                var end = start.AddHours(2).AddMinutes(10);

                return now >= start && now <= end;
            });

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Tables");
        }
    }
}
