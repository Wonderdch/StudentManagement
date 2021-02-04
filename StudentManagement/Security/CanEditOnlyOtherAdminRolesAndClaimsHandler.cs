using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace StudentManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement)
        {
            // 获取 httpContext 上下文
            var httpContext = _httpContextAccessor.HttpContext;

            var loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = _httpContextAccessor.HttpContext.Request.Query["userId"];

            // 判断用户是 Admin，并且拥有 claim.Type == "Edit Role" 且值为 true
            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true"))
            {
                // 如果当前拥有 Admin 角色的 userId 为空，说明进入的是角色列表页面。无法判断当前登录用户的 Id
                if (string.IsNullOrEmpty(adminIdBeingEdited))
                {
                    context.Succeed(requirement);
                }
                else if (adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
                {
                    // 成功满足需求
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}