using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterpriseProject.Data;
using EnterpriseProject.Utility;
using EnterpriseProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnterpriseProject.Areas.Authenticated.Controllers
{
    [Area("Authenticated")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersManagementController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersManagementController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _db = db;
            _roleManager = roleManager;
        }
        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // get all users except login id
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //used to avoid deleting your role by mistake
            var userList = _db.ApplicationUsers.Where(u => u.Id != claims.Value); 

            foreach (var user in userList)
            {
                // Get all user roles in userlist -
                //user.Role = roleTemp.FirstOrDefault => to get the first role of the user ( user.roleTemp in case the user has multiple roles )
                //var userTemp = await _userManager.FindByIdAsync(user.Id);
                var roleTemp = await _userManager.GetRolesAsync(user);
                user.Role = roleTemp.FirstOrDefault();
            }

            return View(userList.ToList());
        }
        
        [HttpGet]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userNeedToLock = _db.ApplicationUsers.Where(u => u.Id == id).First();
            if (userNeedToLock.Id == claims.Value)
            {
                //show that you're using your own earphones
            }

            if (userNeedToLock.LockoutEnd != null && userNeedToLock.LockoutEnd > DateTime.Now)
            {
                userNeedToLock.LockoutEnd = DateTime.Now;
            }
            else
            {
                userNeedToLock.LockoutEnd = DateTime.Now.AddYears(1);
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

       
        [HttpGet]
        public async Task<IActionResult> Update(String id)
        {
            if (id != null)
            {
                UserVM userVm = new UserVM();
                var user = _db.ApplicationUsers.Find(id);
                userVm.ApplicationUser = user;
                var roleTemp = await _userManager.GetRolesAsync(user);
                userVm.Role = roleTemp.First();
                userVm.Rolelist = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem()
                {
                    Text = i,
                    Value = i
                });
                return View(userVm);
            }
            return NotFound();
        }
        
        [HttpPost]
        public async Task<IActionResult> Update(UserVM userVm)
        {
            if (ModelState.IsValid)
            {
                var user = _db.ApplicationUsers.Find(userVm.ApplicationUser.Id);
                user.FullName = userVm.ApplicationUser.FullName;
                user.PhoneNumber = userVm.ApplicationUser.PhoneNumber;

                var oldRole = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRoleAsync(user, oldRole.First());
                await _userManager.AddToRoleAsync(user, userVm.Role);

                _db.ApplicationUsers.Update(user);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(userVm);
        }

        
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var user = _db.ApplicationUsers.Find(id);

            if (user == null)
            {
                return View();
            }

            ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
            {
                Email = user.Email
            };

            return View(confirmEmailVm);
        }

        [HttpPost]
        
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(confirmEmailVm.Email);

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                return RedirectToAction("ResetPassword", "UsersManagement", new {token = token, email = user.Email});
            }

            return View(confirmEmailVm);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("","Invalid password reset token");
            }

            ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel()
            {
                Email = email,
                Token = token
            };
            return View(resetPasswordViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token,
                        resetPasswordViewModel.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View(resetPasswordViewModel);
        }
    }
}

