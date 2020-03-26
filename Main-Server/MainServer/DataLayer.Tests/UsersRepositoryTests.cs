using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataLayer.ORM;
using DataLayer.Models;
using Moq;
using System.Data.SqlClient;
using DataLayer.Repositories;
using System.Linq;
using System.Security.Cryptography;

namespace DataLayer.Tests
{
    /// <summary>
    /// Tests for the users repository
    /// </summary>
    [TestClass]
    public class UsersRepositoryTests
    {
        public IDb<User> Db { get; set; }
        public List<User> UsersList { get; set; }

        [TestInitialize()]
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

        #region Select Tests

        [TestMethod]
        public void ReturnUsersFromRepository_NoSearchCondition_ReturnsTwoUsers()
        {
            var usersDbMock = new Mock<IDb<User>>();


            usersDbMock.Setup(db =>
                db.Select(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
            .Returns(UsersList);

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var users = usersRepository.Select();

            Assert.AreEqual(users.Count(), 3);
            Assert.AreEqual(users.ElementAt(0).Id, UsersList[0].Id);
            Assert.AreEqual(users.ElementAt(1).Id, UsersList[1].Id);
            Assert.AreEqual(users.ElementAt(2).Id, UsersList[2].Id);
        }

        [TestMethod]
        public void ReturnUsersFromRepository_IdEquals1_ReturnsFirstUser()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var index = 0;
            var userToTest = UsersList[index];

            var parameters = new SqlParameter[] { new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = userToTest.Id } };

            usersDbMock.Setup(db =>
                db.Select(It.IsInRange(1, int.MaxValue, Range.Inclusive), "WHERE Id = @Id", parameters))
            .Returns(new List<User> { userToTest });

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var users = usersRepository.Select(1, "WHERE Id = @Id", parameters);

            Assert.AreEqual(1, users.Count());
            Assert.AreEqual(userToTest.Id, users.ElementAt(index).Id);
        }

        [TestMethod]
        public void ReturnUsersFromRepository_IdEquals3_ReturnsLastUser()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var index = 2;
            var userToTest = UsersList[index];

            var parameters = new SqlParameter[] { new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = userToTest.Id } };

            usersDbMock.Setup(db =>
                db.Select(It.IsInRange(1, int.MaxValue, Range.Inclusive), "WHERE Id = @Id", parameters))
            .Returns(new List<User> { userToTest });

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var users = usersRepository.Select(1, "WHERE Id = @Id", parameters);

            Assert.AreEqual(1, users.Count());
            Assert.AreEqual(userToTest.Id, users.ElementAt(0).Id);
        }

        [TestMethod]
        public void ReturnUsersFromRepository_IdEquals4_ReturnsEmptyList()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var parameters = new SqlParameter[] { new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = 3 } };

            usersDbMock.Setup(db =>
                db.Select(It.IsInRange(1, int.MaxValue, Range.Inclusive), "WHERE Id = @Id", parameters))
            .Returns(new List<User>());

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var users = usersRepository.Select(1, "WHERE Id = @Id", parameters);

            Assert.AreEqual(0, users.Count());
        }

        #endregion

        #region Insert Tests

        [TestMethod]
        public void InsertUserToDB_3UsersInDb_InsertsUserAndSetsItsIdTo4()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var list2 = new List<User>();
            list2.AddRange(UsersList);

            usersDbMock.Setup(db =>
                db.Insert(It.IsAny<User>())).Callback((User added) =>
                {
                    list2.Add(added);
                    added.Id = 3;
                });
            
            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user = new User { Name = "NewUser" };
            usersRepository.Insert(user);

            Assert.AreEqual(list2.Count(), 4);
            Assert.AreEqual(list2.ElementAt(3).Id, user.Id);
            Assert.AreEqual(list2.ElementAt(3).Name, user.Name);
        }

        [TestMethod]
        public void InsertUserToDB_3UsersInDb_InsertsExistingUserThrowException()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var list2 = new List<User>();
            list2.AddRange(UsersList);

            usersDbMock.Setup(db =>
                db.Insert(It.IsAny<User>())).Callback(() => 
                {
                    throw new System.Exception("User exists already");
                });

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user = new User { Name = "NewUser" };
            var exception = Assert.ThrowsException<System.Exception>(() => usersRepository.Insert(user));

            Assert.AreEqual(exception.Message, "User exists already");
        }

        #endregion

        [TestMethod]
        public void SelectSpecificUser_UserIsInDatabase_ReturnsUser()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var userToTest = UsersList[2];
            var username = userToTest.Username;
            var parameterCheck = false;

            usersDbMock.Setup(db =>
                db.Select(It.IsAny<int>(), "WHERE Username = @Username", It.IsAny<SqlParameter[]>()))
            .Callback((int count, string sql, SqlParameter[] parameters) =>
            {
                if (parameters.Length == 1 &&
                    parameters[0].ParameterName == "@Username" &&
                    parameters[0].Value as string == username)
                    parameterCheck = true;
                else
                    parameterCheck = false;
            }).Returns(new List<User>() { userToTest });

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user = usersRepository.SelectSpecificUser(username);
            
            Assert.AreNotEqual(user, null);
            Assert.AreEqual(parameterCheck, true);
            Assert.AreEqual(user.Id, userToTest.Id);
        }

        [TestMethod]
        public void SelectSpecificUser_UserIsNotInDatabase_ReturnsNull()
        {
            var usersDbMock = new Mock<IDb<User>>();
            var username = "doesntexist123";
            var parameterCheck = false;

            usersDbMock.Setup(db =>
                db.Select(It.IsAny<int>(), "WHERE Username = @Username", It.IsAny<SqlParameter[]>()))
            .Callback((int count, string sql, SqlParameter[] parameters) =>
            {
                if (parameters.Length == 1 &&
                    parameters[0].ParameterName == "@Username" &&
                    parameters[0].Value as string == username)
                    parameterCheck = true;
                else
                    parameterCheck = false;
            }).Returns(new List<User>());

            var usersRepository = new UsersRepository(usersDbMock.Object);
            var user = usersRepository.SelectSpecificUser(username);

            Assert.AreEqual(parameterCheck, true);
            Assert.AreEqual(user, null);
        }
    }
}
