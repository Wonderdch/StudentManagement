using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DiagnosticAdapter.Internal;
using StudentManagement.Models;
using StudentManagement.ViewModels;

namespace StudentManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                // 如果尝试创建重名角色，会收到验证错误
                IdentityResult result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public IActionResult ListRoles()
        {
            var roles = _roleManager.Roles;

            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色 Id 为 {id} 的信息不存在，请重试。";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                // 如果用户拥有此角色，请讲用户名添加到 EditRoleViewModel 的 Users 属性中
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色 Id 为 {model.Id} 的信息不存在，请重试。";
                return View("NotFound");
            }

            role.Name = model.RoleName;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("ListRoles");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色 ID 为 {roleId} 的角色不存在";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in _userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                };

                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色 ID 为 {roleId} 的角色不存在";
                return View("NotFound");
            }

            for (var i = 0; i < model.Count; i++)
            {
                var userRoleVM = model[i];
                var user = await _userManager.FindByIdAsync(userRoleVM.UserId);

                // 判断当前用户是否已属于该角色且被选中
                // 不属于的话，要添加进来；没有选中的话要移除出来

                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);

                IdentityResult result;

                // 被选中，但尚不属于该角色
                if (userRoleVM.IsSelected && !isInRole)
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                // 本来属于该角色，但未被选中
                else if (!userRoleVM.IsSelected && isInRole)
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else continue;

                if (result.Succeeded)
                {
                    if (i < model.Count - 1) continue;

                    return RedirectToAction("EditRole", new { id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { id = roleId });
        }
    }
}