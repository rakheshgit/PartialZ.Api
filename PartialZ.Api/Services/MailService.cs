using Microsoft.Extensions.Logging;
using PartialZ.Api.Services.Interfaces;
using System.Net.Mail;
using System.Net;
using PartialZ.DataAccess.PartialZDB;
using PartialZ.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PartialZ.Api.Enums;

namespace PartialZ.Api.Services
{
    public class MailService: IMailService
    {
        private PartialZContext _PartialZContext;
        private ICryptographyService _cryptographyService;
        private IConfiguration _configuration { get; }
        public MailService(PartialZContext PartialZContext, IConfiguration configuration,
            ICryptographyService cryptographyService)
        {
            this._PartialZContext = PartialZContext;
            this._cryptographyService = cryptographyService;
            this._configuration = configuration;
        }
        public void SendVerificationMail(string toMailID)
        {
            try
            {
                using (MailMessage user = new MailMessage("PartialZ<" + this._configuration.GetValue<string>("Mail:FromEmail") + ">", toMailID.Trim()))
                {
                    var Template = GetTemplate((int)EmailEnum.EmailVerification).Result;
                    string myString = "";
                    myString = Template.Template;
                    myString = myString.Replace("$$VERIFICATION_URL$$", this._configuration.GetValue<string>("Mail:TrgetURL") + this._cryptographyService.Encrypt(toMailID));
                    user.Bcc.Add(new MailAddress(this._configuration.GetValue<string>("Mail:BCC")));
                    user.Subject = Template.Subject;
                    user.Body = myString.ToString();
                    user.IsBodyHtml = true;
                    Send(user);
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void SendOTP(string toMailID,string OTP)
        {
            try
            {
                using (MailMessage user = new MailMessage("PartialZ<" + this._configuration.GetValue<string>("Mail:FromEmail") + ">", toMailID.Trim()))
                {
                    var Template = GetTemplate((int)EmailEnum.OTP).Result;
                    string myString = "";
                    myString = Template.Template;
                    myString = myString.Replace("$$OTP_CODE$$",OTP);
                    user.Bcc.Add(new MailAddress(this._configuration.GetValue<string>("Mail:BCC")));
                    user.Subject = Template.Subject;
                    user.Body = myString.ToString();
                    user.IsBodyHtml = true;
                    Send(user);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Send(MailMessage user)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = this._configuration.GetValue<string>("Mail:Host");
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            NetworkCredential NetworkCred = new NetworkCredential(this._configuration.GetValue<string>("Mail:Username"), this._configuration.GetValue<string>("Mail:Password"));
            //smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = Convert.ToInt32(this._configuration.GetValue<string>("Mail:Port"));
            try
            {
                smtp.Send(user);
            }
            catch (SmtpException ex)
            {
                throw ex;
            }
        }
        private async Task<EmailTemplateDto> GetTemplate(int TempID)
        {
            try
            {
                return await this._PartialZContext.EmailTemplates.Where(e => e.Id == TempID && e.IsActive==1).Select(e => new EmailTemplateDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Subject = e.Subject,
                    Template = e.Template
                }).FirstAsync();
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
