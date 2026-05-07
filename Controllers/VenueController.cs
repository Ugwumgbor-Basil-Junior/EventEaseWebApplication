using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string connectionString = "UseDevelopmentStorage=true"; // Azurite
        private const string containerName = "venueimages";

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venue.ToListAsync();
            return View(venues);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (!ModelState.IsValid)
                return View(venue);

            if (venue.ImageFile != null)
            {
                venue.ImageUrl = await UploadImageToBlobAsync(venue.ImageFile);
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue successfully created!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(venue);

            try
            {
                var existingVenue = await _context.Venue.FindAsync(id);
                if (existingVenue == null)
                    return NotFound();

                existingVenue.VenueName = venue.VenueName;
                existingVenue.Location = venue.Location;
                existingVenue.Capacity = venue.Capacity;

                if (venue.ImageFile != null)
                {
                    existingVenue.ImageUrl = await UploadImageToBlobAsync(venue.ImageFile);
                }

                _context.Update(existingVenue);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Venue updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueID))
                    return NotFound();

                throw;
            }
        }

        // GET: Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueID == id);

            if (venue == null) return NotFound();

            return View(venue);
        }


        // GET: Confirm Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venue
                .FirstOrDefaultAsync(v => v.VenueID == id);

            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // POST: Delete (with booking restriction)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);

            if (venue == null)
                return NotFound();

            // 🔒 BLOCK deletion if bookings exist
            var hasBookings = await _context.Booking
                .AnyAsync(b => b.VenueID == id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] =
                    "Cannot delete venue because it has existing bookings.";

                return RedirectToAction(nameof(Index));
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Venue deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }

        // Upload to Azure Blob (Azurite local)
        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var headers = new BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using var stream = imageFile.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = headers
            });

            return blobClient.Uri.ToString();
        }
    }
}