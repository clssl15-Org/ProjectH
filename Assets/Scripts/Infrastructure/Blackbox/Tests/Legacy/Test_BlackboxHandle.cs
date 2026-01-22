using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace BlackboxSystem.Tests
{
    public class Test_BlackboxHandle
    {
        private const string Message = "Hello, Happy World!";

        [SetUp]
        public void SetUp()
        {
            Blackbox.ForceResetStaticProperties();
            Infrastructure.StrongReference = false;
        }

        private Blackbox GetBlackbox(BlackboxHandle handle)
        {
            var handleType = typeof(BlackboxHandle);
            var fieldInfo =
                handleType.GetField("_blackbox", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new Exception("Field '_blackbox' not found. Make sure the field name has been changed.");

            return (Blackbox)fieldInfo.GetValue(handle);
        }


        #region Of
        [TestCase(true), TestCase(false)]
        public void Of_ValidSubject_Idle(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);
            
            // Act
            var handle = BlackboxHandle.Of(owner);

            // Assert
            Asserts.AssertBlackbox(GetBlackbox(handle), owner, ownerName, 0);
            Assert.That(BlackboxRegistry.Contains(owner), Is.True);
        }
        [TestCase(true), TestCase(false)]
        public void Of_ValidSubject_Printed(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;
            Assert.That(() => new Blackbox(new object(), true).TryPrint(-1, out _), Throws.Nothing); // Assure print execution

            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);

            // Act
            var handle = BlackboxHandle.Of(owner);

            // Assert
            Asserts.AssertBlackbox(GetBlackbox(handle), owner, ownerName, 1);
            Assert.That(BlackboxRegistry.Contains(owner), Is.True);
        }

        [TestCase(true), TestCase(false)]
        public void Of_NullSubject_Idle(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            // Act & Assert
            Assert.That(() => BlackboxHandle.Of(null), Throws.ArgumentNullException);
        }
        [TestCase(true), TestCase(false)]
        public void Of_NullSubject_Printed(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;
            Assert.That(() => new Blackbox(new object(), true).TryPrint(-1, out _), Throws.Nothing); // Assure print execution

            // Act & Assert
            Assert.That(() => BlackboxHandle.Of(null), Throws.ArgumentNullException);
        }
        #endregion


        #region Write
        [TestCase(true), TestCase(false)]
        public void Write_Null(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;
            var handle = new BlackboxHandle();

            // Act
            var result = handle.Write(Message);

            // Assert
            Assert.That(result, Is.EqualTo(Message));
            Assert.That(GetBlackbox(handle), Is.Null);
        }
        [TestCase(true), TestCase(false)]
        public void Write_Idle(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);
            var handle = BlackboxHandle.Of(owner);

            // Act
            var result = handle.Write(Message);

            // Assert
            Assert.That(result, Is.EqualTo(Message));
            Asserts.AssertBlackbox(GetBlackbox(handle), owner, ownerName, 0);
        }
        [TestCase(true), TestCase(false)]
        public void Write_Printed(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;
            Assert.That(() => new Blackbox(new object(), true).TryPrint(-1, out _), Throws.Nothing); // Assure print execution

            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);
            var handle = BlackboxHandle.Of(owner);

            // Act
            var result = handle.Write(Message);

            // Assert
            Assert.That(result, Is.EqualTo(Message));
            Asserts.AssertBlackbox(GetBlackbox(handle), owner, ownerName, 1);
        }
        #endregion


        #region Exert (Exerted)
        [TestCase(true, true), TestCase(true, false)]
        [TestCase(false, true), TestCase(false, false)]
        public void Exert_Null_Self(bool exerted, bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);
            var handle = BlackboxHandle.Of(owner);

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);

            // Act
            var message = exerted
                ? handle.Exert(peerOwner, Message)
                : handle.Exerted(peerOwner, Message);

            // Assert
            Assert.That(message, Is.EqualTo(Message));
            Asserts.AssertBlackbox(GetBlackbox(BlackboxHandle.Of(owner)), owner, ownerName, 0);
            Asserts.AssertBlackbox(GetBlackbox(BlackboxHandle.Of(peerOwner)), peerOwner, peerName, 1);

            Assert.That(GetBlackbox(handle).TryPrint(-1, out var result), Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
            Assert.That(result, Does.Contain("Depth = 1"));
        }
        [TestCase(true, true), TestCase(true, false)]
        [TestCase(false, true), TestCase(false, false)]
        public void Exert_Null_Other(bool exerted, bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);

            var peerName = "Peer";
            var peerOwner = new NamedOwner(peerName);
            var peerHandle = BlackboxHandle.Of(peerOwner);

            // Act
            var message = exerted
                ? peerHandle.Exert(owner, Message)
                : peerHandle.Exerted(owner, Message);

            // Assert
            Assert.That(message, Is.EqualTo(Message));
            Asserts.AssertBlackbox(GetBlackbox(BlackboxHandle.Of(owner)), owner, ownerName, 1);
            Asserts.AssertBlackbox(GetBlackbox(BlackboxHandle.Of(peerOwner)), peerOwner, peerName, 0);

            Assert.That(GetBlackbox(peerHandle).TryPrint(-1, out var result), Is.True);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
            Assert.That(result, Does.Contain("Depth = 0"));
            Assert.That(result, Does.Contain("Depth = 1"));
        }
        #endregion
    }
}
