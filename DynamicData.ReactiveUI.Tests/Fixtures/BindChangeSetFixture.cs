using System;
using System.Linq;
using DynamicData.ReactiveUI.Tests.Domain;
using ReactiveUI;
using Xunit;

namespace DynamicData.ReactiveUI.Tests.Fixtures
{
    
    public class BindChangeSetFixture: IDisposable
    {
        private readonly ISourceCache<Person, string> _source;
        private readonly IDisposable _binder;
        private readonly RandomPersonGenerator _generator = new RandomPersonGenerator();


        public BindChangeSetFixture()
        {
            var collection = new ReactiveList<Person>();
            _source = new SourceCache<Person, string>(p => p.Name);
            _binder = _source.Connect().Bind(collection).Subscribe();
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
            _source.AddOrUpdate(person);
            //Assert.AreEqual(1, _collection.Count, "Should be 1 item in the collection");
            //Assert.AreEqual(person, _collection.First(), "Should be same person");
        }

        [Fact]
        public void UpdateToSourceUpdatesTheDestination()
        {
            var person = new Person("Adult1", 50);
            var personUpdated = new Person("Adult1", 51);
            _source.AddOrUpdate(person);
            _source.AddOrUpdate(personUpdated);
            //Assert.AreEqual(1, _collection.Count, "Should be 1 item in the collection");
            //Assert.AreEqual(personUpdated, _collection.First(), "Should be updated person");
        }
        
        [Fact]
        public void RemoveSourceRemovesFromTheDestination()
        {
            var person = new Person("Adult1", 50);
            _source.AddOrUpdate(person);
            _source.Remove(person);
            //Assert.AreEqual(0, _collection.Count, "Should be 1 item in the collection");
        }

        [Fact]
        public void BatchAdd()
        {
            var people = _generator.Take(100).ToList();
            _source.AddOrUpdate(people);
            //Assert.AreEqual(100, _collection.Count, "Should be 100 items in the collection");
           // CollectionAssert.AreEquivalent(people, _collection, "Collections should be equivalent");
        }

        [Fact]
        public void BatchRemove()
        {
            var people = _generator.Take(100).ToList();
            _source.AddOrUpdate(people);
            _source.Clear();
            //Assert.AreEqual(0, _collection.Count, "Should be 100 items in the collection");
        }

    }
}
