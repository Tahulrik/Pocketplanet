using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class SelectionTests
    {
        GameObject first;
        GameObject second;
        int changed;
        int reselected;

        [SetUp]
        public void SetUp()
        {
            first = new GameObject("First");
            second = new GameObject("Second");
            changed = 0;
            reselected = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (first != null)
                Object.DestroyImmediate(first);
            if (second != null)
                Object.DestroyImmediate(second);
        }

        Selection Watch(Selection selection)
        {
            selection.Changed += () => changed++;
            selection.Reselected += () => reselected++;
            return selection;
        }

        [Test]
        public void Select_SetsCurrent_RaisesChanged()
        {
            var selection = Watch(new Selection());
            selection.Select(first);
            Assert.AreSame(first, selection.Current);
            Assert.IsTrue(selection.HasTarget);
            Assert.AreEqual(1, changed);
        }

        [Test]
        public void SelectSame_RaisesReselectedNotChanged()
        {
            var selection = Watch(new Selection());
            selection.Select(first);
            selection.Select(first);
            Assert.AreEqual(1, changed);
            Assert.AreEqual(1, reselected);
        }

        [Test]
        public void SelectOther_Switches_RaisesChanged()
        {
            var selection = Watch(new Selection());
            selection.Select(first);
            selection.Select(second);
            Assert.AreSame(second, selection.Current);
            Assert.AreEqual(2, changed);
        }

        [Test]
        public void Clear_RaisesChanged_OnlyWhenSomethingSelected()
        {
            var selection = Watch(new Selection());
            selection.Clear();
            Assert.AreEqual(0, changed);
            selection.Select(first);
            selection.Clear();
            Assert.IsFalse(selection.HasTarget);
            Assert.AreEqual(2, changed);
        }

        [Test]
        public void SelectNull_Clears()
        {
            var selection = Watch(new Selection());
            selection.Select(first);
            selection.Select(null);
            Assert.IsNull(selection.Current);
            Assert.AreEqual(2, changed);
        }

        [Test]
        public void Refresh_DestroyedTarget_Clears()
        {
            var selection = Watch(new Selection());
            selection.Select(first);
            Object.DestroyImmediate(first);
            selection.Refresh();
            Assert.IsFalse(selection.HasTarget);
            Assert.AreEqual(2, changed);
            selection.Refresh();
            Assert.AreEqual(2, changed);
        }

        [Test]
        public void Refresh_InactiveTarget_Clears()
        {
            var selection = Watch(new Selection());
            selection.Select(first);
            first.SetActive(false);
            selection.Refresh();
            Assert.IsFalse(selection.HasTarget);
        }

        [Test]
        public void Refresh_InvalidTarget_Clears()
        {
            var valid = true;
            var selection = Watch(new Selection(_ => valid));
            selection.Select(first);
            selection.Refresh();
            Assert.IsTrue(selection.HasTarget);
            valid = false;
            selection.Refresh();
            Assert.IsFalse(selection.HasTarget);
            Assert.AreEqual(2, changed);
        }
    }
}
