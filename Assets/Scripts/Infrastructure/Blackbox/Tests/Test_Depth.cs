using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BlackboxSystem.Tests
{
    public class Test_Depth
    {
        private const bool OpenLog = true;

        [SetUp]
        public void SetUp()
        {
            BlackboxHandle.ForceReset();
            BlackboxHandle.NormalLogger = Debug.Log;
            BlackboxHandle.WarningLogger = Debug.LogWarning;

            Infrastructure.StrongReference = false;
            Infrastructure.MaxLogCount = 200;
        }

        [TearDown]
        public void TearDown()
        {
            BlackboxHandle.NormalLogger = null;
            BlackboxHandle.WarningLogger = null;
        }


        private class AClass
        {
            public BClass BClassInstance { get; private set; }

            public AClass()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Creating AClass");
                BClassInstance = new BClass();
            }

            public void A_Simple()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Do A_Simple");
                BlackboxHandle.Of(this).Write("Did A_Simple");
            }

            public void A_Public()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Entering A_Public");
                A_Private();
                BlackboxHandle.Of(this).Write("Did A_Public");
            }

            private void A_Private()
            {
                using var _ = BlackboxHandle.Of(this).ExertScope(BClassInstance, "Entering A_Private");
                BClassInstance.B_Public(this);
                BlackboxHandle.Of(this).Write("Did A_Private");
            }
        }

        private class BClass
        {
            public void B_Public(AClass a)
            {
                using var _ = BlackboxHandle.Of(this).ExertedScope(a, "B_Public");
                B_Private();
                B_Private();
                BlackboxHandle.Of(this).Write("Did B_Public");
            }

            private void B_Private()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Entering B_Private");
                BlackboxHandle.Of(this).Write("Do/Did B_Private");
            }
        }


        [Test]
        public void Depth()
        {
            // Arrange
            var aClass = new AClass();

            // Act
            aClass.A_Simple();
            aClass.A_Public();

            // Assert
            var path = OpenLog ? Application.persistentDataPath : Path.GetTempPath();
            var dir = Path.Combine(path, "BlackboxDepthTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);

            BlackboxHandle.LogDirectory = dir;
            BlackboxHandle.Of(aClass).Export(
                format: ExportFormat.Html,
                fullExportOption: FullExportOption.Full,
                openLogOption: OpenLog ? OpenLogOption.Open : OpenLogOption.Never);

            if (!OpenLog)
            {
#pragma warning disable CS0162
                try { Directory.Delete(dir, recursive: true); }
                catch { /* ignore */ }
#pragma warning restore
            }
        }
    }
}
