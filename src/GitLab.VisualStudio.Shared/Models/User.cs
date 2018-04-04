namespace GitLab.VisualStudio.Shared.Models
{
    public class User
    {
        public static implicit operator User(NGitLab.Models.Session session)
        {
            return (NGitLab.Models.User)session;
        }

        public static implicit operator User(NGitLab.Models.User session)
        {
            if (session != null)
            {
                return new User()
                {
                    AvatarUrl = session.AvatarUrl,
                    Email = session.Email,
                    Id = session.Id,
                    Name = session.Name,
                    TwoFactorEnabled = session.TwoFactorEnabled,
                    Username = session.Username,
                };
            }
            else
            {
                return null;
            }
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string PrivateToken { get; set; }
        public string Host { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public ApiVersion ApiVersion { get; set; }
    }
}