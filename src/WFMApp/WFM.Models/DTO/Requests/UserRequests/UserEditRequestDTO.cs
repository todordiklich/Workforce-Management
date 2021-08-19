using System;
using System.ComponentModel.DataAnnotations;

namespace WFM.Models.Requests.UserRequests
{
    public class UserEditRequestDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryOfResidence { get; set; }

        [Required]
        [Range(0, 30)]
        public int AvailablePaidDaysOff { get; set; }

        [Required]
        [Range(0, 100)]
        public int AvailableUnpaidDaysOff { get; set; }

        [Required]
        [Range(0, 200)]
        public int AvailableSickLeaveDaysOff { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
