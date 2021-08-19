using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.Common;

namespace WFM.WEB.CustomAuthorization
{
    public class CreatorHandler : AuthorizationHandler<CreatorRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly ITimeOffRequestService _timeOffRequestService;

        public CreatorHandler(IHttpContextAccessor httpContextAccessor, IUserService userService, ITimeOffRequestService timeOffRequestService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _timeOffRequestService = timeOffRequestService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CreatorRequirement requirement)
        {
            string timeOffRequestId = _httpContextAccessor.HttpContext.GetRouteValue("timeOffRequestId")?.ToString();
            if (timeOffRequestId == null)
            {
                context.Fail();
                return;
            }

            Guid timeOffRequestIdAsGuid = Guid.Parse(timeOffRequestId);

            string currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
            {
                context.Fail();
                return;
            }

            Guid currentUserIdAsGuid = Guid.Parse(currentUserId);
            User currentUser = await _userService.GetByIdAsync(currentUserIdAsGuid);

            if (await _timeOffRequestService.CheckIfUserIsOwner(timeOffRequestIdAsGuid, currentUser) || await _userService.IsUserInRole(currentUser, GlobalConstants.AdminRoleName))
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
            return;
        }
    }
}
