namespace WFM.Models.DTO.Responses.UserResponses
{
    public class UserResponseDTO : BaseResponseDTO
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryOfResidence { get; set; }
        public int AvailablePaidDaysOff { get; set; }
        public int AvailableUnpaidDaysOff { get; set; }
        public int AvailableSickLeaveDaysOff { get; set; }
    }
}
