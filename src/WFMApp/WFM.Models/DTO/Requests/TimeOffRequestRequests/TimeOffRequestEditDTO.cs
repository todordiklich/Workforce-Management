using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WFM.Common;

namespace WFM.Models.DTO.Requests.TimeOffRequestRequests
{
    public class TimeOffRequestEditDTO : IValidatableObject
    {
        [Required]
        [MaxLength(GlobalConstants.DescriptionLength)]
        public string Description { get; set; }

        [ValidDate]
        [Required]
        public DateTime StartDate { get; set; }

        [ValidDate]
        [Required]
        public DateTime EndDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> result = new List<ValidationResult>();
            if (EndDate < StartDate)
            {
                result.Add(new ValidationResult("The end day of your time off request cannot precede the start date.", new string[] { "StartDate", "EndDate" }));
            }
            return result;
        }

        public class ValidDateAttribute : RequiredAttribute
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
