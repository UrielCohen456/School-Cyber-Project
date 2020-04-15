using System.Runtime.Serialization;

namespace DataLayer
{
    [DataContract]
    public enum FriendStatus : byte
    {
        [EnumMember]
        Waiting = 0,

        [EnumMember]
        Accepted,

        [EnumMember]
        Denied,

        [EnumMember]
        Removed
    }

    public static class FriendStatusExtensions
    {
        public static bool IsChangePossible(this FriendStatus status, FriendStatus destStatus)
        {
            if (status == FriendStatus.Waiting &&
                (destStatus == FriendStatus.Accepted || destStatus == FriendStatus.Denied))
                return true;

            if (status == FriendStatus.Accepted && destStatus == FriendStatus.Removed)
                return true;

            // No possible changes so returning false
            return false;
        }
    }
}
