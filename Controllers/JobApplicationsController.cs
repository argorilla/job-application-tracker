using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
using JobApplicationTracker.Pagination;
using JobApplicationTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTracker.Controllers;

[Authorize]
public class JobApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly JobApplicationCursorCodec _cursorCodec;

    public JobApplicationsController(ApplicationDbContext context, JobApplicationCursorCodec cursorCodec)
    {
        _context = context;
        _cursorCodec = cursorCodec;
    }

    public async Task<IActionResult> Index(string? q, ApplicationStatus? status, string? after, string? before)
    {
        const int pageSize = 10;

        var hasAfterCursor = !string.IsNullOrWhiteSpace(after);
        var hasBeforeCursor = !string.IsNullOrWhiteSpace(before);

        if (hasAfterCursor && hasBeforeCursor)
        {
            return BadRequest("Cannot specify both 'after' and 'before' cursors.");
        }

        if (status.HasValue && !Enum.IsDefined(typeof(ApplicationStatus), status.Value))
        {
            return BadRequest("Invalid status value.");
        }

        var searchTerm = q?.Trim();

        if (searchTerm is not null && searchTerm.Length > 100)
        {
            return BadRequest("Search term is too long.");
        }

        var applicationsQuery = _context.JobApplications
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm}%";
            applicationsQuery = applicationsQuery.Where(application =>
                EF.Functions.Like(application.CompanyName, pattern) ||
                EF.Functions.Like(application.PositionTitle, pattern));
        }

        if (status.HasValue)
        {
            applicationsQuery = applicationsQuery.Where(application => application.Status == status.Value);
        }

        var totalCount = await applicationsQuery.CountAsync();

        List<JobApplication> applications;
        string? previousCursor = null;
        string? nextCursor = null;

        if (hasBeforeCursor)
        {
            if (!_cursorCodec.TryDecode(before!, out var beforeCursor) || beforeCursor is null)
            {
                return BadRequest("Invalid previous cursor.");
            }

            var fetchedApplications = await applicationsQuery
                .Where(application => application.AppliedDate > beforeCursor.AppliedDate ||
                    (application.AppliedDate == beforeCursor.AppliedDate && application.Id > beforeCursor.Id))
                .OrderBy(application => application.AppliedDate)
                .ThenBy(application => application.Id)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasPreviousPage = fetchedApplications.Count > pageSize;

            if (hasPreviousPage)
            {
                fetchedApplications.RemoveAt(pageSize);
            }

            fetchedApplications.Reverse();
            applications = fetchedApplications;

            if (applications.Count > 0)
            {
                if (hasPreviousPage)
                {
                    var firstApplication = applications[0];
                    previousCursor = _cursorCodec.Encode(new JobApplicationCursor(
                        firstApplication.AppliedDate,
                        firstApplication.Id
                    ));
                }

                var lastApplication = applications[^1];
                nextCursor = _cursorCodec.Encode(new JobApplicationCursor(
                    lastApplication.AppliedDate,
                    lastApplication.Id
                ));
            }
        }
        else
        {
            JobApplicationCursor? afterCursor = null;

            if (hasAfterCursor)
            {
                if (!_cursorCodec.TryDecode(after!, out afterCursor) || afterCursor is null)
                {
                    return BadRequest("Invalid next cursor.");
                }
            }

            if (afterCursor is not null)
            {
                applicationsQuery = applicationsQuery
                    .Where(application => application.AppliedDate < afterCursor.AppliedDate ||
                        (application.AppliedDate == afterCursor.AppliedDate && application.Id < afterCursor.Id));
            }

            var fetchedApplications = await applicationsQuery
                .OrderByDescending(application => application.AppliedDate)
                .ThenByDescending(application => application.Id)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasNextPage = fetchedApplications.Count > pageSize;

            if (hasNextPage)
            {
                fetchedApplications.RemoveAt(pageSize);
            }

            applications = fetchedApplications;

            if (applications.Count > 0)
            {
                if (hasAfterCursor)
                {
                    var first = applications[0];

                    previousCursor = _cursorCodec.Encode(
                        new JobApplicationCursor(
                            first.AppliedDate,
                            first.Id
                        )
                    );
                }

                if (hasNextPage)
                {
                    var last = applications[^1];

                    nextCursor = _cursorCodec.Encode(
                        new JobApplicationCursor(
                            last.AppliedDate,
                            last.Id
                        )
                    );
                }
            }
        }

        if ((hasAfterCursor || hasBeforeCursor) && applications.Count == 0 && totalCount > 0)
        {
            return RedirectToAction(
                nameof(Index),
                new
                {
                    q = searchTerm,
                    status
                });
        }

        var viewModel = new JobApplicationIndexViewModel
        {
            Applications = applications,
            SearchTerm = searchTerm,
            Status = status,
            TotalCount = totalCount,
            PageSize = pageSize,
            PreviousCursor = previousCursor,
            NextCursor = nextCursor
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var application = new JobApplication
        {
            AppliedDate = DateTime.Today,
            Status = ApplicationStatus.Applied
        };

        return View(application);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(
            "CompanyName," +
            "PositionTitle," +
            "Source," +
            "JobUrl," +
            "Location," +
            "AppliedDate," +
            "Status," +
            "Notes")]
        JobApplication application)
    {
        if (!ModelState.IsValid)
        {
            return View(application);
        }

        application.CompanyName = application.CompanyName.Trim();
        application.PositionTitle = application.PositionTitle.Trim();
        application.Source = application.Source.Trim();
        application.JobUrl = string.IsNullOrWhiteSpace(application.JobUrl) ? null : application.JobUrl.Trim();
        application.Location = string.IsNullOrWhiteSpace(application.Location) ? null : application.Location.Trim();
        application.Notes = string.IsNullOrWhiteSpace(application.Notes) ? null : application.Notes.Trim();

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"{application.PositionTitle} at {application.CompanyName} was added.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var jobApplication = await _context.JobApplications.FindAsync(id);

        if (jobApplication == null)
        {
            return NotFound();
        }

        return View(jobApplication);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,CompanyName,PositionTitle,Source,JobUrl,Location,AppliedDate,Status,Notes")]
        JobApplication jobApplication)
    {
        if (id != jobApplication.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(jobApplication);
        }

        jobApplication.CompanyName = jobApplication.CompanyName.Trim();
        jobApplication.PositionTitle = jobApplication.PositionTitle.Trim();
        jobApplication.Source = jobApplication.Source.Trim();
        jobApplication.JobUrl = string.IsNullOrWhiteSpace(jobApplication.JobUrl) ? null : jobApplication.JobUrl.Trim();
        jobApplication.Location = string.IsNullOrWhiteSpace(jobApplication.Location) ? null : jobApplication.Location.Trim();
        jobApplication.Notes = string.IsNullOrWhiteSpace(jobApplication.Notes) ? null : jobApplication.Notes.Trim();

        try
        {
            _context.Update(jobApplication);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _context.JobApplications
                .AnyAsync(application => application.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var application = await _context.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(application => application.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        return View(application);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var application = await _context.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(application => application.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        return View(application);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var application = await _context.JobApplications.FindAsync(id);

        if (application is null)
        {
            return NotFound();
        }

        _context.JobApplications.Remove(application);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"{application.PositionTitle} at {application.CompanyName} was deleted.";

        return RedirectToAction(nameof(Index));
    }
}
