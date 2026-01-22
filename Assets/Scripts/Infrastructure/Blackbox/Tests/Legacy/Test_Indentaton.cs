using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BlackboxSystem.Tests
{
    public class BlackboxScopeDepthTests
    {
        private const bool OpenLog = false;
        private string _dir = null;

        private sealed class Owner
        {
            private readonly string _name;
            public Owner(string name) => _name = name;
            public override string ToString() => _name;
        }

        [SetUp]
        public void SetUp()
        {
            BlackboxHandle.ForceReset(); // _printed 포함 초기화

            var path = OpenLog ? Application.persistentDataPath : Path.GetTempPath();
            _dir = Path.Combine(path, "BlackboxIntegrationTests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_dir);

            BlackboxHandle.Initialize(_dir, Debug.Log, strongReference: false);
            BlackboxHandle.MaxLogCount = 1000;
        }

        [TearDown]
        public void TearDown()
        {
            if (!OpenLog && Directory.Exists(_dir))
                Directory.Delete(_dir, recursive: true);
        }

        [Test]
        public void Print_IndentChanges_ByScopeDepth()
        {
            var owner = new Owner("Owner");
            var h = BlackboxHandle.Of(owner);

            using (h.WriteScope("outer", methodName: "Outer"))
            {
                h.Write("outer-write", methodName: "Outer");

                using (h.WriteScope("inner", methodName: "Inner"))
                {
                    h.Write("inner-write", methodName: "Inner");
                }

                h.Write("outer-after", methodName: "Outer");
            }

            h.Export(recursionDepth: 0, openLog: OpenLog);

            var file = Directory.GetFiles(_dir, "*.txt").Single();
            var text = File.ReadAllText(file);

            string FindLine(string needle) =>
                text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .First(l => l.Contains(needle));

            // LogData.ToString(): "[time] {indent}[MethodName] ..."
            // => time 뒤에 항상 공백 1개 + indent(ScopeDepth*2)가 들어갑니다.
            static int SpacesAfterTime(string line)
            {
                var timeEnd = line.IndexOf(']');             // time 닫는 ]
                var methodBracket = line.IndexOf('[', timeEnd + 1); // [MethodName] 시작 [
                return methodBracket - (timeEnd + 1);        // time 다음부터 method 시작 전까지(공백+indent)
            }

            var outerBegin = FindLine("[Outer] outer");
            var innerBegin = FindLine("[Inner] inner");
            var outerAfter = FindLine("[Outer] outer-after");

            Assert.That(SpacesAfterTime(outerBegin), Is.EqualTo(1 + 2)); // depth 1 => indent 2
            Assert.That(SpacesAfterTime(innerBegin), Is.EqualTo(1 + 4)); // depth 2 => indent 4
            Assert.That(SpacesAfterTime(outerAfter), Is.EqualTo(1 + 2)); // 다시 depth 1

        }
    }
}
