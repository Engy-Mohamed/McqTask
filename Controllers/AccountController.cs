using McqTask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace McqTask.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ExamContext _context; // Added DbContext

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 ExamContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //var users = _userManager.Users.ToList(); // Get all users
            var users = await _userManager.GetUsersInRoleAsync("ExamTaker");
            return View(users);
        }

        public async Task<IActionResult> ListExamTakers()
        {
            var examTakers = await _userManager.GetUsersInRoleAsync("ExamTaker");
            return View(examTakers);
        }

        public async Task<IActionResult> ListAdmins()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return View(admins);
        }
        public IActionResult Login()
        {
            ViewData["UseCustomLayout"] = true; // Flag to indicate a custom layout
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl); // ✅ Redirect to the original page
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Invalid login attempt.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewData["UseCustomLayout"] = true;
            return View();
        }

        public IActionResult Create()
        {
            ViewData["Group"] = new SelectList(_context.Groups, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string fullName, string email, string password, string PhoneNumber,int GroupId)
        {
            var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName , PhoneNumber = PhoneNumber , GroupId = GroupId };

            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "ExamTaker"); // Default role
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewData["Group"] = new SelectList(_context.Groups, "Id", "Name", user.GroupId);
            return View(user);
        }

      
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin"); // Assign the role
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Registration failed.";
            ViewData["UseCustomLayout"] = true;
            return View();
        }

        public IActionResult AccessDenied() => View();

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewData["Group"] = new SelectList(_context.Groups, "Id", "Name", user.GroupId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email; // Ensure username is updated
                user.PhoneNumber = model.PhoneNumber;
                user.GroupId = model.GroupId;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewData["Group"] = new SelectList(_context.Groups, "Id", "Name", model.GroupId);
            return View(model);
        }

        // ============================ DELETE USER ============================
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(user);
        }
    }
}
