using System.Threading.Tasks;

namespace WFM.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendDayOffNotificationAsync(
            string toName,
            string toEmailAddress,
            string subject,
            string message
            );
    }
}
