using System;
using CourierPortal.Infrastructure.Models;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class TokenDto
    {

        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public string RefreshToken { get; set; }
        public AccountType AccountType { get; set; }
        public int? CourierType { get; set; }
        public bool PasswordResetRequired { get; set; }
        public string CountryCode { get; set; }
    }
}
