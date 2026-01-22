using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BlackboxSystem.Tests
{
    // AI Generated Test
    public class Test_Integration
    {
        private const bool OpenLog = false;

        [SetUp]
        public void SetUp()
        {
            BlackboxHandle.ForceReset();
            BlackboxHandle.NormalLogger = Debug.Log;
            BlackboxHandle.WarningLogger = Debug.LogWarning;

            // Reference Lost(Owner WeakReference) 시나리오를 위해 필요
            Infrastructure.StrongReference = false;

            // 로그 컷오프에 걸리지 않도록 넉넉히
            Infrastructure.MaxLogCount = 200;
        }

        [TearDown]
        public void TearDown()
        {
            BlackboxHandle.NormalLogger = null;
            BlackboxHandle.WarningLogger = null;
        }


        [Test]
        public void Integration_4Owners_Interactions_ReferenceLost_ExportSingleResult()
        {
            // Arrange (A, D는 끝까지 살아있게 유지)
            var aOwner = new NamedOwner("A");
            var dOwner = new NamedOwner("D");

            var a = BlackboxHandle.Of(aOwner);
            var d = BlackboxHandle.Of(dOwner);

            // BLACKBOX 미정의(핸들 invalid)면 Owner 접근에서 예외가 납니다.
            Assert.DoesNotThrow(() => _ = a.Owner);

            a.Write("A: boot");

            // B, C는 메서드 스코프를 빠져나가며 참조를 잃도록 만든다.
            var (bOwnerWeak, cOwnerWeak) = CreateBAndC_AndInteract(a, d);

            // Act: GC로 B/C owner를 날려서 (Reference Lost) 상태 유도
            ForceGcUntilCollected(bOwnerWeak, cOwnerWeak);

            // 최종 출력은 A에서 1회 Export
            var path = OpenLog ? Application.persistentDataPath : Path.GetTempPath();
            var dir = Path.Combine(path, "BlackboxIntegrationTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);

            BlackboxHandle.LogDirectory = dir;
            a.Export(recursionDepth: 3, openLog: OpenLog);

            // Assert: Export 파일 1개 생성 + 내용 검증
            var files = Directory.GetFiles(dir, "*.txt");
            Assert.That(files.Length, Is.EqualTo(1), "Export should create exactly one log file.");

            var text = File.ReadAllText(files[0]);
            Debug.Log(text);

            // 4개 섹션이 모두 포함되는지
            Assert.That(text, Does.Contain("A".ToTitle()));
            Assert.That(text, Does.Contain("D".ToTitle()));

            // 중간에 Reference Lost가 실제로 반영되는지 (헤더/peer 표기 어디든 등장하면 OK)
            Assert.That(text, Does.Contain("B (Reference Lost)"));
            Assert.That(text, Does.Contain("C (Reference Lost)"));

            // 상호작용 메시지들이 최종 출력에 남는지
            Assert.That(text, Does.Contain("A->B ping"));
            Assert.That(text, Does.Contain("B->C forward"));
            Assert.That(text, Does.Contain("C->D handoff"));
            Assert.That(text, Does.Contain("D->A ack"));

            // Cleanup (원하시면 유지해도 됩니다)
            if (!OpenLog)
            {
#pragma warning disable CS0162
                try { Directory.Delete(dir, recursive: true); }
                catch { /* ignore */ }
#pragma warning restore
            }
        }

        private static (WeakReference bOwnerWeak, WeakReference cOwnerWeak) CreateBAndC_AndInteract(
            BlackboxHandle a,
            BlackboxHandle d)
        {
            var bOwner = new NamedOwner("B");
            var cOwner = new NamedOwner("C");

            var b = BlackboxHandle.Of(bOwner);
            var c = BlackboxHandle.Of(cOwner);

            var bOwnerWeak = new WeakReference(bOwner);
            var cOwnerWeak = new WeakReference(cOwner);

            // 4자 상호작용(체인 + 루프)
            a.Exert(bOwner, "A->B ping");
            b.Write("B: got ping");

            b.Exert(cOwner, "B->C forward");
            c.Write("C: got forward");

            c.Exert(d.Owner, "C->D handoff");
            d.Write("D: received handoff");

            d.Exert(a.Owner, "D->A ack");
            a.Write("A: received ack");

            return (bOwnerWeak, cOwnerWeak);
        }

        private static void ForceGcUntilCollected(WeakReference bOwnerWeak, WeakReference cOwnerWeak)
        {
            // GC는 환경에 따라 즉시 수거가 안 될 수 있어 약간의 리트라이를 둡니다.
            for (int i = 0; i < 8; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                if (!bOwnerWeak.IsAlive && !cOwnerWeak.IsAlive)
                    return;

                // 약간의 메모리 압박(선택)
                _ = new byte[1024 * 128];
            }

            Assert.That(bOwnerWeak.IsAlive, Is.False, "B owner should be collected (Reference Lost).");
            Assert.That(cOwnerWeak.IsAlive, Is.False, "C owner should be collected (Reference Lost).");
        }
    }
}
