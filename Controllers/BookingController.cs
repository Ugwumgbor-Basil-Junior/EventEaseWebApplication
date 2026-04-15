using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();

            return View(bookings);
        }

        public IActionResult Create()
        {
            ViewBag.Events = new SelectList(_context.Event.ToList(), "EventID", "EventName");
            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var eventDate = _context.Event.FirstOrDefault(e => e.EventID == booking.EventID)?.EventDate;

            var conflict = await _context.Booking
                .AnyAsync(b => b.VenueID == booking.VenueID &&
                               _context.Event.Any(e =>
                                   e.EventID == b.EventID &&
                                   e.EventDate == eventDate));

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Events = new SelectList(_context.Event.ToList(), "EventID", "EventName");
            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName");
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();

            ViewBag.Events = new SelectList(_context.Event.ToList(), "EventID", "EventName", booking.EventID);
            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var eventDate = _context.Event
                        .FirstOrDefault(e => e.EventID == booking.EventID)?.EventDate;

                    var conflict = await _context.Booking
                        .AnyAsync(b => b.VenueID == booking.VenueID &&
                                       b.BookingID != booking.BookingID &&
                                       _context.Event.Any(e =>
                                           e.EventID == b.EventID &&
                                           e.EventDate == eventDate));

                    if (conflict)
                    {
                        ModelState.AddModelError("", "This venue is already booked for that date.");
                        ViewBag.Events = new SelectList(_context.Event.ToList(), "EventID", "EventName", booking.EventID);
                        ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", booking.VenueID);
                        return View(booking);
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Events = new SelectList(_context.Event.ToList(), "EventID", "EventName", booking.EventID);
            ViewBag.Venues = new SelectList(_context.Venue.ToList(), "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingID == id);
        }
    }
}