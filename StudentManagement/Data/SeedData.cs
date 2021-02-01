using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Models;

namespace StudentManagement.Data
{
    public static class SeedData
    {
        /// <summary>
        /// 用户种子数据
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDataInitializer(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

                Console.WriteLine("开始执行迁移数据库……");

                dbContext.Database.Migrate();
                Console.WriteLine("数据库迁移完成……");

                #region 初始化学生数据

                if (!dbContext.Students.Any())
                {
                    Console.WriteLine("开始创建种子数据中……");

                    dbContext.Students.Add(new Student
                    {
                        Id = 1,
                        Name = "梁桐铭",
                        ClassName = ClassNameEnum.FirstGrade,
                        Email = "ltm@ddxc.org"
                    });

                    dbContext.Students.Add(new Student
                    {
                        Id = 2,
                        Name = "角落的白板报",
                        ClassName = ClassNameEnum.GradeThree,
                        Email = "werltm@qq.com"
                    });
                }

                #endregion

                #region 初始化用户

                var adminUser = dbContext.Users.FirstOrDefault(u => u.UserName == "ltm@ddxc.org");

                if (adminUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "ltm@ddxc.org",
                        Email = "ltm@ddxc.org",
                        EmailConfirmed = true,
                        City = "成都"
                    };

                    var identityResult = userManager.CreateAsync(user, "bb123456").GetAwaiter().GetResult();

                    var role = dbContext.Roles.Add(new IdentityRole
                    {
                        Name = "Admin",
                        NormalizedName = "ADMIN"
                    });

                    dbContext.SaveChanges();

                    dbContext.UserRoles.Add(new IdentityUserRole<string>
                    {
                        RoleId = role.Entity.Id,
                        UserId = user.Id
                    });

                    var userClaims = new List<IdentityUserClaim<string>>
                    {
                        new IdentityUserClaim<string>
                        {
                            UserId = user.Id, ClaimType = "Create Role", ClaimValue = "Create Role"
                        },
                        new IdentityUserClaim<string>
                        {
                            UserId = user.Id, ClaimType = "Edit Role", ClaimValue = "Edit Role"
                        },
                        new IdentityUserClaim<string>
                        {
                            UserId = user.Id, ClaimType = "Delete Role", ClaimValue = "Delete Role"
                        },
                        new IdentityUserClaim<string>
                        {
                            UserId = user.Id, ClaimType = "Edit Student", ClaimValue = "Edit Student"
                        }
                    };
                    dbContext.UserClaims.AddRange(userClaims);

                    dbContext.SaveChanges();

                    #endregion
                }
                else
                {
                    Console.WriteLine("无需创建种子数据……");
                }
            }

            return builder;
        }
    }
}