using NUnit.Framework;

namespace BlackboxSystem.Tests
{
    internal static class Asserts
    {
        public static void AssertBlackbox(Blackbox blackbox, object owner, object ownerString, long id)
        {
            Assert.That(blackbox, Is.Not.Null, "Target blackbox object is null.");

            Assert.That(blackbox.Owner, Is.SameAs(owner), "Owner reference does not match.");
            if (owner != null) Assert.That(blackbox.OwnerString, Is.EqualTo(ownerString), "OwnerString value does not match.");
            Assert.That(blackbox.Id, Is.EqualTo(id), "Id value does not match.");
        }
    }
}
