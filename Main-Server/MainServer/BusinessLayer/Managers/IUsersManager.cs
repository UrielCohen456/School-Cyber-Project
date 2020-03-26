using DataLayer.Models;

namespace DataLayer.Managers
{
    public interface IUsersManager
    {
        public int UserCount { get; }

        User Login(string username, string password);
        User Signup(string name, string username, string password);
        void Logout(User user);
        bool IsUserConnected(string username);
    }
}