using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BullfrogAPI.Extensions
{
    public static class InputValidationExtension
    {
        public static bool ValidateDealerName(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (Regex.IsMatch(text, "^[A-Za-z0-9\\s-()\"\']*$"))
                return true;
            return false;
        }

        public static bool ValidateAlphaCharactersOnly(this string text)
        {
            if (Regex.IsMatch(text, "^[A-Za-z\\s]*$"))
                return true;
            return false;
        }

        public static bool ValidateCabinetColor(this string text)
        {
            if (Regex.IsMatch(text, "^[A-Za-z0-9\\s-()\"\']*$"))
                return true;
            return false;
        }

        public static bool ValidatePhoneNumber(this string text)
        {
            if (Regex.IsMatch(text, "^[0-9\\s-()]*$"))
                return true;
            return false;
        }

        public static bool ValidateAlphaNumericCharactersOnly(this string text)
        {
            if (Regex.IsMatch(text, "^[A-Za-z0-9\\s]*$"))
                return true;
            return false;
        }

        public static bool ValidateStreetAddress(this string text)
        {

            return true;


        }

        public static bool ValidateNumeric(this string text)
        {
            if (Regex.IsMatch(text, "^[0-9]*$"))
                return true;
            return false;
        }

        public static bool ValidateDate(this string text)
        {
            if (Regex.IsMatch(text, "^[0-9]{2}/[0-9]{2}/[0-9]{4}$"))
                return true;
            return false;
        }

        public static bool ValidatePartNumber(this string text)
        {
            if (Regex.IsMatch(text, "^[0-9]{2}-[0-9]{4}$") || Regex.IsMatch(text, "^[0-9]{2}-[0-9]{5}$"))
                return true;
            return false;
        }

        public static bool ValidateEmailAddress(this string email)
        {
            try
            {
                MailAddress mail = new MailAddress(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
