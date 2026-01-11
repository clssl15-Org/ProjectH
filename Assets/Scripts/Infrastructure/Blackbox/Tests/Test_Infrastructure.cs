using System;
using NUnit.Framework;

namespace BlackboxSystem.Tests
{
    public class Test_Infrastructure
    {
        [TearDown]
        public void TearDown()
        {
            Infrastructure.Logger = null;
        }


        [Test]
        public void Log_NullLogger()
        {
            // Arrange
            var message = "Hello, Happy World!";

            // Act
            var succeed = Infrastructure.Log(message);

            // Assert
            Assert.That(succeed, Is.False);
        }

        [Test]
        public void Log_ValidLogger()
        {
            // Arrange
            var message = "Hello, Happy World!";
            var recorded = string.Empty;

            var logger = (Action<string>)(msg => recorded = msg);
            Infrastructure.Logger = logger;

            // Act
            var succeed = Infrastructure.Log(message);

            // Assert
            Assert.That(succeed, Is.True);
            Assert.That(recorded, Is.EqualTo(message));
        }
    }
}
