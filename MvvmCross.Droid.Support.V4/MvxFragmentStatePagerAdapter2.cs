using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using Java.Lang;
using MvvmCross.Droid.Support.V7.Fragging.Fragments;
using Object = Java.Lang.Object;
using String = Java.Lang.String;

namespace MvvmCross.Droid.Support.V4
{
    public class MvxFragmentStatePagerAdapter2
        : FixedFragmentStatePagerAdapter
    {
        private readonly Context _context;

        protected MvxFragmentStatePagerAdapter2(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public MvxFragmentStatePagerAdapter2(Context context, FragmentManager fragmentManager,
                                             IEnumerable<FragmentInfo> fragments)
            : base(fragmentManager)
        {
            _context = context;
            Fragments = fragments;
        }

        public override int Count => Fragments.Count();

        public IEnumerable<FragmentInfo> Fragments { get; }

        protected static string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public override Fragment GetItem(int position)
        {
            var fragment = CreateFragment(position);
            return fragment;
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(Fragments.ElementAt(position).Title);
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            var fragment = base.InstantiateItem(container, position);
            var updateFragment = UpdateFragment(position, (Fragment) fragment);

            return updateFragment;
        }

        protected override string GetTag(int position)
        {
            return Fragments.ElementAt(position).Tag;
        }

        private Fragment CreateFragment(int position)
        {
            var fragInfo = Fragments.ElementAt(position);

            var fragment = Fragment.Instantiate(_context, FragmentJavaName(fragInfo.FragmentType));
            var mvxFragment = fragment as MvxFragment;
            if (mvxFragment == null)
                return fragment;

            fragInfo.CachedViewModel = CreateViewModel(position);
            mvxFragment.ViewModel = fragInfo.CachedViewModel;

            return fragment;
        }

        private IMvxViewModel CreateViewModel(int position)
        {
            var fragInfo = Fragments.ElementAt(position);

            MvxBundle mvxBundle = null;
            if (fragInfo.ParameterValuesObject != null)
                mvxBundle = new MvxBundle(fragInfo.ParameterValuesObject.ToSimplePropertyDictionary());

            var request = new MvxViewModelRequest(fragInfo.ViewModelType, mvxBundle, null, null);

            return Mvx.Resolve<IMvxViewModelLoader>().LoadViewModel(request, null);
        }

        private Fragment UpdateFragment(int position, Fragment fragment)
        {
            var mvxFragment = fragment as MvxFragment;
            if (mvxFragment == null)
                return fragment;

            if (mvxFragment.ViewModel != null)
                return fragment;

            var fragInfo = Fragments.ElementAt(position);

            if (fragInfo.CachedViewModel == null)
                fragInfo.CachedViewModel = CreateViewModel(position);

            mvxFragment.ViewModel = fragInfo.CachedViewModel;

            return mvxFragment;
        }

        //Do call restore state
        //public override void RestoreState(IParcelable state, ClassLoader loader)
        //{
        //    //Don't call restore to prevent crash on rotation
        //    //base.RestoreState (state, loader);
        //}

        public class FragmentInfo
        {
            public FragmentInfo(string title, Type fragmentType, Type viewModelType, object parameterValuesObject = null)
                : this(title, null, fragmentType, viewModelType, parameterValuesObject)
            {
            }

            public FragmentInfo(string title, string tag, Type fragmentType, Type viewModelType,
                                object parameterValuesObject = null)
            {
                Title = title;
                Tag = tag ?? title;
                FragmentType = fragmentType;
                ViewModelType = viewModelType;
                ParameterValuesObject = parameterValuesObject;
            }

            //public Fragment CachedFragment { get; set; }
            public IMvxViewModel CachedViewModel { get; set; }

            public Type FragmentType { get; }

            public object ParameterValuesObject { get; }

            public string Tag { get; }

            public string Title { get; }

            public Type ViewModelType { get; }
        }
    }
}