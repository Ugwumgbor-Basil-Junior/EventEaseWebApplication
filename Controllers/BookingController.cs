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

        // =========================
        // INDEX (NOW USING VIEW - SECTION C COMPLIANT)
        // =========================
        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = _context.Set<BookingViewModel>()
                .FromSqlRaw("SELECT * FROM vw_BookingDetails")
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.EventName.Contains(searchString) ||
                    b.BookingID.ToString().Contains(searchString));
            }

            return View(await bookings.ToListAsync());
        }

        // =========================
        // CREATE
        // =========================
        public IActionResult Create()
        {
            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName");
            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var selectedEvent = await _context.Event
                .FirstOrDefaultAsync(e => e.EventID == booking.EventID);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");

                ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName");
                ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName");

                return View(booking);
            }

            // 🔥 Double booking check
            var conflict = await _context.Booking
                .Include(b => b.Event)
                .AnyAsync(b => b.VenueID == booking.VenueID &&
                               b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");

                ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
                ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

                return View(booking);
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking created successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            return View(booking);
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);

            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}