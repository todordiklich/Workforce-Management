namespace WFM.BLL.Interfaces
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; set; }

        int SmtpPort { get; set; }

        string SmtpNormalizedUserName { get; set; }

        string SmtpUsername { get; set; }

        string SmtpPassword { get; set; }

        bool AwaitTask { get; set; }

        bool SecureConnection { get; set; }

        bool RequireAuthentication { get; set; }
    }
}
