using System;
using NUnit.Framework;
using UnityEngine;
using static BlackboxSystem.Tests.Asserts;

namespace BlackboxSystem.Tests
{
    public class Test_Blackbox
    {
        private const string Message = "Hello, Happy World!";

        [SetUp]
        public void SetUp()
        {
            Blackbox.ForceResetStaticProperties();
        }

        private static Blackbox CreateWeakBlackbox(string ownerName, out NamedOwner owner)
        {
            owner = new NamedOwner(ownerName);
            var blackbox = new Blackbox(owner, false);

            return blackbox;
        }


        #region Create
        [TestCase(true), TestCase(false)]
        public void Create_Valid(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);

            // Act
            var blackbox = new Blackbox(owner, strongReference);

            // Assert
            AssertBlackbox(blackbox, owner, name, 0);
        }
        [TestCase(true), TestCase(false)]
        public void Create_Invalid(bool strongReference)
        {
            // Act & Assert
            Assert.That(() => new Blackbox(null, strongReference), Throws.ArgumentNullException);
        }
        #endregion


        #region Write
        [TestCase(true), TestCase(false)]
        public void Write_ValidMessage(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            // Act
            var message = blackbox.Write(Message);

            // Assert
            AssertBlackbox(blackbox, owner, name, 0);
            Assert.That(message, Is.EqualTo(Message));

            var success = blackbox.TryPrint(-1, out var result);
            Assert.That(success, Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
        }
        [Test]
        public void Write_ValidMessage_OwnerLost()
        {
            // Arrange
            var blackbox = CreateWeakBlackbox("Owner", out var owner);
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            Assert.That(blackbox.Owner, Is.Null); // Assure owner lost
            
            // Act
            var message = blackbox.Write(Message);

            // Assert
            AssertBlackbox(blackbox, null, default, 0);
            Assert.That(message, Is.EqualTo(Message));

            var success = blackbox.TryPrint(-1, out var result);
            Assert.That(success, Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
        }
        [TestCase(true), TestCase(false)]
        public void Write_ValidMessage_Printed(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);
            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(() => blackbox.Write(Message), Throws.Nothing);
        }


        [TestCase(true), TestCase(false)]
        public void Write_InvalidMessage_Null(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            // Act & Assert
            Assert.That(() => blackbox.Write(null), Throws.ArgumentException);
        }
        [TestCase(true), TestCase(false)]
        public void Write_InvalidMessage_Empty(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            // Act & Assert
            Assert.That(() => blackbox.Write(string.Empty), Throws.ArgumentException);
        }
        #endregion


        #region Exert
        [TestCase(true, true), TestCase(true, false)]
        [TestCase(false, true), TestCase(false, false)]
        public void Exert_Valid(bool strongReference_owner, bool strongReference_peer)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference_owner);

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerBlackbox = new Blackbox(peerOwner, strongReference_peer);

            // Act
            var message = blackbox.Exert(peerBlackbox, Message);
            Assert.That(message, Is.EqualTo(Message));

            // Assert
            AssertBlackbox(blackbox, owner, name, 0);
            AssertBlackbox(peerBlackbox, peerOwner, peerName, 1);

            var success = blackbox.TryPrint(-1, out var result);
            Assert.That(success, Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
            Assert.That(result, Does.Contain("Depth = 1"));
        }
        [TestCase(true), TestCase(false)]
        public void Exert_Valid_OwnerLost(bool strongReference_other)
        {
            // Arrange
            var blackbox = CreateWeakBlackbox("Owner", out var owner);
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            Assert.That(blackbox.Owner, Is.Null); // Assure owner lost

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerBlackbox = new Blackbox(peerOwner, strongReference_other);

            // Act
            var message = blackbox.Exert(peerBlackbox, Message);
            Assert.That(message, Is.EqualTo(Message));

            // Assert
            AssertBlackbox(blackbox, owner, default, 0);
            AssertBlackbox(peerBlackbox, peerOwner, peerName, 1);

            var success = blackbox.TryPrint(-1, out var result);
            Assert.That(success, Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
            Assert.That(result, Does.Contain("Depth = 1"));
        }
        [TestCase(true, true), TestCase(true, false)]
        [TestCase(false, true), TestCase(false, false)]
        public void Exert_Valid_Printed(bool strongReference_owner, bool strongReference_peer)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference_owner);

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerBlackbox = new Blackbox(peerOwner, strongReference_peer);

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(() => blackbox.Exert(blackbox, Message), Throws.Nothing);
        }


        [TestCase(true), TestCase(false)]
        public void Exert_Self(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            // Act
            var message = blackbox.Exert(blackbox, Message);
            Assert.That(message, Is.EqualTo(Message));

            // Assert
            AssertBlackbox(blackbox, owner, name, 0);

            var success = blackbox.TryPrint(-1, out var result);
            Assert.That(success, Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
            Assert.That(result, Does.Not.Contain("Depth = 1"));
        }
        [Test]
        public void Exert_Self_OwnerLost()
        {
            // Arrange
            var blackbox = CreateWeakBlackbox("Owner", out var owner);
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            Assert.That(blackbox.Owner, Is.Null); // Assure owner lost

            // Act
            var message = blackbox.Exert(blackbox, Message);
            Assert.That(message, Is.EqualTo(Message));

            // Assert
            AssertBlackbox(blackbox, owner, default, 0);

            var success = blackbox.TryPrint(-1, out var result);
            Assert.That(success, Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
            Assert.That(result, Does.Not.Contain("Depth = 1"));
        }
        [TestCase(true), TestCase(false)]
        public void Exert_Self_Printed(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(() => blackbox.Exert(blackbox, Message), Throws.Nothing);
        }


        [TestCase(true), TestCase(false)]
        public void Exert_InvalidOther(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            // Act & Assert
            Assert.That(() => blackbox.Exert(null, Message), Throws.ArgumentNullException);
        }
        [Test]
        public void Exert_InvalidOther_OwnerLost()
        {
            // Arrange
            var blackbox = CreateWeakBlackbox("Owner", out var owner);
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            Assert.That(blackbox.Owner, Is.Null); // Assure owner lost

            // Act & Assert
            Assert.That(() => blackbox.Exert(null, Message), Throws.ArgumentNullException);
        }
        [TestCase(true), TestCase(false)]
        public void Exert_InvalidOther_Printed(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(() => blackbox.Exert(null, Message), Throws.ArgumentNullException);
        }


        [TestCase(true, true), TestCase(true, false)]
        [TestCase(false, true), TestCase(false, false)]
        public void Exert_InvalidMessage(bool strongReference_owner, bool strongReference_peer)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference_owner);

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerBlackbox = new Blackbox(peerOwner, strongReference_peer);

            // Act & Assert
            Assert.That(() => blackbox.Exert(peerBlackbox, null), Throws.ArgumentException);
        }
        [TestCase(true), TestCase(false)]
        public void Exert_InvalidMessage_OwnerLost(bool strongReference_other)
        {
            // Arrange
            var blackbox = CreateWeakBlackbox("Owner", out var owner);
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            Assert.That(blackbox.Owner, Is.Null); // Assure owner lost

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerBlackbox = new Blackbox(peerOwner, strongReference_other);

            // Act & Assert
            Assert.That(() => blackbox.Exert(peerBlackbox, null), Throws.ArgumentException);
        }
        [TestCase(true, true), TestCase(true, false)]
        [TestCase(false, true), TestCase(false, false)]
        public void Exert_InvalidMessage_Printed(bool strongReference_owner, bool strongReference_peer)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference_owner);

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerBlackbox = new Blackbox(peerOwner, strongReference_peer);

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(() => blackbox.Exert(peerBlackbox, null), Throws.ArgumentException);
        }
        #endregion


        #region Try Print
        [TestCase(true), TestCase(false)]
        public void TryPrint(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(blackbox.TryPrint(-1, out _), Is.False);
            AssertBlackbox(blackbox, owner, name, 0);
        }
        [Test]
        public void TryPrint_OwnerLost()
        {
            // Arrange
            var blackbox = CreateWeakBlackbox("Owner", out var owner);
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            Assert.That(blackbox.Owner, Is.Null); // Assure owner lost

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(blackbox.TryPrint(-1, out _), Is.False);
            AssertBlackbox(blackbox, owner, default, 0);
        }
        [TestCase(true), TestCase(false)]
        public void TryPrint_Printed(bool strongReference)
        {
            // Arrange
            var name = "Owner";
            var owner = new NamedOwner(name);
            var blackbox = new Blackbox(owner, strongReference);

            Assert.That(blackbox.TryPrint(-1, out _), Is.True); // Assure print execution

            // Act & Assert
            Assert.That(blackbox.TryPrint(-1, out _), Is.False);
            AssertBlackbox(blackbox, owner, name, 0);
        }
        #endregion
    }
}
