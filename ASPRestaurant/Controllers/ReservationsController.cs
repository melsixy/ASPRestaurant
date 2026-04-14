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
            var reservations = await _context.Reservations
                .Include(r => r.Tables)
                .Include(r => r.Clients)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Clients)
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["TableId"] = new SelectList(
                _context.Tables
                    .Select(t => new
                    {
                        t.Id,
                        Name = "Маса №" + t.TableNumber + " - " + t.Description
                    }),
                "Id",
                "Name"
            );

            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            reservation.RegisterOn = DateTime.Now;
            reservation.ClientId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                ViewData["TableId"] = new SelectList(
                    _context.Tables.Select(t => new
                    {
                        t.Id,
                        Name = "Маса №" + t.TableNumber + " - " + t.Description
                    }),
                    "Id",
                    "Name"
                );

                return View(reservation);
            }

            var start = reservation.Date.Date.Add(reservation.Time);
            var end = start.AddHours(2);

            var reservations = await _context.Reservations.ToListAsync();

            bool isTaken = reservations.Any(r =>
            {
                var rStart = r.Date.Date.Add(r.Time);
                var rEnd = rStart.AddHours(2);

                return r.TableId == reservation.TableId &&
                       start < rEnd &&
                       end > rStart;
            });

            if (isTaken)
            {
                ModelState.AddModelError("", "Масата е заета!");

                ViewData["TableId"] = new SelectList(
                    _context.Tables.Select(t => new
                    {
                        t.Id,
                        Name = "Маса №" + t.TableNumber + " - " + t.Description
                    }),
                    "Id",
                    "Name"
                );

                return View(reservation);
            }

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            //ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Id", reservation.ClientId);
            ViewData["TableId"] = new SelectList(_context.Tables, "Id", "Description", reservation.TableId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NumberOfPeople,Date,Time,TableId")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["ClientId"] = new SelectList(_context.Users, "Id", "Id", reservation.ClientId);
            ViewData["TableId"] = new SelectList(_context.Tables, "Id", "Id", reservation.TableId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Clients)
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
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
