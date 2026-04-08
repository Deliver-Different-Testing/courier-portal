using System;
using System.Collections.Generic;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;

namespace CourierPortal.Core.Validators.Admin
{
    public class NonExpiringJwtValidator(IJsonSerializer serializer, IDateTimeProvider dateTimeProvider) : IJwtValidator
    {
        // Original method
        public void Validate(string payloadJson, string[] validationParameters)
        {
            // Parse payload but skip expiration validation
            var payloadData = serializer.Deserialize<Dictionary<string, object>>(payloadJson);
            var now = dateTimeProvider.GetNow();
        
            // Check "nbf" (not before) claim
            if (payloadData.TryGetValue("nbf", out object value))
            {
                var nbf = ConvertToUnixDateTime(value);
                if (nbf > now)
                {
                    throw new TokenNotYetValidException($"Token is not valid before {nbf}");
                }
            }
        
            // Check "iat" (issued at) claim
            if (payloadData.TryGetValue("iat", out var value1))
            {
                var iat = ConvertToUnixDateTime(value1);
                if (iat > now)
                {
                    throw new TokenNotYetValidException($"Token was issued in the future: {iat}");
                }
            }
        
            // Validate issuer if provided in parameters
            if (validationParameters is { Length: > 0 })
            {
                // Implement issuer, audience validation based on your needs
            }
        
            // Intentionally NOT checking the "exp" claim
        }
    
        private DateTime ConvertToUnixDateTime(object secondsSinceEpoch)
        {
            if (secondsSinceEpoch is long longValue)
            {
                return DateTimeOffset.FromUnixTimeSeconds(longValue).DateTime;
            }
            else if (secondsSinceEpoch is int intValue)
            {
                return DateTimeOffset.FromUnixTimeSeconds(intValue).DateTime;
            }
            else
            {
                throw new FormatException("Expected integer value for date");
            }
        }
    
        // Additional required methods
        public void Validate(string payloadJson, string issuer, params string[] audiences)
        {
            // Skip validation but can check issuer/audience if needed
        }
    
        public void Validate(string payloadJson, IAsymmetricAlgorithm algorithm, byte[] key, byte[] initVector)
        {
            // Skip validation for asymmetric algorithms
        }

        public bool TryValidate(string payloadJson, string signature, string decodedSignature, out Exception ex)
        {
            //Always valid, no exception
            ex = null;
            return true;
        }

        public bool TryValidate(string payloadJson, string issuer, string[] audiences, out Exception ex)
        {
            // Always valid, no exception
            ex = null;
            return true;
        }
    
        public bool TryValidate(string payloadJson, IAsymmetricAlgorithm algorithm, byte[] key, byte[] initVector, out Exception ex)
        {
            // Always valid, no exception
            ex = null;
            return true;
        }
    
        public bool TryValidate(string payloadJson, string[] validationParameters, out Exception ex)
        {
            // Always valid, no exception
            ex = null;
            return true;
        }
    }
}
