using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace sth1edwv.BindingListView
{
    class ProvidedViewPropertyDescriptor : PropertyDescriptor
    {
        public ProvidedViewPropertyDescriptor(string name, Type propertyType)
            : base(name, null)
        {
            PropertyType = propertyType;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType => typeof(IProvideViews);

        public override object GetValue(object component)
        {
            if (component is IProvideViews views)
            {
                return views.GetProvidedView(Name);
            }

            throw new ArgumentException("Type of component is not valid.", nameof(component));
        }

        public override bool IsReadOnly => true;

        public override Type PropertyType { get; }

        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object component, object value)
        {
            throw new NotSupportedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Gets if a BindingListView can be provided for given property. 
        /// The property type must implement IList&lt;&gt; i.e. some generic IList.
        /// </summary>
        public static bool CanProvideViewOf(PropertyDescriptor prop)
        {
            return prop.PropertyType.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>));
        }
    }
}
