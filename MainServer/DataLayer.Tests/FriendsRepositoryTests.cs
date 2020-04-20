using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataLayer.Tests
{
    [TestClass]
    public class FriendsRepositoryTests
    {
        #region CreateFriend

        [TestMethod]
        public void CreateFriend_Success_ReturnsFriend()
        {
            var dbMock = new Mock<IDb<Friend>>();

            dbMock.Setup(db => db.Insert(It.IsAny<Friend>()))
                .Callback((Friend f) => { f.Id = 0; })
                .Verifiable();
            var friendsRepository = new FriendsRepository(dbMock.Object);

            var friend = friendsRepository.CreateFriend(0, 1);
            Assert.IsNotNull(friend);
            Assert.AreEqual(0, friend.Id);
            dbMock.Verify();
        }

        [TestMethod]
        public void CreateFriend_Failed_ReturnsNullDoesntThrow()
        {
            var dbMock = new Mock<IDb<Friend>>();

            dbMock.Setup(db => db.Insert(It.IsAny<Friend>()))
                .Throws<Exception>()
                .Verifiable();
            var friendsRepository = new FriendsRepository(dbMock.Object);

            var friend = friendsRepository.CreateFriend(0, 1);
            Assert.IsNull(friend);
            dbMock.Verify();
        }

        #endregion

        #region GetFriendsOfUser

        [TestMethod]
        public void GetFriendsOfUser_SuccessFriends_ReturnsList()
        {
            var dbMock = new Mock<IDb<Friend>>();

            dbMock.Setup(db => db.Select(1, "WHERE userId1 = @userId OR userId2 = @userId", It.IsAny<SqlParameter[]>()))
                .Returns(new List<Friend>() { new Friend(), new Friend() })
                .Verifiable();
            var friendsRepository = new FriendsRepository(dbMock.Object);

            var friends = friendsRepository.GetFriendsOfUserByStatus(0);
            Assert.IsNotNull(friends);
            Assert.AreEqual(2, friends.Count());
            dbMock.Verify();
        }

        [TestMethod]
        public void GetFriendsOfUser_SuccessNoFriends_ReturnsEmptyList()
        {
            var dbMock = new Mock<IDb<Friend>>();

            dbMock.Setup(db => db.Select(1, "WHERE userId1 = @userId OR userId2 = @userId", It.IsAny<SqlParameter[]>()))
                .Returns(default(List<Friend>))
                .Verifiable();
            var friendsRepository = new FriendsRepository(dbMock.Object);

            var friends = friendsRepository.GetFriendsOfUserByStatus(0);
            Assert.IsNotNull(friends);
            Assert.AreEqual(0, friends.Count());
            dbMock.Verify();
        }

        #endregion
    }
}
