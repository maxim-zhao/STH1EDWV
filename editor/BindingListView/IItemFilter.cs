using System;

namespace sth1edwv.BindingListView
{
    /// <summary>
    /// Defines a general method to test it an item should be included in a <see cref="BindingListView&lt;T&gt;"/>.
    /// </summary>
    /// <typeparam name="T">The type of item to be filtered.</typeparam>
    public interface IItemFilter<in T>
    {
        /// <summary>
        /// Tests if the item should be included.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>True if the item should be included, otherwise false.</returns>
        bool Include(T item);
    }

    /// <summary>
    /// A dummy filter that is used when no filter is needed.
    /// It simply includes any and all items tested.
    /// </summary>
    public class IncludeAllItemFilter<T> : IItemFilter<T>
    {
        public bool Include(T item)
        {
            // All items are to be included.
            // So always return true.
            return true;
        }

        public override string ToString()
        {
            return "NoFilter";
        }

        #region Singleton Accessor

        private static IncludeAllItemFilter<T> _instance;

        /// <summary>
        /// Gets the singleton instance of <see cref="IncludeAllItemFilter&lt;T&gt;"/>.
        /// </summary>
        public static IncludeAllItemFilter<T> Instance => _instance ??= new IncludeAllItemFilter<T>();

        #endregion
    }

    /// <summary>
    /// A filter that uses a user-defined <see cref="BindingListView{T}"/> to test items for inclusion in <see cref="Predicate{T}"/>.
    /// </summary>
    public class PredicateItemFilter<T> : IItemFilter<T>
    {
        /// <summary>
        /// Creates a new <see cref="PredicateItemFilter&lt;T&gt;"/> that uses the specified <see cref="Predicate&lt;T&gt;"/> and default name.
        /// </summary>
        /// <param name="includeDelegate">The <see cref="Predicate&lt;T&gt;"/> used to test items.</param>
        public PredicateItemFilter(Predicate<T> includeDelegate)
            : this(includeDelegate, null)
        {
            // The other constructor is called to do the work.
        }

        /// <summary>
        /// Creates a new <see cref="PredicateItemFilter&lt;T&gt;"/> that uses the specified <see cref="Predicate&lt;T&gt;"/>.
        /// </summary>
        /// <param name="includeDelegate">The <see cref="PredicateItemFilter&lt;T&gt;"/> used to test items.</param>
        /// <param name="name">The name used for the ToString() return value.</param>
        public PredicateItemFilter(Predicate<T> includeDelegate, string name)
        {
            // We don't allow a null string. Use the default instead.
            _name = name ?? defaultName;
            if (includeDelegate != null)
            {
                _includeDelegate = includeDelegate;
            }
            else
            {
                throw new ArgumentNullException(nameof(includeDelegate), "IncludeDelegateCannotBeNull");
            }
        }

        private readonly Predicate<T> _includeDelegate;
        private readonly string _name;
        private readonly string defaultName = "(predicate filter)";

        public bool Include(T item)
        {
            return _includeDelegate(item);
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
