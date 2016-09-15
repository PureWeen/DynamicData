using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DynamicData.Annotations;
using DynamicData.Kernel;

namespace DynamicData.List.Internal
{
    internal class EditDiff<T>
    {
        private readonly ISourceList<T> _source;
        private readonly bool _preserveOrder;
        private readonly IEqualityComparer<T> _equalityComparer;
      
        public EditDiff([NotNull] ISourceList<T> source, IEqualityComparer<T> equalityComparer, bool preserveOrder = false)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _source = source;
            _preserveOrder = preserveOrder;
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        public void Edit(IEnumerable<T> items)
        {
            _source.Edit(innerList =>
            {
                var originalItems = innerList.AsArray();
                var newItems = items.AsArray();
                var removes = originalItems.Except(newItems, _equalityComparer);
                var adds = newItems.Except(originalItems, _equalityComparer);
                
                innerList.EnsureCapacityFor(newItems);
                innerList.RemoveMany(removes);
                innerList.AddRange(adds);

                if (!_preserveOrder) return;

                //var desiredOrder = newItems.Select((t, index) => new {Item = t, Index = index}).ToArray();

                //This is a superfast algorithm for reordering a list
                var currentIndicies = innerList.IndexOfMany(newItems)
                    .Select((many,index)=>new {OriginalIndex= many.Index, NewIndex = index, Item= many.Item })
                    .Where(x=>x.NewIndex != x.OriginalIndex)
                    .ToArray();

                int offset = 0;
                int i =0;
                Debug.WriteLine(currentIndicies);
                currentIndicies.ForEach(x =>
                {
                    Debug.WriteLine(currentIndicies);

                    if (x.OriginalIndex < x.NewIndex)
                        i++;

                    var originalIndex = x.OriginalIndex + offset;



                    //if (x.NewIndex != x.OriginalIndex)
                    //    return;
                    var original = innerList[originalIndex];

                    if (!(ReferenceEquals(original, x.Item)))
                    {
                        var actualOriginalIndex = innerList.IndexOf(x.Item);
                       Debug.WriteLine("Original is at " + actualOriginalIndex);


                    }

                    innerList.Move(originalIndex, x.NewIndex);

                    var current = innerList[x.NewIndex];

                    if (!(ReferenceEquals(current, x.Item)))
                    {
                        Debug.WriteLine(current);
                    }





                        });
                Debug.WriteLine(currentIndicies);
            });
        }
    }
}
