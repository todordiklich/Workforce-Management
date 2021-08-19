using WFM.BLL.Interfaces;

namespace WFM.BLL.Services
{
    public class EmailConfiguration : IEmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpNormalizedUserName { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool AwaitTask { get; set; }
        public bool SecureConnection { get; set; }
        public bool RequireAuthentication { get; set; }
    }
}
