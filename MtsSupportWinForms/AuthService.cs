using System.Configuration;

namespace MtsSupportWinForms
{
    public static class AuthService
    {
        public static UserAccount Authenticate(string email)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (email.Length == 0)
            {
                return null;
            }

            if (email == Read("AdminEmail"))
            {
                return new UserAccount { Email = email, FullName = "Администратор системы", Role = UserRole.Administrator };
            }
            if (email == Read("OperatorEmail"))
            {
                return new UserAccount { Email = email, FullName = "Оператор 1 линии", Role = UserRole.OperatorLine1 };
            }
            if (email == Read("EngineerEmail"))
            {
                return new UserAccount { Email = email, FullName = "Оператор 2 линии", Role = UserRole.SpecialistLine2 };
            }

            return null;
        }

        private static string Read(string key)
        {
            return (ConfigurationManager.AppSettings[key] ?? string.Empty).Trim().ToLowerInvariant();
        }
    }
}
