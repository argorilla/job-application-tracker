using System.Diagnostics;
using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
using JobApplicationTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var statusCounts = await _context.JobApplications
            .AsNoTracking()
            .GroupBy(application => application.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.Status,
                item => item.Count);

        var recentApplications = await _context.JobApplications
            .AsNoTracking()
            .OrderByDescending(application => application.AppliedDate)
            .ThenByDescending(application => application.Id)
            .Take(5)
            .ToListAsync();

        var viewModel = new HomeDashboardViewModel
        {
            TotalApplications = statusCounts.Values.Sum(),
            StatusCounts = statusCounts,
            RecentApplications = recentApplications
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
        });
    }
}
