using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.DAL.Repositories.Interfaces;
using WFM.Models.Requests.TeamRequests;
using WFM.Common.CustomExceptions;

namespace WFM.BLL.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TeamService(ITeamRepository teamRepository, IUserRepository userRepository, IDateTimeProvider dateTimeProvider)
        {
            _teamRepository = teamRepository;
            _userRepository = userRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Team> AddMemberAsync(TeamAddRemoveMemberDTO teamAddRemoveMemberDTO, Guid currentUserId)
        {
            User user = await _userRepository.FindById(teamAddRemoveMemberDTO.UserId);
            if (user == null)
            {
                return null;
            }

            Team team = await _teamRepository.GetByIdAsync(teamAddRemoveMemberDTO.TeamId);

            if (team == null)
            {
                return null;
            }

            team.TeamMembers.Add(user);

            team.ModifiedOn = DateTime.UtcNow;
            team.ModifiedBy = currentUserId;

            return await _teamRepository.AddTeamMember(team);
        }

        public async Task<Team> RemoveMemberAsync(TeamAddRemoveMemberDTO teamAddRemoveMemberDTO, Guid currentUserId)
        {
            Team team = await _teamRepository.GetByIdWithMembers(teamAddRemoveMemberDTO.TeamId, teamAddRemoveMemberDTO.UserId);

            if (team == null)
            {
                return null;
            }

            User user = await _userRepository.FindById(teamAddRemoveMemberDTO.UserId);
            if (user == null)
            {
                return null;
            }

            team.TeamMembers.Remove(user);

            return await _teamRepository.UpdateTeamAsync(team, currentUserId);
        }

        public async Task<Team> CreateAsync(Team team, Guid creatorUserId)
        {
            if (await _teamRepository.GetByNameAsync(team.TeamName) != null)
            {
                return null;
            }

            User teamLeader = await _userRepository.FindById(team.TeamLeaderId);

            if (teamLeader == null)
            {
                return null;
            }

            var dateTimeNow = _dateTimeProvider.UtcNow;
            team.CreatedOn = dateTimeNow;
            team.ModifiedOn = dateTimeNow;
            team.CreatedBy = creatorUserId;
            team.ModifiedBy = creatorUserId;
            team.TeamMembers.Add(teamLeader);

            return await _teamRepository.AddTeamAsync(team);
        }

        public async Task<Team> DeleteAsync(Team team, Guid currentUserId)
        {
            team.IsDeleted = true;
            team.DeletedOn = _dateTimeProvider.UtcNow;
            team.ModifiedBy = currentUserId;

            return await _teamRepository.DeleteTeamAsync(team);
        }

        public async Task<Team> UpdateAsync(Team team, Guid currentUserId)
        {
            if (await _teamRepository.GetByNameAsync(team.TeamName) != null)
            {
                throw new CustomApplicationException($"There is already a team with name {team.TeamName}");
            }

            User teamLeader = await _userRepository.FindById(team.TeamLeaderId);

            if (teamLeader == null)
            {
                throw new CustomApplicationException($"No team leader with id {team.TeamLeaderId} found");
            }

            team.ModifiedOn = _dateTimeProvider.UtcNow;
            team.ModifiedBy = currentUserId;
            team.TeamMembers.Add(teamLeader);

            return await _teamRepository.UpdateTeamAsync(team, currentUserId);
        }

        public async Task<Team> GetByNameAsync(string name)
        {
            return await _teamRepository.GetByNameAsync(name);
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _teamRepository.GetAllAsync();
        }

        public async Task<Team> GetByIdAsync(Guid id)
        {
            return await _teamRepository.GetByIdAsync(id);
        }
    }
}
