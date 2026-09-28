namespace WebAPI.DTOs
{
    public class CheckUserResult
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public List<string> Roles { get; set; }
        // For schedule a re-login prompt, since there is no refresh token.
        public DateTime ExpiresAt { get; set; }
    }
}
