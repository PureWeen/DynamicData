using System;
using System.Linq;
using DynamicData.Tests.Domain;
using NUnit.Framework;

namespace DynamicData.Tests.ListFixtures
{
    [TestFixture]
    public class EditDiffFixtureWithOrderPreservation
    {
        private SourceList<Person> _list;
        private ChangeSetAggregator<Person> _result;

        [SetUp]
        public void Initialise()
        {
            _list = new SourceList<Person>();
            _result = _list.Connect().AsAggregator();
            _list.AddRange(Enumerable.Range(1, 10).Select(i => new Person("Name" + i, i)).ToArray());
        }

        [TearDown]
        public void OnTestCompleted()
        {
            _list.Dispose();
            _result.Dispose();
        }

        [Test]
        public void New()
        {
            var newPeople = Enumerable.Range(1, 15).Select(i => new Person("Name" + i, i)).ToArray();

            _list.EditDiff(newPeople, Person.NameAgeGenderComparer);

            Assert.AreEqual(15, _list.Count);
            CollectionAssert.AreEquivalent(newPeople, _list.Items);
            var lastChange = _result.Messages.Last();
            Assert.AreEqual(5, lastChange.Adds);

            CollectionAssert.AreEqual(newPeople, _list.Items);
        }

        [Test]
        public void EditWithSameData()
        {
            var newPeople = Enumerable.Range(1, 10).Select(i => new Person("Name" + i, i)).ToArray();

            _list.EditDiff(newPeople, Person.NameAgeGenderComparer, true);

            Assert.AreEqual(10, _list.Count);
            CollectionAssert.AreEquivalent(newPeople, _list.Items);
            Assert.AreEqual(1, _result.Messages.Count);
        }

        [Test]
        public void Amends()
        {
            var newList = Enumerable.Range(5, 3).Select(i => new Person("Name" + i, i + 10)).ToArray();
            _list.EditDiff(newList, Person.NameAgeGenderComparer, true);

            Assert.AreEqual(3, _list.Count);

            var lastChange = _result.Messages.Last();
            Assert.AreEqual(3, lastChange.Adds);
            Assert.AreEqual(10, lastChange.Removes);
            CollectionAssert.AreEqual(newList, _list.Items);
        }

        [Test]
        public void Removes()
        {
            var newList = Enumerable.Range(1, 7).Select(i => new Person("Name" + i, i)).ToArray();
            _list.EditDiff(newList, Person.NameAgeGenderComparer, true);

            Assert.AreEqual(7, _list.Count);

            var lastChange = _result.Messages.Last();
            Assert.AreEqual(0, lastChange.Adds);
            Assert.AreEqual(3, lastChange.Removes);

            CollectionAssert.AreEqual(newList, _list.Items);
        }


        [Test]
        public void VariousChanges()
        {
            var areSame = Enumerable.Range(1, 3).Select(i => new Person("Name" + i, i)).OrderBy(x=> Guid.NewGuid()).ToArray();
            var changed = Enumerable.Range(6, 10).Select(i => new Person("Name" + i, i+1)).OrderBy(x => Guid.NewGuid()).ToArray();

            var result = areSame.Union(changed).ToArray();

            _list.EditDiff(result, Person.NameAgeGenderComparer, true);

            Assert.AreEqual(13, _list.Count);

            var lastChange = _result.Messages.Last();
            Assert.AreEqual(10, lastChange.Adds);
            Assert.AreEqual(7, lastChange.Removes);

            CollectionAssert.AreEqual(result, _list.Items);
        }
    }
}