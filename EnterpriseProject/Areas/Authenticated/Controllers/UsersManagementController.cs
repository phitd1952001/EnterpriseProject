using System;
using System.Collections.Generic;
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
    [Area(SD.Authenticated)]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Manager)]
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
        public async Task<IActionResult> Index(string searchString)
        {
            // get all users except login id
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //used to avoid deleting your role by mistake
            var userList = _db.ApplicationUsers.Where(u => u.Id != claims.Value).ToList(); 

            foreach (var user in userList)
            {
                // Get all user roles in userlist -
                //user.Role = roleTemp.FirstOrDefault => to get the first role of the user ( user.roleTemp in case the user has multiple roles )
                //var userTemp = await _userManager.FindByIdAsync(user.Id);
                var roleTemp = await _userManager.GetRolesAsync(user);
                user.Role = roleTemp.FirstOrDefault();
            }
            
            if (!String.IsNullOrEmpty(searchString))
            {
                userList = userList.Where(s => s.Email.Contains(searchString)).ToList();
            }

            return View(userList);
        }
        
        [HttpGet]
        public async Task<IActionResult> LockUnLock(string id)
        {
            // take out your account to avoid being wrongly deleted
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            // down the database searching accounts according to ID.
            var userNeedToLock = _db.ApplicationUsers.Where(u => u.Id == id).First();
            if (userNeedToLock.Id == claims.Value)
            {
                //show that you're using your own earphones
            }

            // Lock_out_end = NULL: Or by Time Past, it will not lock, because the time is in the past then the present time, the account will not be locked anymore.
            // set time lock_out_end = Time future means the account will be locked.
            // When the Lock_out_end field will compare with the current Time School. If the current time is smaller, the account is staggered. And the time is larger than the present, the account is being opened.
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
                // Show old Role to avoid when pressing the submit button does not choose the Role
                var roleTemp = await _userManager.GetRolesAsync(user);
                userVm.Role = roleTemp.First();
                // Show additional Role List. If the user wants to update the role.
                userVm.Rolelist = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem()
                {
                    Text = i,
                    Value = i
                });

                userVm.DepartmentId = user.DepartmentId ?? 0;
                userVm.Departmentlist = DepartmentSelectListItems();
                
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
                user.PassportID = userVm.ApplicationUser.PassportID;
                user.FullName = userVm.ApplicationUser.FullName;
                user.PhoneNumber = userVm.ApplicationUser.PhoneNumber;
                user.Birthday = userVm.ApplicationUser.Birthday;
                user.Address = userVm.ApplicationUser.Address;
                user.DepartmentId = userVm.DepartmentId;
                
                // update role
                var oldRole = await _userManager.GetRolesAsync(user);
                // Delete old roles
                await _userManager.RemoveFromRoleAsync(user, oldRole.First());
                // Add new role
                await _userManager.AddToRoleAsync(user, userVm.Role);

                _db.ApplicationUsers.Update(user);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            
            userVm.Departmentlist = DepartmentSelectListItems();
            return View(userVm);
        }

        [NonAction]
        private IEnumerable<SelectListItem> DepartmentSelectListItems()
        {
            var Department = _db.Departments.ToList().Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return Department;
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

            // initialize confirmemailvm
            // Putting the value of emailvm confold into the model below.
            ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
            {
                Email = user.Email
            };

            return View(confirmEmailVm);
        }

        [HttpPost]
        // Receive value from Confirmemailvm
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
        {
            if (ModelState.IsValid)
            {
                // Use the library _usermanager down to DB to check the user.
                var user = await _userManager.FindByEmailAsync(confirmEmailVm.Email);
                // this token will check if it is true for the token and the system has kept this email.
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // move to tran resetpassword => controller Usersmanagement
                // New keywords to create 1 object, value toe at ConfirmemailVM = token at resetpassword page. The email is similar.
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

