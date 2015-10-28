// MvxCachingFragmentActivity.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using Android.OS;
using Android.Runtime;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Support.Fragging.Caching;
using Cirrious.MvvmCross.Droid.Support.Fragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace Cirrious.MvvmCross.Droid.Support.Fragging
{
    public class MvxCachingFragmentActivity
        : MvxFragmentActivity
    {
        private readonly CachedFragmentManager _cachedFragmentManager;
        private readonly FragmentCache _fragmentCache;

        protected MvxCachingFragmentActivity()
        {
            _fragmentCache = new FragmentCache(() => SupportFragmentManager);
            _cachedFragmentManager = new CachedFragmentManager(this, _fragmentCache);
        }

        protected MvxCachingFragmentActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            _fragmentCache = new FragmentCache(() => SupportFragmentManager);
            _cachedFragmentManager = new CachedFragmentManager(this, _fragmentCache);

            BindingContext = new MvxAndroidBindingContext(this, this);
            this.AddEventListeners();
        }

        /// <summary>
        ///     Register a Fragment to be shown, this should usually be done in OnCreate
        /// </summary>
        /// <typeparam name="TFragment">Fragment Type</typeparam>
        /// <typeparam name="TViewModel">ViewModel Type</typeparam>
        /// <param name="tag">The tag of the Fragment, it is used to register it with the FragmentManager</param>
        public MvxCachingFragmentActivity RegisterFragment<TFragment, TViewModel>(string tag)
            where TFragment : IMvxFragmentView
            where TViewModel : IMvxViewModel
        {
            _fragmentCache.RegisterFragment<TFragment, TViewModel>(tag);
            return this;
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            _fragmentCache.RestoreCachedFragments(savedInstanceState);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _fragmentCache.CacheFragments(outState);
            base.OnSaveInstanceState(outState);
        }

        /// <summary>
        ///     Show Fragment with a specific tag at a specific placeholder
        /// </summary>
        /// <param name="showFragmentRequest">Configures show fragment details.</param>
        protected void ShowFragment(ShowFragmentRequest showFragmentRequest)
        {
            if (showFragmentRequest != null)
                _cachedFragmentManager.ShowFragment(showFragmentRequest);
        }

        public override void OnBackPressed()
        {
            _cachedFragmentManager.HandleOnBackPressed();
        }

        /// <summary>
        ///     Close Fragment with a specific tag at a specific placeholder
        /// </summary>
        /// <param name="closeFragmentRequest">Configures which and where fragment should be closed.</param>
        protected void CloseFragment(CloseFragmentRequest closeFragmentRequest)
        {
            if (closeFragmentRequest != null)
                _cachedFragmentManager.CloseFragment(closeFragmentRequest);
        }

        public event Action<FragmentChangingEvenArgs> BeforeFragmentChanging
        {
            add
            {
                lock (this)
                {
                    _cachedFragmentManager.BeforeFragmentChanging += value;
                }
            }
            remove
            {
                lock (this)
                {
                    _cachedFragmentManager.BeforeFragmentChanging -= value;
                }
            }
        }

        public event Action<FragmentChangingEvenArgs> FragmentChanging
        {
            add
            {
                lock (this)
                {
                    _cachedFragmentManager.FragmentChanging += value;
                }
            }
            remove
            {
                lock (this)
                {
                    _cachedFragmentManager.FragmentChanging -= value;
                }
            }
        }
    }

    public abstract class MvxCachingFragmentActivity<TViewModel>
        : MvxCachingFragmentActivity
            , IMvxAndroidView<TViewModel> where TViewModel : class, IMvxViewModel
    {
        public new TViewModel ViewModel
        {
            get { return (TViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }
    }
}