using Microsoft.AspNetCore.Authorization;

namespace StudentManagement.Security
{
    /// <summary>
    /// 管理 Admin 角色与声明的需求
    /// </summary>
    public class ManageAdminRolesAndClaimsRequirement : IAuthorizationRequirement
    {
    }
}