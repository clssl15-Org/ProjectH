using System;
using System.Collections.Concurrent;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace BlackboxSystem.Tests
{
    // AI Generated Tests
    public class Test_Blackbox_Complex
    {
        private static readonly FieldInfo LogsField =
            typeof(Blackbox).GetField("_logs", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new Exception("Field '_logs' not found. Make sure the field name has been changed.");

        [SetUp]
        public void SetUp()
        {
            Blackbox.ForceResetStaticProperties();
            Infrastructure.MaxLogCount = 100;
            Infrastructure.StrongReference = false;
        }

        private static int GetLogCount(Blackbox blackbox)
        {
            var logs = (ConcurrentQueue<LogData>)LogsField.GetValue(blackbox);
            return logs.Count;
        }

        private static int CountOf(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token))
                return 0;

            int count = 0;
            int index = 0;

            while (true)
            {
                index = text.IndexOf(token, index, StringComparison.Ordinal);
                if (index < 0) break;

                count++;
                index += token.Length;
            }

            return count;
        }


        #region Print / Recursion Depth
        [Test]
        public void TryPrint_Depth0_DoesNotPrintPeerSections()
        {
            // Arrange
            var a = new Blackbox(new NamedOwner("A"), strongReference: true);
            var b = new Blackbox(new NamedOwner("B"), strongReference: true);

            a.Exert(b, "A->B");

            // Act
            Assert.That(a.TryPrint(recursionDepth: 0, out var result), Is.True);

            // Assert
            Debug.Log(result);

            Assert.That(result, Does.Contain("========= A"));
            Assert.That(result, Does.Not.Contain("========= B"));
        }

        [Test]
        public void TryPrint_Depth1_PrintsDirectPeerButNotSecondHop()
        {
            // Arrange
            var a = new Blackbox(new NamedOwner("A"), strongReference: true);
            var b = new Blackbox(new NamedOwner("B"), strongReference: true);
            var c = new Blackbox(new NamedOwner("C"), strongReference: true);

            a.Exert(b, "A->B");
            b.Exert(c, "B->C");

            // Act
            Assert.That(a.TryPrint(recursionDepth: 1, out var result), Is.True);

            // Assert
            Debug.Log(result);

            Assert.That(result, Does.Contain("========= A"));
            Assert.That(result, Does.Contain("========= B"));
            Assert.That(result, Does.Not.Contain("========= C"));

            // Nested section should include "From" marker.
            Assert.That(result, Does.Contain("From = #0: A"));
        }

        [Test]
        public void TryPrint_Depth2_PrintsSecondHop_AndWrapsImportedMessages()
        {
            // Arrange
            var a = new Blackbox(new NamedOwner("A"), strongReference: true);
            var b = new Blackbox(new NamedOwner("B"), strongReference: true);
            var c = new Blackbox(new NamedOwner("C"), strongReference: true);

            a.Exert(b, "A->B");
            b.Exert(c, "B->C");

            // Act
            Assert.That(a.TryPrint(recursionDepth: 2, out var result), Is.True);

            // Assert
            Debug.Log(result);

            Assert.That(result, Does.Contain("========= A"));
            Assert.That(result, Does.Contain("========= B"));
            Assert.That(result, Does.Contain("========= C"));

            // C section should be printed as depth 2 and should mention that it came from B.
            Assert.That(result, Does.Contain("Depth = 2 | From = #1: B"));
        }
        #endregion


        #region Print / History (Cycle)
        [Test]
        public void TryPrint_Cycle_DoesNotDuplicateSections()
        {
            // Arrange
            var a = new Blackbox(new NamedOwner("A"), strongReference: true);
            var b = new Blackbox(new NamedOwner("B"), strongReference: true);

            a.Exert(b, "A->B");
            b.Exert(a, "B->A");

            // Act
            Assert.That(a.TryPrint(recursionDepth: 10, out var result), Is.True);

            // Assert
            Debug.Log(result);

            // In a 2-node cycle, it should print exactly two sections (A and B) once each.
            Assert.That(CountOf(result, "========= A"), Is.EqualTo(1));
            Assert.That(CountOf(result, "========= B"), Is.EqualTo(1));
        }
        #endregion


        #region MaxLogCount
        [Test]
        public void MaxLogCount_EvictsOldestLogs()
        {
            // Arrange
            Infrastructure.MaxLogCount = 3;
            var a = new Blackbox(new NamedOwner("A"), strongReference: true);

            a.Write("MSG_001");
            a.Write("MSG_002");
            a.Write("MSG_003");
            a.Write("MSG_004");
            a.Write("MSG_005");

            // Act
            Assert.That(a.TryPrint(recursionDepth: 0, out var result), Is.True);

            // Assert
            Debug.Log(result);

            Assert.That(result, Does.Contain("MSG_003"));
            Assert.That(result, Does.Contain("MSG_004"));
            Assert.That(result, Does.Contain("MSG_005"));

            Assert.That(result, Does.Not.Contain("MSG_001"));
            Assert.That(result, Does.Not.Contain("MSG_002"));
            Assert.That(result, Does.Not.Contain("Created ("));
        }
        #endregion


        #region Global Printed
        [Test]
        public void TryPrint_IsGlobal_AndBlocksFurtherEnqueue()
        {
            // Arrange
            var a = new Blackbox(new NamedOwner("A"), strongReference: true);
            var b = new Blackbox(new NamedOwner("B"), strongReference: true);

            a.Write("A_1");
            b.Write("B_1");

            var aBefore = GetLogCount(a);
            var bBefore = GetLogCount(b);

            Assert.That(aBefore, Is.GreaterThanOrEqualTo(1));
            Assert.That(bBefore, Is.GreaterThanOrEqualTo(1));

            // Act
            Assert.That(a.TryPrint(recursionDepth: 0, out var result), Is.True);

            // Assert
            Debug.Log(result);

            // A's queue should be drained by printing.
            Assert.That(GetLogCount(a), Is.EqualTo(0));

            // B was not printed, so its queue should remain.
            Assert.That(GetLogCount(b), Is.EqualTo(bBefore));

            // After any print, logging is globally disabled.
            a.Write("A_2");
            b.Write("B_2");

            Assert.That(GetLogCount(a), Is.EqualTo(0));
            Assert.That(GetLogCount(b), Is.EqualTo(bBefore));

            // And other blackboxes cannot print either.
            Assert.That(b.TryPrint(recursionDepth: 0, out var otherResult), Is.False);
            Assert.That(otherResult, Is.EqualTo(string.Empty));
        }
        #endregion
    }
}
