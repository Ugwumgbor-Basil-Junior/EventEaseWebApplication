using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Event
                .Include(e => e.Venue)
                .ToListAsync();

            return View(events);
        }

        public IActionResult Create()
        {
            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", @event.VenueID);
                return View(@event);
            }

            _context.Add(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", @event.VenueID);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventID) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", @event.VenueID);
                return View(@event);
            }

            _context.Update(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);

            if (@event == null)
                return NotFound();

            // BLOCK if bookings exist
            var hasBookings = await _context.Booking
                .AnyAsync(b => b.EventID == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] =
                    "Cannot delete event because it has existing bookings.";

                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Event deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}