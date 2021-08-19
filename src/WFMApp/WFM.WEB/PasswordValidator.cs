using System.Linq;
using IdentityServer4.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

using WFM.DAL.Entities;

namespace WFM.WEB
{
    public class PasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<User> _userManager;

        public PasswordValidator(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        //This method validates the user credentials and if successful the IdentiryServer will build a token from the context.Result object
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            User user = await FindByNameOrEmail(context.UserName);

            if (user != null)
            {
                bool authResult = await ValidateUserCredentials(context.UserName, context.Password);
                if (authResult)
                {
                    List<string> roles = await GetUserRolesAsync(user);

                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, user.UserName));

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    context.Result = new GrantValidationResult(subject: user.Id.ToString(), authenticationMethod: "password", claims: claims);
                }
                else
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials");
                }

                return;
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid credentials");
        }

        private async Task<bool> ValidateUserCredentials(string userName, string password)
        {
            User user = await FindByNameOrEmail(userName);

            if (user != null)
            {
                bool result = await _userManager.CheckPasswordAsync(user, password);
                return result;
            }

            return false;
        }

        private async Task<List<string>> GetUserRolesAsync(User user)
        {
            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        private async Task<User> FindByNameOrEmail(string userName)
        {
            User user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userName);
            }

            return user;
        }
    }
}
