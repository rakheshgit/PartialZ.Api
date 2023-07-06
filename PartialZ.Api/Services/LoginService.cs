using Microsoft.EntityFrameworkCore;
using PartialZ.Api.Services.Interfaces;
using PartialZ.DataAccess.PartialZDB;
using System.Xml.Linq;

namespace PartialZ.Api.Services
{
    public class LoginService : ILoginService
    {
        private PartialZContext _PartialZContext;
        private ICryptographyService _cryptographyService;
        private IMailService _mailService;
        public LoginService(PartialZContext PartialZContext,
            IMailService mailService,
            ICryptographyService cryptographyService)
        {
            this._PartialZContext = PartialZContext;
            this._cryptographyService = cryptographyService;
            this._mailService = mailService;
        }
        public async Task<string> Login(string emailID, string password)
        {
            try
            {
                password = this._cryptographyService.Encrypt(password);
                if (this._PartialZContext.Employees.Where(e => e.Email == emailID && e.Password == password && e.IsVerified == 1).Any())
                {
                    string instantOTP = GenerateOTP();
                    SaveOTP(emailID, instantOTP);
                    this._mailService.SendOTP(emailID, instantOTP);
                    return "We have sent you the otp to register email.";

                }
                else if (this._PartialZContext.Employees.Where(e => e.Email == emailID && e.Password == password).Any())
                {
                    var existingdata = await this._PartialZContext.Employees.Where(e => e.Email == emailID && e.Password == password).FirstAsync();
                    if (existingdata != null && existingdata.IsVerified == 0)
                    {
                        return "Your account is inactive,please check your email to activate your account";
                    }
                    else
                    {
                        return "Invalid credentials";
                    }
                }
                else
                {
                    return "Invalid credentials";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string ValidateOTP(string emailID, string OTP)
        {
            try
            {
                int cotp = Convert.ToInt32(OTP);
                if (this._PartialZContext.Employees.Where(e => e.Email == emailID && e.LoginOtp == cotp && e.IsVerified == 1).Any())
                {
                    return "OTP verified successfully";

                }
                else
                {
                    return "Invalid OTP";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveOTP(string emailID, string OTP)
        {
            try
            {

                if (this._PartialZContext.Employees.Where(e => e.Email == emailID && e.IsVerified == 1).Any())
                {
                    var Data = this._PartialZContext.Employees.Where(e => e.Email == emailID && e.IsVerified == 1).Single();
                    Data.LoginOtp =Convert.ToInt32(OTP);
                    Data.LastModifedDate= DateTime.Now;
                    this._PartialZContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static string GenerateOTP()
        {
            Random random = new Random();
            string otp = "";

            for (int i = 0; i < 6; i++)
            {
                otp += random.Next(0, 9).ToString();
            }

            return otp;
        }
    }
}
