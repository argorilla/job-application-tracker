using JobApplicationTracker.Data;
using JobApplicationTracker.Models;
using JobApplicationTracker.Security;
using JobApplicationTracker.Services;
using JobApplicationTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationTracker.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AccountController(
        ApplicationDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        var viewModel = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var normalizedUsername = viewModel.Username
            .Trim()
            .ToUpperInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(
            user => user.NormalizedUsername == normalizedUsername);

        if (user is null)
        {
            AddInvalidCredentialsError();
            return View(viewModel);
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                viewModel.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            AddInvalidCredentialsError();
            return View(viewModel);
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                viewModel.Password);

            await _context.SaveChangesAsync();
        }

        var tokenResult = _jwtTokenService.CreateToken(user);

        Response.Cookies.Append(
            AuthenticationConstants.AccessTokenCookieName,
            tokenResult.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = new DateTimeOffset(
                    tokenResult.ExpiresAtUtc),
                IsEssential = true,
                Path = "/"
            });

        if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl) &&
            Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return LocalRedirect(viewModel.ReturnUrl);
        }

        return RedirectToAction(
            "Index",
            "Home");
    }

    private void AddInvalidCredentialsError()
    {
        ModelState.AddModelError(
            string.Empty,
            "Invalid username or password.");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            AuthenticationConstants.AccessTokenCookieName,
            new CookieOptions
            {
                Path = "/"
            });

        return RedirectToAction(nameof(Login));
    }
}
