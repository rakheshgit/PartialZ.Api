using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartialZ.Api.Dtos;
using PartialZ.Api.Services.Interfaces;
using PartialZ.DataAccess.PartialZDB;
using System.Runtime.InteropServices;

namespace PartialZ.Api.Services
{
    public class EmployeeService : IEmployee
    {
        private PartialZContext _PartialZContext;
        private ICryptographyService _cryptographyService;
        private IMailService _mailService;
        public EmployeeService(PartialZContext PartialZContext, IMailService mailService,
            ICryptographyService cryptographyService)
        {
            this._PartialZContext = PartialZContext;
            this._cryptographyService = cryptographyService;
           this._mailService = mailService;
        }
        public async Task<EmployeeDto> GetEmployee(string EmailID)
        {
            return await this._PartialZContext.Employees.Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Password = e.Password,
                BusinessTitle = e.BusinessTitle,
                PhoneNumber = e.PhoneNumber
            }).Where(e => e.Email == EmailID).FirstAsync();
        }
        public async Task<int> RegsregisterEmployee(string emailID, string password)
        {
            try
            {
                bool isverified = false;
                if (this._PartialZContext.Employees.Where(e => e.Email == emailID).Any())
                {
                    //update
                    var existingdata = await this._PartialZContext.Employees.Where(e => e.Email == emailID).FirstAsync();
                    existingdata.Email = emailID;
                    existingdata.Password = this._cryptographyService.Encrypt(password);
                    existingdata.LastModifedDate = DateTime.UtcNow;
                    if (existingdata.IsVerified == 1)
                    {
                        isverified=true;
                    }
                }
                else
                {
                    //insert
                    var data = new Employee()
                    {
                        Email = emailID,
                        Password = this._cryptographyService.Encrypt(password)
                    };
                    await this._PartialZContext.Employees.AddAsync(data);
                }
                var result= await this._PartialZContext.SaveChangesAsync();
                //send mail sync
                if(!isverified)
                this._mailService.SendVerificationMail(emailID);                
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> VerifyEmployee(string emailID)
        {
            try
            {
                emailID=this._cryptographyService.Decrypt(emailID);
                if (this._PartialZContext.Employees.Where(e => e.Email == emailID).Any())
                {
                    //update
                    var existingdata = await this._PartialZContext.Employees.Where(e => e.Email == emailID).FirstAsync();
                    existingdata.IsVerified = 1;
                    existingdata.LastModifedDate = DateTime.UtcNow;
                    return await this._PartialZContext.SaveChangesAsync();
                }
                else
                {
                    //bad request
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> EmplpyeeDetailRegistration(AffidavitDto affidavitDto)
        {
            try
            {
                int employeeID = 0;
                affidavitDto.Email = this._cryptographyService.Decrypt(affidavitDto.Email);
                if (this._PartialZContext.Employees.Where(e => e.Email == affidavitDto.Email).Any())
                {
                    //update
                    var existingdata = await this._PartialZContext.Employees.Where(e => e.Email == affidavitDto.Email).FirstAsync();
                    existingdata.Email = affidavitDto.Email;
                    existingdata.FirstName = affidavitDto.FirstName;
                    existingdata.LastName = affidavitDto.LastName;
                    existingdata.BusinessTitle = affidavitDto.BusinessTitle;
                    existingdata.PhoneNumber = affidavitDto.PhoneNumber;
                    existingdata.LastModifedDate = DateTime.UtcNow;
                    await this._PartialZContext.SaveChangesAsync();
                    employeeID = existingdata.EmployeeId;
                }
                else
                {
                    //insert
                    var data = new Employee()
                    {
                        Email = affidavitDto.Email,
                        FirstName = affidavitDto.FirstName,
                        LastName = affidavitDto.LastName,
                        BusinessTitle = affidavitDto.BusinessTitle,
                        PhoneNumber = affidavitDto.PhoneNumber
                    };
                    await this._PartialZContext.Employees.AddAsync(data);
                    await this._PartialZContext.SaveChangesAsync();
                    employeeID =data.EmployeeId;
                }                
                return employeeID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
