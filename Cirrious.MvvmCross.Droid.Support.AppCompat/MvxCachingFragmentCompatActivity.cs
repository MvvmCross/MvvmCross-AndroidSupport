// MvxCachingFragmentActivityCompat.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Support.Fragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using System;
using Android.Content;
using Cirrious.MvvmCross.Droid.Support.Fragging;
using Cirrious.MvvmCross.Droid.Support.Fragging.Cache;

namespace Cirrious.MvvmCross.Droid.Support.AppCompat
{
	[Register("cirrious.mvvmcross.droid.support.appcompat.MvxCachingFragmentCompatActivity")]
	public class MvxCachingFragmentCompatActivity : MvxFragmentCompatActivity
    {
        private readonly Lazy<MvxActivityCachingProcessor> _lazyActivityCachingProcessorFactory;
        protected MvxActivityCachingProcessor ActivityCachingProcessor => _lazyActivityCachingProcessorFactory.Value;

	    ///     Register a Fragment to be shown, this should usually be done in OnCreate
        /// </summary>
        /// <typeparam name="TFragment">Fragment Type</typeparam>
        /// <typeparam name="TViewModel">ViewModel Type</typeparam>
        /// <param name="tag">The tag of the Fragment, it is used to register it with the FragmentManager</param>
        public MvxCachingFragmentCompatActivity RegisterFragment<TFragment, TViewModel>(string tag)
            where TFragment : IMvxFragmentView
            where TViewModel : IMvxViewModel
        {
            ActivityCachingProcessor.RegisterFragment<TFragment, TViewModel>(tag);
            return this;
        }

        protected MvxCachingFragmentCompatActivity()
        {
            _lazyActivityCachingProcessorFactory = new Lazy<MvxActivityCachingProcessor>(BuildAndSetupActivityCachingProcessor);
        }

        protected MvxCachingFragmentCompatActivity(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            _lazyActivityCachingProcessorFactory = new Lazy<MvxActivityCachingProcessor>(BuildAndSetupActivityCachingProcessor);
            BindingContext = new MvxAndroidBindingContext(this, this);
            this.AddEventListeners();
        }

	    private MvxActivityCachingProcessor BuildAndSetupActivityCachingProcessor()
	    {
	        var activityCachingProcessor = BuildActivityCachingProcessor(() => SupportFragmentManager, () => this);
	        activityCachingProcessor.BeforeFragmentChanging += OnBeforeFragmentChanging;
	        activityCachingProcessor.FragmentChanging += OnFragmentChanging;

	        return activityCachingProcessor;
	    }

        protected virtual MvxActivityCachingProcessor BuildActivityCachingProcessor(Func<FragmentManager> fragmentManagerProvider, Func<Context> androidContext)
        {
            return new MvxActivityCachingProcessor(fragmentManagerProvider, androidContext);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            ActivityCachingProcessor.RestoreFragmentsOnPostCreate(savedInstanceState);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            ActivityCachingProcessor.CacheFragmentsOnActivityStateSave(outState);
            base.OnSaveInstanceState(outState);
        }

        /// <summary>
        ///     Show Fragment with a specific tag at a specific placeholder
        /// </summary>
        /// <param name="tag">The tag for the fragment to lookup</param>
        /// <param name="contentId">Where you want to show the Fragment</param>
        /// <param name="bundle">Bundle which usually contains a Serialized MvxViewModelRequest</param>
        /// <param name="addToBackStack">If you want to add the fragment to the backstack so on backbutton it will go back to it</param>
        /// <param name="forceReplaceFragment">Force replace a fragment with the same viewmodel at the same contentid</param>
        protected void ShowFragment(string tag, int contentId, Bundle bundle = null, bool addToBackStack = false, bool forceReplaceFragment = false)
        {
            ActivityCachingProcessor.ShowFragment(tag, contentId, bundle, addToBackStack, forceReplaceFragment);
        }

        public override void OnBackPressed()
        {
            ActivityCachingProcessor.HandleFragmentsStateOnBackPressed();
            base.OnBackPressed();
        }

        /// <summary>
        /// Close Fragment with a specific tag at a specific placeholder
        /// </summary>
        /// <param name="tag">The tag for the fragment to lookup</param>
        /// <param name="contentId">Where you want to close the Fragment</param>
        protected void CloseFragment(string tag, int contentId)
        {
            ActivityCachingProcessor.CloseFragment(tag, contentId);
        }

	    public virtual void OnBeforeFragmentChanging(string tag, FragmentTransaction transaction)
	    {
	    }

	    public virtual void OnFragmentChanging(string tag, FragmentTransaction transaction)
	    {

        }        
    }

    public abstract class MvxCachingFragmentCompatActivity<TViewModel>
        : MvxCachingFragmentCompatActivity
    , IMvxAndroidView<TViewModel> where TViewModel : class, IMvxViewModel
    {
        public new TViewModel ViewModel
        {
            get { return (TViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
    }
}
