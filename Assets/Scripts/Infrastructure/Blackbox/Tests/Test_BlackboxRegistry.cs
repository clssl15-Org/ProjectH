using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using static BlackboxSystem.Tests.Asserts;
using static UnityEngine.UI.GridLayoutGroup;

namespace BlackboxSystem.Tests
{
    public class Test_BlackboxRegistry
    {
        private const int MaxTryCount = 5;

        [SetUp]
        public void SetUp()
        {
            BlackboxRegistry.ForceReset();
            Infrastructure.StrongReference = false;
        }

        private void CreateOwner(string name, out NamedOwner owner)
        {
            owner = new NamedOwner(name);
        }
        private void GetBlackbox(object owner, out Blackbox blackbox)
        {
            blackbox = BlackboxRegistry.GetBlackbox(owner);
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

            // Assert
            int count = 0;
            while (true)
            {
                Tools.ForceGC();

                try
                {
                    Assert.That(BlackboxRegistry.Count(), Is.EqualTo(0));
                    break;
                }
                catch (AssertionException) { }

                if (++count > MaxTryCount)
                {
                    Assert.Fail($"BlackboxRegistry.Count() is '{BlackboxRegistry.Count()}'");
                    break;
                }
            }
        }
        [TestCase(true), TestCase(false)]
        public void ReferenceLost_Multiple(bool strongReference)
        {
            // Arrange
            var holdingOwner = new object();
            Infrastructure.StrongReference = strongReference;
            BlackboxRegistry.GetBlackbox(holdingOwner);

            CreateOwner("Owner", out var owner);
            BlackboxRegistry.GetBlackbox(owner);

            // Act
            owner = null;

            // Assert
            int count = 0;
            while (true)
            {
                Tools.ForceGC();

                try
                {
                    Assert.That(BlackboxRegistry.Count(), Is.EqualTo(1));
                    break;
                }
                catch (AssertionException) { }

                if (++count > MaxTryCount)
                {
                    Assert.Fail($"BlackboxRegistry.Count() is '{BlackboxRegistry.Count()}'");
                    break;
                }
            }
        }
        [TestCase(true), TestCase(false)]
        public void ReferenceLost_Related(bool strongReference)
        {
            // Arrange
            Infrastructure.StrongReference = strongReference;

            CreateOwner("Owner 1", out var owner1);
            CreateOwner("Owner 2", out var owner2);

            GetBlackbox(owner1, out Blackbox bb1);
            GetBlackbox(owner2, out Blackbox bb2);

            bb1.Exert(bb2, "Exerting");

            // Act
            owner1 = null;
            owner2 = null;
            bb1 = null;
            bb2 = null;

            // Assert
            int count = 0;
            while (true)
            {
                Tools.ForceGC();

                try
                {
                    Assert.That(BlackboxRegistry.Count(), Is.EqualTo(0));
                    break;
                }
                catch (AssertionException) { }

                if (++count > MaxTryCount)
                {
                    Assert.Fail($"BlackboxRegistry.Count() is '{BlackboxRegistry.Count()}'");
                    break;
                }
            }
        }
        #endregion
    }
}
