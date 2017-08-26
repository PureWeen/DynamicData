﻿using System;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using Xunit;

namespace DynamicData.Tests.Kernal
{
    
    public class ErrorHandlingFixture
    {

        private class Entity
        {
            public int Key => 10;
        }

        private class TransformEntityWithError
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public TransformEntityWithError(Entity entity)
            {
                throw new Exception("Error transforming entity");
            }

            public int Key { get; } = 10;
        }

        private class ErrorInKey
        {
            public static int Key => throw new Exception("Calling Key");
        }

        [Fact]
        public void TransformError()
        {
            bool completed = false;
            bool error = false;

            var cache = new SourceCache<Entity, int>(e => e.Key);

            var subscriber = cache.Connect()
                                  .Transform(e => new TransformEntityWithError(e))
                                  .Finally(() => completed = true)
                                  .Subscribe(updates => { Console.WriteLine(); }, ex => error = true);

            cache.AddOrUpdate(Enumerable.Range(0, 10000).Select(_ => new Entity()).ToArray());
            cache.AddOrUpdate(new Entity());

            subscriber.Dispose();

            error.Should().BeTrue();
            completed.Should().BeTrue();
        }

        [Fact]
        public void FilterError()
        {
            bool completed = false;
            bool error = false;

            var source = new SourceCache<TransformEntityWithError, int>(e => e.Key);

            var subscriber = source.Connect()
                .Filter(x => true)
                .Finally(() => completed = true)
                .Subscribe(updates => { Console.WriteLine(); });

            source.Edit(updater => updater.AddOrUpdate(new TransformEntityWithError(new Entity())), ex => error = true);
            subscriber.Dispose();

            error.Should().BeTrue();
            completed.Should().BeTrue();
        }

        [Fact]
        public void ErrorUpdatingStreamIsHandled()
        {
            bool completed = false;
            bool error = false;

            var cache = new SourceCache<ErrorInKey, int>(p => ErrorInKey.Key);

            var subscriber = cache.Connect()
                .Finally(() => completed = true)
                .Subscribe(updates => { Console.WriteLine(); });

            cache.Edit(updater => updater.AddOrUpdate(new ErrorInKey()), ex => error = true);
            subscriber.Dispose();

            error.Should().BeTrue();
            completed.Should().BeTrue();
        }
    }
}
