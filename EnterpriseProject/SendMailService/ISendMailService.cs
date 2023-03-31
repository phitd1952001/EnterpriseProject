using System.Threading.Tasks;
using EnterpriseProject.SendMailService;

public interface ISendMailService {
    Task SendMail(MailContent mailContent);
    
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}