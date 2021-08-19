using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using WFM.Common;
using WFM.DAL.Context;
using WFM.DAL.Entities;

namespace WFM.DAL.Seeder
{
    public class DatabaseSeeder
    {
        private const int paidDaysOff = 20;
        private const int unpaidDaysOff = 90;
        private const int sickLeaveDaysOff = 40;

        public static void Seed(IServiceProvider applicationServices)
        {
            using (IServiceScope serviceScope = applicationServices.CreateScope())
            {
                AppEntityContext context = serviceScope.ServiceProvider.GetRequiredService<AppEntityContext>();

                if (context.Database.EnsureCreated())
                {
                    PasswordHasher<User> hasher = new PasswordHasher<User>();

                    //seed user roles
                    IdentityRole<Guid> adminRole = new IdentityRole<Guid>()
                    {
                        Id = Guid.NewGuid(),
                        Name = GlobalConstants.AdminRoleName,
                        NormalizedName = GlobalConstants.AdminRoleName.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString("D")
                    };

                    context.Roles.Add(adminRole);

                    IdentityRole<Guid> regularRole = new IdentityRole<Guid>()
                    {
                        Id = Guid.NewGuid(),
                        Name = GlobalConstants.RegularRoleName,
                        NormalizedName = GlobalConstants.RegularRoleName.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString("D")
                    };

                    context.Roles.Add(regularRole);

                    //seed default days off
                    DaysOffLimitDefault daysOffLimitDefaultBG = new()
                    {
                        CountryCode = GlobalConstants.CountryCodeBG,
                        PaidDaysOff = paidDaysOff,
                        UnpaidDaysOff = unpaidDaysOff,
                        SickLeaveDaysOff = sickLeaveDaysOff
                    };

                    DaysOffLimitDefault daysOffLimitDefaultMK = new()
                    {
                        CountryCode = GlobalConstants.CountryCodeMK,
                        PaidDaysOff = paidDaysOff,
                        UnpaidDaysOff = unpaidDaysOff,
                        SickLeaveDaysOff = sickLeaveDaysOff
                    };

                    context.DaysOffLimitDefault.Add(daysOffLimitDefaultBG);
                    context.DaysOffLimitDefault.Add(daysOffLimitDefaultMK);

                    context.SaveChanges();

                    //seed admin
                    User admin = new User()
                    {
                        Id = Guid.NewGuid(),
                        Email = "admin@test.test",
                        NormalizedEmail = "admin@test.test".ToUpper(),
                        UserName = "admin",
                        NormalizedUserName = "admin".ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        CreatedOn = DateTime.UtcNow,
                        FirstName = "admin",
                        LastName = "admin",
                        AvailablePaidDaysOff = paidDaysOff,
                        AvailableUnpaidDaysOff = unpaidDaysOff,
                        AvailableSickLeaveDaysOff = sickLeaveDaysOff,
                        CountryOfResidence = GlobalConstants.CountryCodeBG,
                    };

                    admin.PasswordHash = hasher.HashPassword(admin, "adminpass");

                    IdentityUserRole<Guid> identityUserRoleAdmin = new IdentityUserRole<Guid>() { UserId = admin.Id, RoleId = adminRole.Id };

                    context.Users.Add(admin);
                    context.UserRoles.Add(identityUserRoleAdmin);

                    //seed regular users
                    User regular1 = new User()
                    {
                        Id = Guid.NewGuid(),
                        Email = "regular1@test.test",
                        NormalizedEmail = "regular1@test.test".ToUpper(),
                        UserName = "regular1",
                        NormalizedUserName = "regular1".ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        CreatedOn = DateTime.UtcNow,
                        FirstName = "regular1",
                        LastName = "regular1",
                        AvailablePaidDaysOff = paidDaysOff,
                        AvailableUnpaidDaysOff = unpaidDaysOff,
                        AvailableSickLeaveDaysOff = sickLeaveDaysOff,
                        CountryOfResidence = GlobalConstants.CountryCodeBG,
                    };

                    regular1.PasswordHash = hasher.HashPassword(regular1, "regular1pass");

                    IdentityUserRole<Guid> regular1Role = new IdentityUserRole<Guid>() { UserId = regular1.Id, RoleId = regularRole.Id };

                    context.Users.Add(regular1);
                    context.UserRoles.Add(regular1Role);

                    User regular2 = new User()
                    {
                        Id = Guid.NewGuid(),
                        Email = "regular2@test.test",
                        NormalizedEmail = "regular2@test.test".ToUpper(),
                        UserName = "regular2",
                        NormalizedUserName = "regular2".ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        CreatedOn = DateTime.UtcNow,
                        FirstName = "regular2",
                        LastName = "regular2",
                        AvailablePaidDaysOff = paidDaysOff,
                        AvailableUnpaidDaysOff = unpaidDaysOff,
                        AvailableSickLeaveDaysOff = sickLeaveDaysOff,
                        CountryOfResidence = GlobalConstants.CountryCodeBG,
                    };

                    regular2.PasswordHash = hasher.HashPassword(regular2, "regular2pass");

                    IdentityUserRole<Guid> regular2Role = new IdentityUserRole<Guid>() { UserId = regular2.Id, RoleId = regularRole.Id };

                    context.Users.Add(regular2);
                    context.UserRoles.Add(regular2Role);

                    User regular3 = new User()
                    {
                        Id = Guid.NewGuid(),
                        Email = "regular3@test.test",
                        NormalizedEmail = "regular3@test.test".ToUpper(),
                        UserName = "regular3",
                        NormalizedUserName = "regular3".ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        CreatedOn = DateTime.UtcNow,
                        FirstName = "regular3",
                        LastName = "regular3",
                        AvailablePaidDaysOff = paidDaysOff,
                        AvailableUnpaidDaysOff = unpaidDaysOff,
                        AvailableSickLeaveDaysOff = sickLeaveDaysOff,
                        CountryOfResidence = GlobalConstants.CountryCodeMK,
                    };

                    regular3.PasswordHash = hasher.HashPassword(regular3, "regular3pass");

                    IdentityUserRole<Guid> regular3Role = new IdentityUserRole<Guid>() { UserId = regular3.Id, RoleId = regularRole.Id };

                    context.Users.Add(regular3);
                    context.UserRoles.Add(regular3Role);

                    User regularNoTeam = new User()
                    {
                        Id = Guid.NewGuid(),
                        Email = "regularNoTeam@test.test",
                        NormalizedEmail = "regularNoTeam@test.test".ToUpper(),
                        UserName = "regularNoTeam",
                        NormalizedUserName = "regularNoTeam".ToUpper(),
                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        CreatedOn = DateTime.UtcNow,
                        FirstName = "regularNoTeam",
                        LastName = "regularNoTeam",
                        AvailablePaidDaysOff = paidDaysOff,
                        AvailableUnpaidDaysOff = unpaidDaysOff,
                        AvailableSickLeaveDaysOff = sickLeaveDaysOff,
                        CountryOfResidence = GlobalConstants.CountryCodeMK,
                    };

                    regularNoTeam.PasswordHash = hasher.HashPassword(regularNoTeam, "regularnoteampass");

                    IdentityUserRole<Guid> regularNoTeamRole = new IdentityUserRole<Guid>() { UserId = regularNoTeam.Id, RoleId = regularRole.Id };

                    context.Users.Add(regularNoTeam);
                    context.UserRoles.Add(regularNoTeamRole);

                    context.SaveChanges();

                    //seed team
                    Team team = new Team
                    {
                        Id = Guid.NewGuid(),
                        TeamName = "testTeam",
                        Description = "test description",
                        TeamLeaderId = regular1.Id,
                        TeamMembers = new List<User>() { admin, regular3 }
                    };

                    Team team1 = new Team
                    {
                        Id = Guid.NewGuid(),
                        TeamName = "testTeam1",
                        Description = "test description",
                        TeamLeaderId = regular2.Id,
                        TeamMembers = new List<User>() { admin, regular3 }
                    };

                    context.Team.Add(team);
                    context.Team.Add(team1);

                    context.SaveChanges();
                }
            }
        }
    }
}
