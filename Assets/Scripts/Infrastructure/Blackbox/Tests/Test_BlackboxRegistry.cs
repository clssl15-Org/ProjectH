using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static BlackboxSystem.Tests.Asserts;

namespace BlackboxSystem.Tests
{
    public class Test_BlackboxRegistry
    {
        [SetUp]
        public void SetUp()
        {
            Blackbox.ForceResetStaticProperties();
            Infrastructure.StrongReference = false;
        }

        private void CreateOwner(string name, out NamedOwner owner)
        {
            owner = new NamedOwner(name);
        }


        #region Get Blackbox
        [Test]
        public void GetBlackbox_ValidSubject()
        {
            // Arrange
            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);

            // Act
            var blackbox = BlackboxRegistry.GetBlackbox(owner);

            // Assert
            AssertBlackbox(blackbox, owner, ownerName, 0);
        }
        [Test]
        public void GetBlackbox_ValidSubject_Multiple()
        {
            // Arrange
            var ownerNames = Enumerable.Range(0, 5).Select(i => $"Owner {i}").ToList();
            var owners = ownerNames.Select(name => new NamedOwner(name)).ToList();

            // Act
            var blackboxes = owners.Select(owner => BlackboxRegistry.GetBlackbox(owner)).ToList();

            // Assert
            for (int i = 0; i < blackboxes.Count; i++)
                AssertBlackbox(blackboxes[i], owners[i], ownerNames[i], i);
        }

        [Test]
        public void GetBlackbox_NullSubject()
        {
            // Act & Assert
            Assert.That(() => BlackboxRegistry.GetBlackbox(null), Throws.ArgumentNullException);
        }
        #endregion


        #region Contains
        [Test]
        public void Contains_ValidSubject()
        {
            // Arrange
            var ownerName = "Owner";
            var owner = new NamedOwner(ownerName);
            BlackboxRegistry.GetBlackbox(owner);

            // Act & Assert
            Assert.That(BlackboxRegistry.Contains(owner), Is.True);
        }
        [Test]
        public void Contains_ValidSubject_Multiple_A()
        {
            // Arrange
            var ownerNames = Enumerable.Range(0, 5).Select(i => $"Owner {i}").ToList();
            var owners = ownerNames.Select(name => new NamedOwner(name)).ToList();
            owners.ForEach(owner => BlackboxRegistry.GetBlackbox(owner));

            // Act & Assert
            Assert.That(owners.All(owner => BlackboxRegistry.Contains(owner)), Is.True);
        }
        [Test]
        public void Contains_ValidSubject_Multiple_B()
        {
            // Arrange
            IEnumerable<NamedOwner> owners = Enumerable
                .Range(0, 5)
                .Select(i => $"Owner {i}")
                .Select(name => new NamedOwner(name))
                .ToList();

            foreach (var owner in owners) BlackboxRegistry.GetBlackbox(owner);
            owners = owners.Concat(new NamedOwner[] { new("A"), new("B"), new("C") });

            // Act & Assert
            Assert.That(owners.Count(owner => BlackboxRegistry.Contains(owner)), Is.EqualTo(5));
        }

        [Test]
        public void Contains_NullSubject()
        {
            // Act & Assert
            Assert.That(() => BlackboxRegistry.Contains(null), Throws.ArgumentNullException);
        }
        [Test]
        public void Contains_NullSubject_Multiple()
        {
            // Arrange
            IEnumerable<NamedOwner> owners = Enumerable
                .Range(0, 5)
                .Select(i => $"Owner {i}")
                .Select(name => new NamedOwner(name))
                .ToList();

            foreach (var owner in owners) BlackboxRegistry.GetBlackbox(owner);
            owners = owners.Concat(new NamedOwner[] { null });

            // Act & Assert
            Assert.That(() => owners.Select(owner => BlackboxRegistry.Contains(owner)).ToList(), Throws.ArgumentNullException);
        }
        #endregion


        #region Reference Lost
        [TestCase(true), TestCase(false)]
        public void ReferenceLost(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            CreateOwner("Owner", out var owner);
            BlackboxRegistry.GetBlackbox(owner);

            // Act
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            // Assert
            Assert.That(BlackboxRegistry.Count(), Is.EqualTo(0));
        }
        [TestCase(true), TestCase(false)]
        public void ReferenceLost_Multiple(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;
            BlackboxRegistry.GetBlackbox(new object());

            CreateOwner("Owner", out var owner);
            BlackboxRegistry.GetBlackbox(owner);

            // Act
            owner = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            // Assert
            Assert.That(BlackboxRegistry.Count(), Is.EqualTo(1));
        }
        #endregion
    }
}
