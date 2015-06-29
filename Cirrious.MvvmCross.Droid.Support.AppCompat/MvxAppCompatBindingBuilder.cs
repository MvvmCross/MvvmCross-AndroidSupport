// MvxActivityCompat.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using Cirrious.MvvmCross.Binding.Droid;
using Cirrious.MvvmCross.Binding.Droid.Binders;

namespace Cirrious.MvvmCross.Droid.Support.AppCompat
{
    public class MvxAppCompatBindingBuilder : MvxAndroidBindingBuilder
    {
        protected override IMvxAndroidViewFactory CreateAndroidViewFactory()
        {
            return new MvxAppCompat21ViewFactory();
        }
    }
}

