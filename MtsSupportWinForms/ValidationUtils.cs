using System;
using System.Text.RegularExpressions;

namespace MtsSupportWinForms
{
    public static class ValidationUtils
    {
        private static readonly Regex PhoneRegex = new Regex(@"^[0-9+()\-\s]{6,20}$", RegexOptions.Compiled);
        private static readonly Regex NameRegex = new Regex(@"^[А-Яа-яA-Za-zЁё\-\s]{2,120}$", RegexOptions.Compiled);
        private static readonly Regex SerialRegex = new Regex(@"^[A-Za-z0-9\-_/]{3,64}$", RegexOptions.Compiled);

        private static readonly Regex SearchRegex = new Regex(@"^[А-Яа-яA-Za-z0-9Ёё\-\s_.,()]*$", RegexOptions.Compiled);

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var _ = new System.Net.Mail.MailAddress(email.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone) => !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone.Trim());
        public static bool IsValidPhonePlus7(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digits = Regex.Replace(phone, @"\D", string.Empty);
            return digits.Length == 11 && digits.StartsWith("7");
        }
        public static bool IsValidPersonName(string fio) => !string.IsNullOrWhiteSpace(fio) && NameRegex.IsMatch(fio.Trim());
        public static bool IsValidSerial(string serial) => !string.IsNullOrWhiteSpace(serial) && SerialRegex.IsMatch(serial.Trim());

        public static bool IsFutureDate(DateTime value) => value > DateTime.Now.AddMinutes(5);
        public static bool IsValidSearchText(string text) => text == null || (text.Length <= 100 && SearchRegex.IsMatch(text));
    }
}
