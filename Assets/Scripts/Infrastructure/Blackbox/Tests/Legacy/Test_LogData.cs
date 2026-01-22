using System;
using NUnit.Framework;
using UnityEngine;

namespace BlackboxSystem.Tests
{
    public class Test_LogData
    {
        private const string Message = "Hello, Happy World!";
        private const double TimeTolerance = 1;

        [SetUp]
        public void SetUp()
        {
            Blackbox.ForceResetStaticProperties();
        }

        private void AssertLogData(LogData logData, string message, DateTime time, Blackbox peer, InteractionType interaction)
        {
            Assert.That(logData.Message, Is.EqualTo(message));
            Assert.That(logData.Time, Is.EqualTo(time).Within(TimeSpan.FromSeconds(TimeTolerance)));
            Assert.That(logData.InteractionPeer, Is.SameAs(peer));
            Assert.That(logData.Interaction, Is.EqualTo(interaction));
        }


        #region Create
        [Test]
        public void Create_Peerless()
        {
            // Act
            var logData = new LogData(default, default, default, Message);

            // Assert
            AssertLogData(logData, Message, DateTime.UtcNow, null, InteractionType.None);
        }
        [TestCase(0), TestCase(1), TestCase(2), TestCase(3)]
        public void Create_Peered(int interactionType)
        {
            // Arrange
            var peer = new Blackbox(new NamedOwner("Peer"), true);
            var interaction = (InteractionType)interactionType;

            // Act
            var logData = new LogData(default, default, Message, default, peer, interaction);

            // Assert
            AssertLogData(logData, Message, DateTime.UtcNow, peer, interaction);
        }
        #endregion


        #region To String
        [Test]
        public void ToString_Peerless()
        {
            // Arrange
            var logData = new LogData(default, default, default, Message);

            // Act
            var result = logData.ToString();

            // Assert
            AssertLogData(logData, Message, DateTime.UtcNow, null, InteractionType.None);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
        }
        [TestCase(true), TestCase(false)]
        public void ToString_Peered(bool exerting)
        {
            // Arrange
            var peerName = "Peer";
            var peer = new Blackbox(new NamedOwner(peerName), true);
            var interaction = exerting ? InteractionType.Exerting : InteractionType.Exerted;

            var logData = new LogData(default, default, Message, default, peer, interaction);

            // Act
            var result = logData.ToString();

            // Assert
            AssertLogData(logData, Message, DateTime.UtcNow, peer, interaction);

            Debug.Log(result);
            Assert.That(result, Does.Contain(Message));
        }
        #endregion
    }
}
