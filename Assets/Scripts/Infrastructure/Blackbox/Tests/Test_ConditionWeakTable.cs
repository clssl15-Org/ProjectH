using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace BlackboxSystem.Tests
{
    public class Test_ConditionWeakTable
    {
        private ConditionalWeakTable<object, Holder> _table;
        private class Holder
        {
            public WeakReference<object> Key { get; set; }
            public Holder(object key) => Key = new(key);
        }

        [SetUp]
        public void SetUp()
        {
            _table = new();
        }


        [Test]
        public void GetValue_NotExist()
        {
            // Arrange
            var key = new object();

            // Act
            var resultValue = _table.GetValue(key, k => new Holder(k));

            // Assert
            Assert.That(_table.TryGetValue(key, out var attainedValue), Is.True);
            Assert.That(attainedValue, Is.SameAs(resultValue));

            Assert.That(attainedValue.Key.TryGetTarget(out var actualKey), Is.True);
            Assert.That(actualKey, Is.SameAs(key));
        }

        [Test]
        public void GetValue_Exist()
        {
            // Arrange
            var key = new object();
            var value = _table.GetValue(key, k => new Holder(k));

            // Act
            var resultValue = _table.GetValue(key, k => new Holder(k));

            // Assert
            Assert.That(_table.TryGetValue(key, out var actualValue), Is.True);
            Assert.That(resultValue, Is.SameAs(value));
            Assert.That(actualValue, Is.SameAs(value));

            Assert.That(actualValue.Key.TryGetTarget(out var actualKey), Is.True);
            Assert.That(actualKey, Is.SameAs(key));
        }


        [Test]
        public void KeyGCCollected()
        {
            // Arrange
            var holderRef = CreateHolderRef(_table);

            // Act
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            // Assert
            Assert.That(holderRef.TryGetTarget(out _), Is.False);


            WeakReference<Holder> CreateHolderRef(ConditionalWeakTable<object, Holder> table)
            {
                var key = new object();
                var holder = table.GetValue(key, k => new Holder(k));
                return new WeakReference<Holder>(holder);
            }
        }
    }
}
