using System;
using System.Linq;
using DynamicData.ReactiveUI.Tests.Domain;
using FluentAssertions;
using ReactiveUI;
using Xunit;

namespace DynamicData.ReactiveUI.Tests.Fixtures
{
    
    public class BindFromObservableListFixture: IDisposable
    {
        private readonly ReactiveList<Person> _collection;
        private readonly SourceList<Person> _source;
        private IDisposable _binder;
        private static readonly RandomPersonGenerator _generator = new RandomPersonGenerator();

        public BindFromObservableListFixture()
        {
            _collection = new ReactiveList<Person>();
            _source = new SourceList<Person>();
            _binder = _source.Connect().Bind(_collection).Subscribe();
        }

        public void Dispose()
        {
            _binder.Dispose();
            _source.Dispose();
        }

        [Fact]
        public void AddToSourceAddsToDestination()
        {
            var person = new Person("Adult1", 50);
            _source.Add(person);

            _collection.Count.Should().Be(1);
            _collection.First().Should().Be(person);
        }

        [Fact]
        public void UpdateToSourceUpdatesTheDestination()
        {
            var person = new Person("Adult1", 50);
            var personUpdated = new Person("Adult1", 51);
            _source.Add(person);
            _source.Replace(person, personUpdated);

            ////Assert.AreEqual(1, _collection.Count, "Should be 1 item in the collection");
            //Assert.AreEqual(personUpdated, _collection.First(), "Should be updated person");
        }


        [Fact]
        public void RemoveSourceRemovesFromTheDestination()
        {
            var person = new Person("Adult1", 50);
            _source.Add(person);
            _source.Remove(person);

            //Assert.AreEqual(0, _collection.Count, "Should be 1 item in the collection");
        }

        [Fact]
        public void AddRange()
        {
            var people = _generator.Take(100).ToList();
            _source.AddRange(people);

            //Assert.AreEqual(100, _collection.Count, "Should be 100 items in the collection");
           // CollectionAssert.AreEquivalent(people, _collection, "Collections should be equivalent");
        }

        [Fact]
        public void InsertRange()
        {
            var people = _generator.Take(100).ToList();
            _source.AddRange(people);

            var morePeople = _generator.Take(100).ToList();
            _source.InsertRange(morePeople,50);




            //Assert.AreEqual(200, _collection.Count, "Should be 200 items in the collection");
           // CollectionAssert.AreEquivalent(_collection.Skip(50).Take(100), morePeople, "Collections should be equivalent");
        }

        [Fact]
        public void Clear()
        {
            var people = _generator.Take(100).ToList();
            _source.AddRange(people);
            _source.Clear();
            //Assert.AreEqual(0, _collection.Count, "Should be 100 items in the collection");
        }

    }
}