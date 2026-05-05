using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MusicRepairShop.Utils
{
    public static class ValidationService
    {
        public static List<string> Validate(object obj)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(obj);
            Validator.TryValidateObject(obj, context, validationResults, true);
            
            return validationResults.Select(v => v.ErrorMessage).ToList();
        }

        public static bool IsValid(object obj)
        {
            return !Validate(obj).Any();
        }

        public static bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return phone.StartsWith("+") || phone.All(char.IsDigit);
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Email не обязателен

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}