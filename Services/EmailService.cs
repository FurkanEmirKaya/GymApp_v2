using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace GymApp_v2.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Şifre Sıfırlama Talebi";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                            <h1 style='color: #343a40; margin: 0;'>Gym App</h1>
                        </div>
                        
                        <div style='padding: 30px; background-color: white;'>
                            <h2 style='color: #495057; margin-top: 0;'>Şifre Sıfırlama Talebi</h2>
                            
                            <p style='color: #6c757d; line-height: 1.6;'>
                                Merhaba,
                            </p>
                            
                            <p style='color: #6c757d; line-height: 1.6;'>
                                Gym App hesabınız için şifre sıfırlama talebi aldık. 
                                Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:
                            </p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' style='display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                    Şifremi Sıfırla
                                </a>
                            </div>
                            
                            <p style='color: #6c757d; line-height: 1.6; font-size: 14px;'>
                                Eğer bu talebi siz yapmadıysanız, bu e-postayı dikkate almayın. 
                                Hiçbir değişiklik yapılmayacaktır.
                            </p>
                            
                            <p style='color: #6c757d; line-height: 1.6; font-size: 14px;'>
                                Bu bağlantı 24 saat geçerlidir.
                            </p>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d;'>
                            <p>Bu e-posta otomatik olarak gönderilmiştir, lütfen yanıtlamayınız.</p>
                            <p>&copy; 2025 Gym App. Tüm hakları saklıdır.</p>
                        </div>
                    </div>
                ",
                TextBody = $@"
                    Gym App - Şifre Sıfırlama Talebi
                    
                    Merhaba,
                    
                    Gym App hesabınız için şifre sıfırlama talebi aldık.
                    
                    Şifrenizi sıfırlamak için aşağıdaki bağlantıyı tıklayın:
                    {resetLink}
                    
                    Eğer bu talebi siz yapmadıysanız, bu e-postayı dikkate almayın.
                    
                    Bu bağlantı 24 saat geçerlidir.
                    
                    Saygılarımızla,
                    Gym App Ekibi
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, 
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Log the error
                throw new Exception("Email gönderimi sırasında hata oluştu.", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}