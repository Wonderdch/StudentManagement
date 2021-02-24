using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace StudentManagement.Security.CustomTokenProvider
{
    /// <summary>
    /// 自定义邮件验证令牌提供程序
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class CustomEmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<CustomEmailConfirmationTokenProviderOptions> options) : base(dataProtectionProvider, options)
        {
        }
    }
}