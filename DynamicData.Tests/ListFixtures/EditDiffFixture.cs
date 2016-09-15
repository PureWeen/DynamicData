using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DynamicData.Kernel;
using DynamicData.Tests.Domain;
using NUnit.Framework;

namespace DynamicData.Tests.ListFixtures
{

    [TestFixture]
    public class EditDiffFixture
    {
        private SourceList<Person> _cache;
        private ChangeSetAggregator<Person> _result;

        [SetUp]
        public void Initialise()
        {
            _cache = new SourceList<Person>();
            _result = _cache.Connect().AsAggregator();
            _cache.AddRange(Enumerable.Range(1, 10).Select(i => new Person("Name" + i, i)).ToArray());
        }

        [TearDown]
        public void OnTestCompleted()
        {
            _cache.Dispose();
            _result.Dispose();
        }

        [Test]
        public void New()
        {
            var newPeople = Enumerable.Range(1, 15).Select(i => new Person("Name" + i, i)).ToArray();

            _cache.EditDiff(newPeople, Person.NameAgeGenderComparer);

            Assert.AreEqual(15, _cache.Count);
            CollectionAssert.AreEquivalent(newPeople, _cache.Items);
            var lastChange = _result.Messages.Last();
            Assert.AreEqual(5, lastChange.Adds);
        }

        [Test]
        public void EditWithSameData()
        {
            var newPeople = Enumerable.Range(1, 10).Select(i => new Person("Name" + i, i)).ToArray();

            _cache.EditDiff(newPeople, Person.NameAgeGenderComparer);

            Assert.AreEqual(10, _cache.Count);
            CollectionAssert.AreEquivalent(newPeople, _cache.Items);
            Assert.AreEqual(1, _result.Messages.Count);
        }

        [Test]
        public void Amends()
        {
            var newList = Enumerable.Range(5, 3).Select(i => new Person("Name" + i, i + 10)).ToArray();
            _cache.EditDiff(newList, Person.NameAgeGenderComparer);

            Assert.AreEqual(3, _cache.Count);

            var lastChange = _result.Messages.Last();
            Assert.AreEqual(3, lastChange.Adds);
            Assert.AreEqual(10, lastChange.Removes);

            CollectionAssert.AreEquivalent(newList, _cache.Items);
        }

        [Test]
        public void Removes()
        {
            var newList = Enumerable.Range(1, 7).Select(i => new Person("Name" + i, i)).ToArray();
            _cache.EditDiff(newList, Person.NameAgeGenderComparer);

            Assert.AreEqual(7, _cache.Count);

            var lastChange = _result.Messages.Last();
            Assert.AreEqual(0, lastChange.Adds);
            Assert.AreEqual(3, lastChange.Removes);

            CollectionAssert.AreEquivalent(newList, _cache.Items);
        }


        [Test]
        public void VariousChanges()
        {

            var newList = Enumerable.Range(6, 10).Select(i => new Person("Name" + i, i )).ToArray();

            _cache.EditDiff(newList, Person.NameAgeGenderComparer);

            Assert.AreEqual(10, _cache.Count);

            var lastChange = _result.Messages.Last();
            Assert.AreEqual(5, lastChange.Adds);
            Assert.AreEqual(5, lastChange.Removes);

            CollectionAssert.AreEquivalent(newList, _cache.Items);
        }

        [Test]
        public void IndexOfManyTest()
        {
            var generator = new RandomPersonGenerator();

            var original = Enumerable.Range(1, 10).Select(i => new Person("Name" + i, i)).ToArray();

            var changed = original.OrderBy(x => Guid.NewGuid()).ToArray();


            _cache.EditDiff(original, Person.NameAgeGenderComparer,true);
            _cache.EditDiff(changed, Person.NameAgeGenderComparer,true);

            CollectionAssert.AreEqual(changed, _cache.Items);

        }

        [Test]
        public void IndexOfManyTest2()
        {
            var original = Enumerable.Range(1, 9).Select(i => new Person("Name" + i, i)).ToArray();
            var sorted = original.OrderBy(x => Guid.NewGuid()).ToArray();
            var clone = original.ToList();

            //we need to update the clone collection using the sorted items
            var matchedIndicies = clone.IndexOfMany(sorted)
                .Select((many, index) => new { OriginalIndex = many.Index, NewIndex = index, Item = many.Item })
                .Where(x => x.NewIndex != x.OriginalIndex)
                .ToArray();

     
            int movedUp = 0;
            int movedDown = 0;
            matchedIndicies.ForEach((x, idx) =>
            {

                Debug.WriteLine(matchedIndicies);

                int actualIndex;
                if (x.OriginalIndex > x.NewIndex)
                {
                    actualIndex = x.OriginalIndex ;
                }
                else
                {
                    actualIndex = x.OriginalIndex + x.NewIndex;
                }

              //  var originalIndex = x.OriginalIndex + insertedAboveCurrent;
                var originalItem = clone[actualIndex];

                if (!(ReferenceEquals(originalItem, x.Item)))
                {
                    var actualOriginalIndex = clone.IndexOf(x.Item);
                    Debug.WriteLine("Original is at " + actualOriginalIndex);
                }

                Move(clone, actualIndex, x.NewIndex);

                var current = clone[x.NewIndex];

                if (!(ReferenceEquals(current, x.Item)))
                    Debug.WriteLine(current);

                if (x.OriginalIndex > x.NewIndex)
                {
                    movedDown++;
                }
                else
                {
                    movedUp++;
                }

            });

        }



        public virtual void Move(List<Person> target, int original, int destination)
        {
            var item = target[original];
            //if (!(ReferenceEquals(original, x.Item)))
            //{
            //    var actualOriginalIndex = innerList.IndexOf(x.Item);
            //    Console.WriteLine("Original is at " + actualOriginalIndex);


            //}
            target.RemoveAt(original);
            target.Insert(destination, item);
        }
    }
}
