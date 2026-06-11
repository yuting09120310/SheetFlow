using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Infrastructure;
using SheetFlow.Repositories;

namespace SheetFlow.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly IEmployeeProfileRepository _profileRepo;

    public ProfileController(IUserRepository userRepo, IEmployeeProfileRepository profileRepo)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var username = User.Identity?.Name ?? "";
        var user = await _userRepo.GetByUsernameAsync(username);
        var profile = await _profileRepo.GetByUsernameAsync(username);
        if (profile == null) return NotFound();
        ViewBag.Department = user?.Department;
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Update(string email, string? password)
    {
        var username = User.Identity?.Name ?? "";
        var profile = await _profileRepo.GetByUsernameAsync(username);
        if (profile == null) return NotFound();

        profile.Email = email;
        if (!string.IsNullOrWhiteSpace(password))
        {
            profile.Password = password;
            var user = await _userRepo.GetByUsernameAsync(username);
            if (user != null)
            {
                user.PasswordHash = PasswordHelper.HashPassword(password);
                await _userRepo.UpdateAsync(user);
            }
        }

        await _profileRepo.UpdateAsync(profile);
        TempData["Success"] = "個人資料已更新";
        return RedirectToAction("Index");
    }
}
