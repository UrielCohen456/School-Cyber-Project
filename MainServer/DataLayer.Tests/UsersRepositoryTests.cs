using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DataLayer.Tests
{
    [TestClass]
    public class UsersRepositoryTests
    {
        public List<User> UsersList { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            var user1 = new User { Id = 1, Name = "Uriel", Username = "uriel123" };
            var user2 = new User { Id = 2, Name = "Idan", Username = "idan123" };
            var user3 = new User { Id = 3, Name = "Idan", Username = "idan12" };

            UsersList = new List<User>()
            {
                user1,
                user2,
                user3,
            };
        }

        #region SelectByUsername

        [TestMethod]
        public void SelectByUsername_UserDoesntExist_ReturnsNull()
        {
            var usersDbMock = new Mock<IDb<User>>();

            usersDbMock.Setup(db =>
                db.Select(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .Returns(default(IEnumerable<User>));

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user1 = usersRepository.SelectSpecificUser("");
            var user2 = usersRepository.SelectSpecificUser(UsersList[0].Username);

            Assert.AreEqual(user1, null);
            Assert.AreEqual(user2, null);
        }

        [TestMethod]
        public void SelectByUsername_UserExists_ReturnsUser()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var userToSearch = UsersList[0];

            usersDbMock.Setup(db =>
                db.Select(1, It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .Callback((int count, string sql, SqlParameter[] parameters) =>
            {
                Assert.AreEqual(parameters[0].ParameterName, "@Username");
                Assert.AreEqual(parameters[0].Value, userToSearch.Username);
            })
            .Returns(new List<User> { userToSearch });

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user = usersRepository.SelectSpecificUser(userToSearch.Username);

            Assert.IsTrue(user != null);
            Assert.AreEqual(user.Id, userToSearch.Id);
            Assert.AreEqual(user.Username, userToSearch.Username);
            Assert.AreEqual(user.Name, userToSearch.Name);
        }

        #endregion

        #region SelectById

        [TestMethod]
        public void SelectById_UserDoesntExist_ReturnsNull()
        {
            var usersDbMock = new Mock<IDb<User>>();

            usersDbMock.Setup(db =>
                db.Select(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .Returns(default(IEnumerable<User>));

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user1 = usersRepository.SelectSpecificUser(0);
            var user2 = usersRepository.SelectSpecificUser(1);

            Assert.AreEqual(user1, null);
            Assert.AreEqual(user2, null);
        }

        [TestMethod]
        public void SelectById_UserExists_ReturnsUser()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var userToSearch = UsersList[0];

            usersDbMock.Setup(db =>
                db.Select(1, It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .Callback((int count, string sql, SqlParameter[] parameters) =>
            {
                Assert.AreEqual(parameters[0].ParameterName, "@Id");
                Assert.AreEqual(parameters[0].Value, userToSearch.Id);
            })
            .Returns(new List<User> { userToSearch });

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user = usersRepository.SelectSpecificUser(userToSearch.Id);

            Assert.IsTrue(user != null);
            Assert.AreEqual(user.Id, userToSearch.Id);
            Assert.AreEqual(user.Username, userToSearch.Username);
            Assert.AreEqual(user.Name, userToSearch.Name);
        }

        #endregion

        #region RemoveUser

        [TestMethod]
        public void RemoveUser_UserDoesntExist_ReturnsFalse()
        {
            var usersDbMock = new Mock<IDb<User>>();

            usersDbMock.Setup(db =>
                db.Delete(It.IsAny<User>()))
                .Throws<Exception>()
                .Verifiable();

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var removed = usersRepository.RemoveUser(UsersList[0]);

            usersDbMock.Verify();
            Assert.IsFalse(removed);
        }

        [TestMethod]
        public void RemoveUser_UserExists_ReturnsTrue()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var existingUser = UsersList[0];

            usersDbMock.Setup(db =>
                db.Delete(existingUser))
                .Verifiable();

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var removed = usersRepository.RemoveUser(existingUser);

            Assert.IsTrue(removed);
            usersDbMock.Verify();
        }

        #endregion

        #region AddUser

        [TestMethod]
        public void AddUser_InvalidFields_ReturnsNull()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var insertIsCalled = false;

            usersDbMock.Setup(db =>
                db.Insert(It.IsAny<User>()))
                .Callback((User user) => insertIsCalled = true);

            var usersRepository = new UsersRepository(usersDbMock.Object);

            Assert.IsNull(usersRepository.AddUser(null, "username", "password"));
            Assert.IsNull(usersRepository.AddUser("name", null, "password"));
            Assert.IsNull(usersRepository.AddUser("name", "username", null));
            Assert.IsFalse(insertIsCalled);
        }

        [TestMethod]
        public void AddUser_AddWorks_ReturnsUser()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var userToAdd = UsersList[0];

            usersDbMock.Setup(db =>
                db.Insert(It.IsAny<User>()))
                .Callback((User u) => u.Id = 1)
                .Verifiable();

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var userAdded = usersRepository.AddUser(userToAdd.Name, userToAdd.Username, "password");

            Assert.IsNotNull(userAdded);
            Assert.AreEqual(1, userAdded.Id);
            Assert.AreEqual(userToAdd.Name, userAdded.Name);
            Assert.AreEqual(userToAdd.Username, userAdded.Username);
        }

        [TestMethod]
        public void AddUser_AddCrashes_ReturnsNull()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var userToAdd = UsersList[0];

            usersDbMock.Setup(db =>
                db.Insert(It.IsAny<User>()))
                .Throws<Exception>()
                .Verifiable();

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var userAdded = usersRepository.AddUser(userToAdd.Name, userToAdd.Username, "password");

            Assert.IsNull(userAdded);
        }

        #endregion        
    }
}
