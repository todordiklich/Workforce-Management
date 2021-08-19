using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WFM.Models.Requests.UserRequests
{
    public class UserCreateRequestDTO : IValidatableObject
    {
        [Required]
        [MinLength(4)]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public string RoleName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryOfResidence { get; set; }

        [PasswordRepeat]
        [MinLength(8)]
        public string Password { get; set; }

        [PasswordRepeat]
        [MinLength(8)]
        public string RepeatPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> result = new List<ValidationResult>();
            if (Password != RepeatPassword)
            {
                result.Add(new ValidationResult("Passwords do not match", new string[] { "Password" }));
            }
            return result;
        }

        public class PasswordRepeatAttribute : RequiredAttribute
        {
            public override bool IsValid(object value)
            {
                return base.IsValid(value);
            }
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                return base.IsValid(value, validationContext);
            }
        }
    }
}
