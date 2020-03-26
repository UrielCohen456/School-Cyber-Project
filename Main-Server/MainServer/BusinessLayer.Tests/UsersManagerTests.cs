using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BusinessLayer.Tests
{
    [TestClass]
    public class UsersManagerTests
    {
        public List<User> Users { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            var pass1 = User.HashPassword("uriel123");
            var user1 = new User { Id = 1, Name = "Uriel", Username = "uriel123", Password = pass1.Password, Salt = pass1.Salt };

            var pass2 = User.HashPassword("idan12");
            var user2 = new User { Id = 2, Name = "Idan", Username = "idan12", Password = pass2.Password, Salt = pass2.Salt };

            var pass3 = User.HashPassword("idan123");
            var user3 = new User { Id = 3, Name = "Idan", Username = "idan123", Password = pass3.Password, Salt = pass3.Salt };

            Users = new List<User>();
            Users.Add(user1);
            Users.Add(user2);
            Users.Add(user3);
        }


        #region Login Tests

        [TestMethod]
        public void LoginTests_UserConnectedAlready_ThrowException()
        {
            var userToAdd = Users[0];

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(userToAdd.Username))
                .Returns(userToAdd);

            var usersManager = new UsersManager(usersRepositoryMock.Object);
            usersManager.Login(userToAdd.Username, userToAdd.Username);

            // Check that threw connected exception
            Assert.ThrowsException<Exception>(
                () => usersManager.Login(userToAdd.Username, userToAdd.Username));
        }

        [TestMethod]
        public void LoginTests_UserIsntConnectedButWrongPassword_ThrowException()
        {
            var userToAdd = Users[0];

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(userToAdd.Username))
                .Returns(userToAdd);
            var usersManager = new UsersManager(usersRepositoryMock.Object);

            Assert.ThrowsException<Exception>(
                () => usersManager.Login(userToAdd.Username, ""));
            Assert.AreEqual(0, usersManager.UserCount);
        }

        [TestMethod]
        public void LoginTests_UserIsntConnectedRightPassword_ReturnsUserObject()
        {
            var userToAdd = Users[0];

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(userToAdd.Username))
                .Returns(userToAdd);

            var usersManager = new UsersManager(usersRepositoryMock.Object);

            var user = usersManager.Login(userToAdd.Username, userToAdd.Username);

            Assert.AreEqual(1, usersManager.UserCount);
            Assert.AreEqual(userToAdd.Id, user.Id);
            Assert.AreEqual(userToAdd.Username, user.Username);
            Assert.AreEqual(userToAdd.Name, user.Name);
        }

        [TestMethod]
        public void LoginTests_UserIsntConnectedButDoesntExist_ThrowsException()
        {
            var connectedUser = Users[0];
            var userToAdd = Users[1];

            var usersQueue = new Queue<User>();
            usersQueue.Enqueue(connectedUser);
            usersQueue.Enqueue(null);

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(It.IsAny<string>()))
                .Returns(usersQueue.Dequeue);

            var usersManager = new UsersManager(usersRepositoryMock.Object);
            var user = usersManager.Login(connectedUser.Username, connectedUser.Username);

            Assert.ThrowsException<Exception>(
               () => usersManager.Login(connectedUser.Username, ""));
            Assert.AreEqual(1, usersManager.UserCount);
        }

        #endregion

        #region Logout

        [TestMethod]
        public void SignupTests_UserConnected_ThrowsException()
        {
            var userToAdd = Users[0];

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(userToAdd.Username))
                .Returns(userToAdd);

            var usersManager = new UsersManager(usersRepositoryMock.Object);
            usersManager.Login(userToAdd.Username, userToAdd.Username);

            Assert.ThrowsException<Exception>(
                () => usersManager.Signup(userToAdd.Name, userToAdd.Username, userToAdd.Username));
        }

        [TestMethod]
        public void SignupTests_UserIsntConnectedAndExists_ThrowsException()
        {
            var userToAdd = Users[2];

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(userToAdd.Username))
                .Returns(userToAdd);

            var usersManager = new UsersManager(usersRepositoryMock.Object);

            Assert.ThrowsException<Exception>(
                () => usersManager.Signup(userToAdd.Name, userToAdd.Username, userToAdd.Username));
        }

        [TestMethod]
        public void SignupTests_UserIsntConnectedAndDoesntExists_CreatesUserSavesInDatabaseAndReturnsIt()
        {
            var pass = User.HashPassword("Tal123");
            var userToAdd = new User { Name = "Tal", Username = "Tal123", Password = pass.Password, Salt = pass.Salt };

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(It.IsAny<string>()))
                .Returns(default(User));
            usersRepositoryMock.Setup(r =>
                r.Insert(It.IsAny<User>()))
                .Callback((User user) =>
                {
                    user.Id = 4;
                    Assert.AreEqual(userToAdd.Username, user.Username);
                    Assert.AreEqual(userToAdd.Name, user.Name);
                    Assert.AreEqual(true, user.IsHashedPasswordCorrect(userToAdd.Username));
                });
                
            var usersManager = new UsersManager(usersRepositoryMock.Object);

            var userSigned = usersManager.Signup(userToAdd.Name, userToAdd.Username, userToAdd.Username);

            Assert.AreEqual(4, userSigned.Id);
            Assert.AreEqual(userToAdd.Username, userSigned.Username);
            Assert.AreEqual(userToAdd.Name, userSigned.Name);
            Assert.IsTrue(userSigned.IsHashedPasswordCorrect(userToAdd.Username));
        }

        #endregion

        #region Logout Tests

        [TestMethod]
        public void LogoutTests_UserIsntConnected_ThrowsException()
        {
            var userToDisconnect = new User();

            var usersRepositoryMock = new Mock<IUsersRepository>();
            var usersManager = new UsersManager(usersRepositoryMock.Object);

            Assert.ThrowsException<Exception>(
                () => usersManager.Logout(userToDisconnect));
        }

        [TestMethod]
        public void LogoutTests_UserIsConnected_RemovesUser()
        {
            var userToDisconnect = Users[0];

            var usersRepositoryMock = new Mock<IUsersRepository>();
            usersRepositoryMock.Setup(r =>
                r.SelectSpecificUser(userToDisconnect.Username))
                .Returns(userToDisconnect);
            
            var usersManager = new UsersManager(usersRepositoryMock.Object);
            
            usersManager.Login(userToDisconnect.Username, userToDisconnect.Username);
            Assert.AreEqual(1, usersManager.UserCount);

            usersManager.Logout(userToDisconnect);

            Assert.AreEqual(0, usersManager.UserCount);
            Assert.IsFalse(usersManager.IsUserConnected(Users[1].Username));
        }

        #endregion
    }
}
