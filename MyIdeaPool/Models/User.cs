using System;
namespace MyIdeaPool.Models
{
    public class User
    {
        public User()
        {
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
    }
}
