using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CourierPortal.Infrastructure.Services
{
    public class EmailService()
    {
        private readonly int _sendAttempts = 2;

        public bool IsValid(EmailRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.To)
                || string.IsNullOrWhiteSpace(request.Subject)
                || string.IsNullOrWhiteSpace(request.Message)
                || (request.Attachments != null && request.Attachments.Any(a => string.IsNullOrWhiteSpace(a.FileName)
                                                                                || string.IsNullOrWhiteSpace(a.Type)
                                                                                || a.Data == null
                                                                                || a.Data.Length < 1)))
                return false;

            return true;
        }

        public async Task<BaseResponse> SendEmail(EmailRequest request)
        {
            var response = new BaseResponse(request.MessageId);

            if (!IsValid(request))
            {
                response.Messages.Add(new MessageDto() { Message = $"Email params are invalid." });
                return response;
            }

            var attempts = 0;
            while (!response.Success && attempts < _sendAttempts)
            {
                try
                {
                    using var smtpClient = new SmtpClient(Environment.GetEnvironmentVariable("SmtpServer"), int.Parse(Environment.GetEnvironmentVariable("SmtpPort")));
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("SmtpUsername"), Environment.GetEnvironmentVariable("SmtpPassword"));

                    var mailMessage = new MailMessage();
                    mailMessage.Headers.Add("Message-ID", $"{Guid.NewGuid()}@urgent.co.nz"); //Google security automation spam
                    mailMessage.From = new MailAddress(Environment.GetEnvironmentVariable("UrgentArmyEmail"));
                    mailMessage.To.Add(request.To);
                    mailMessage.Subject = request.Subject;
                    mailMessage.IsBodyHtml = request.IsBodyHtml;
                    mailMessage.Body = request.Message;

                    if (request.Attachments != null)
                        foreach (var emailAttachmentDto in request.Attachments)
                            mailMessage.Attachments.Add(new Attachment(new MemoryStream(emailAttachmentDto.Data), emailAttachmentDto.FileName, emailAttachmentDto.Type)
                            {
                                ContentId = emailAttachmentDto.ContentId
                            });

                    await smtpClient.SendMailAsync(mailMessage);
                    response.Success = true;
                }
                catch (Exception ex)
                {
                    attempts += 1;
                    if (attempts == _sendAttempts)
                    {
                        response.Messages.Add(new MessageDto() { Message = $"Failed to send email after {_sendAttempts} attempts." });
                        Log.Error(ex.InnerException == null ? ex.ToString() : ex.ToString() + Environment.NewLine + ex.InnerException.ToString());
                    }
                }
            }

            return response;
        }

    }
}
