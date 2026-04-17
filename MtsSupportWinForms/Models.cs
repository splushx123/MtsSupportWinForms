namespace MtsSupportWinForms
{
    public enum UserRole
    {
        Administrator,
        OperatorLine1,
        SpecialistLine2
    }

    public sealed class UserAccount
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
    }
}
