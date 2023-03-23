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
            // lấy ra tài khoản chính mình tránh bị xóa nhầm
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            // xuống database tìm kiếm tài khoản theo Id.
            var userNeedToLock = _db.ApplicationUsers.Where(u => u.Id == id).First();
            if (userNeedToLock.Id == claims.Value)
            {
                //show that you're using your own earphones
            }

            // Lock_out_end = null : hoặc bằng time quá khứ thì nó sẽ không lock, Bởi vì thời gian nằm ở quá khứ rồi thì thời gian hiện tại thì tài khoản sẽ không bị lock nữa. 
            // set time lock_out_end = time tương lai thì có nghĩa tài khoản sẽ bị khóa. 
            //	Khi mà trường lock_out_end sẽ đi so sánh với cái trường time hiện tại. nếu mà cái thời gian hiện tại nó nhỏ hơn thì tài khoản bị kháo. Còn time lớn hơn hiện tại thì tài khoản đang được mở.

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
                // hiển thị role cũ tránh khi ấn nút submit không chọn role
                var roleTemp = await _userManager.GetRolesAsync(user);
                userVm.Role = roleTemp.First();
                // Hiển thị thêm cái role list. Nếu người dùng muốn update cái role.
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
                // xóa đi role cu
                await _userManager.RemoveFromRoleAsync(user, oldRole.First());
                // add role mới
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

            // khởi tạo ConfirmEmailVM
            // đưa giá trị Confirm EmailVM vào model bên dưới.
            ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
            {
                Email = user.Email
            };

            return View(confirmEmailVm);
        }

        [HttpPost]
        // nhận giá trị về từ ConfirmEmailVM
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
        {
            if (ModelState.IsValid)
            {
                // dùng thư viện _userManager xuống db để kiem tra user.
                var user = await _userManager.FindByEmailAsync(confirmEmailVm.Email);
                // token này sẽ check xem coi có đúng với token và hệ thống đã giử cho email này hay không.
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // chuyển đến tran ResetPassword => controller UsersManagement
                // từ khóa new khởi tạo 1 object, value toke ở ConfirmEmailVM = token ở trang resetPassword. email cũng tương tự.
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

