using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Mageki.Views
{
    public class ViewTag
    {
        public static readonly BindableProperty TagProperty = BindableProperty.CreateAttached(
            propertyName: "Tag",
            defaultValue: null,
            returnType: typeof(object),
            declaringType: typeof(VisualElement));

        public static object GetTag(BindableObject bindable)
        {
            if (bindable == null)
            {
                throw new ArgumentNullException("bindable");
            }

            return bindable!.GetValue(TagProperty);
        }

        public static void SetTag(BindableObject bindable, object value)
        {
            bindable?.SetValue(TagProperty, value);
        }
    }
}
