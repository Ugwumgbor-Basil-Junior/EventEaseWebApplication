using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            var events = await _context.Event.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        public IActionResult Create()
        {
            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName");
            return View(@event);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", @event.VenueID);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventID))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", @event.VenueID);
            return View(@event);
        }

        // POE PART 2 - STEP 3: Confirm Deletion (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POE PART 2 STEP 3: Perform Deletion (POST) Logic to restrict the deletion of events associated with active bookings.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);
            if (@event == null) return NotFound();

            var hasBookings = await _context.Booking.AnyAsync(b => b.EventID == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete event with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventID == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}