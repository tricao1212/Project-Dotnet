﻿using BookStore.Data;
using BookStore.Models;
using BookStore.Models.Binding_Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly BookStoreContext _context;
        public AccountController(BookStoreContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInput model, [FromServices] SignInManager<BookUser> signInManager, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    return RedirectToPage("Identity/Account/Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }
        public IActionResult Register(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterInput model, [FromServices] UserManager<BookUser> userManager, [FromServices] IUserStore<BookUser> userStore,
            [FromServices] SignInManager<BookUser> signInManager, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = model.CreateUser();
                const string roleName = "user";
                await userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
                user.Email = model.Email;
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
					var ranks = await _context.Ranks.SingleOrDefaultAsync(x => x.Name == "Bronze");
					var profile = new Profile
					{
						UserId = user.Id,
						RankId = ranks.Id
					};
                    await userManager.AddToRoleAsync(user, roleName);
                    _context.Profile.Add(profile);
                    await _context.SaveChangesAsync();

					if (userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = model.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        public async Task<IActionResult> EditProfile()
        {
            var username = User.Identity.Name;
            var currentUser = await _context.Users.Include(x => x.Profile).FirstOrDefaultAsync(x => x.UserName == username);
            ViewBag.Ranks = _context.Ranks.ToList();
            var model = new ProfileBinding
            {
                UserId = currentUser.Profile.UserId,
                Avatar = currentUser.Profile.Avatar,
                User = currentUser,
                Rank = currentUser.Profile.Rank,
                RankId = currentUser.Profile.RankId,
                FirstName = currentUser.Profile.FirstName,
                LastName = currentUser.Profile.LastName,
                PhoneNumber = currentUser.Profile.PhoneNumber,
                Address = currentUser.Profile.Address,
            };
            return View(model); 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Profile profile, IFormFile avatar, [FromServices] IWebHostEnvironment host)
        { 
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var currentUser = await _context.Users.Include(x => x.Profile).FirstOrDefaultAsync(x => x.UserName == username);
                if (currentUser.Profile == null)
                {
                    profile.UserId = currentUser.Id;
                    if (avatar != null)
                    {
                        var newName = Guid.NewGuid().ToString() + ".jpg";
                        var filePath = host.WebRootPath + "/images/avatar/";
                        using (var stream = System.IO.File.Create(filePath + newName))
                        {
                            await avatar.CopyToAsync(stream);
                        }
                        profile.Avatar = "images/avatar/" + newName;
                    }

                    _context.Profile.Add(profile);
                    currentUser.Profile = profile;
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    { }
                    return RedirectToAction(nameof(EditProfile));
                }
                else
                {
                    var profileToUpdate = await _context.Profile.FirstOrDefaultAsync(s => s.UserId == currentUser.Id);
                    if (avatar != null)
                    {
                        var newName = Guid.NewGuid().ToString() + ".jpg";
                        var filePath = host.WebRootPath + "/images/avatar/";
                        using (var stream = System.IO.File.Create(filePath + newName))
                        {
                            await avatar.CopyToAsync(stream);
                        }
                        profileToUpdate.Avatar = "images/avatar/" + newName;
                        if (await TryUpdateModelAsync<Profile>(profileToUpdate, "", m => m.FirstName, m => m.LastName,
                         m => m.Address, m => m.PhoneNumber, m => m.Avatar))
                        {
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            { }
                        }
                    }
                    else
                    {
                        if (await TryUpdateModelAsync<Profile>(profileToUpdate, "", m => m.FirstName, m => m.LastName,
                        m => m.Address, m => m.PhoneNumber))
                        {
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            { }
                        }
                    }

                    return RedirectToAction(nameof(EditProfile));
                }

            }
            return View(profile);
        }
        public IActionResult ManageRole([FromServices] BookStoreContext context)
        {
            var users = context.Users.Include(x => x.Profile).ToList();

            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(IdentityUserRole<int> model, [FromServices] BookStoreContext context, [FromServices] UserManager<BookUser> userManager)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);
            var roles = await userManager.GetRolesAsync(new BookUser { Id = model.UserId });
            var role = await context.Roles.SingleOrDefaultAsync(x => x.Id == model.RoleId);

            await userManager.RemoveFromRolesAsync(user, roles.ToArray());
            await userManager.AddToRoleAsync(user, role.Name);
            ViewBag.User = user;
            ViewBag.Role = role;
            return View("UpdateRoleResult");
        }
        public IActionResult GetRole(int id, [FromServices] BookStoreContext context, [FromServices] RoleManager<IdentityRole<int>> roleManager)
        {
            var users = context.Users.SingleOrDefault(x => x.Id == id);
            var roles = roleManager.Roles.ToList();
            var currentRole = context.UserRoles.FirstOrDefault(x => x.UserId == id);
            ViewBag.UserName = users.UserName;
            ViewBag.UserId = id;
            ViewBag.Roles = new SelectList(roles, "Id", "Name", currentRole?.RoleId);
            return PartialView("_RoleForm", currentRole);
        }

        [HttpPost]
        public async Task<IActionResult> Logout([FromServices] SignInManager<BookUser> signInManager, string returnUrl)
        {
            await signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return LocalRedirect("/");
            }
        }
    }
}
