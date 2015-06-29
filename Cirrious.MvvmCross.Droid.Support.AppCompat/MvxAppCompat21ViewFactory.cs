// MvxActivityCompat.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using Android.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.Binders;
using Cirrious.MvvmCross.Droid.Support.AppCompat.Widget;

namespace Cirrious.MvvmCross.Droid.Support.AppCompat
{
    public class MvxAppCompat21ViewFactory : MvxAndroidViewFactory
    {
        private static AppCompatDelegate GetDelegate(Context context)
        {
            // Special case this for when you know you have an AppCompatDelegate
            AppCompatActivity activity = context as AppCompatActivity;
            return activity != null ? activity.Delegate : null;
        }

        public override View CreateView(View parent, string name, Context context, IAttributeSet attrs)
        {
            var @delegate = GetDelegate(context);
            if (@delegate != null)
            {
                var view = @delegate.CreateView(parent, name, context, attrs);
                if (view != null)
                    return view;

                if (name == "MvxSpinner")
                {
                    return new MvxAppCompatSpinner(context, attrs);
                }
            }

            return base.CreateView(parent, name, context, attrs);
        }
    }
}

