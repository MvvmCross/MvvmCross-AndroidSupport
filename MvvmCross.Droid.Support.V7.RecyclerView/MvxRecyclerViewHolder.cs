// MvxRecyclerViewHolder.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Windows.Input;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;

namespace MvvmCross.Droid.Support.V7.RecyclerView
{
    public class MvxRecyclerViewHolder 
        : Android.Support.V7.Widget.RecyclerView.ViewHolder
        , IMvxRecyclerViewHolder
    {
        private readonly IMvxBindingContext _bindingContext;

        private object _cachedDataContext;
        private ICommand _click, _longClick;
        private bool _clickOverloaded, _longClickOverloaded;

        public MvxRecyclerViewHolder(View itemView, IMvxAndroidBindingContext context)
            : base(itemView)
        {
            _bindingContext = context;
        }

        public IMvxBindingContext BindingContext
        {
            get { return _bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the list item"); }
        }

        public object DataContext
        {
            get { return _bindingContext.DataContext; }
            set { _bindingContext.DataContext = value; }
        }

        public ICommand Click
        {
            get { return _click; }
            set
            {
                _click = value;
                if (_click != null)
                    EnsureClickOverloaded();
            }
        }

        private void EnsureClickOverloaded()
        {
            if (_clickOverloaded)
                return;
            _clickOverloaded = true;
            ItemView.Click += ItemViewOnClick;
        }

        private void ItemViewOnClick(object sender, EventArgs e)
        {
            ExecuteCommandOnItem(Click);
        }

        public ICommand LongClick
        {
            get { return _longClick; }
            set
            {
                _longClick = value;
                if (_longClick != null)
                    EnsureLongClickOverloaded();
            }
        }

        private void EnsureLongClickOverloaded()
        {
            if (_longClickOverloaded)
                return;
            _longClickOverloaded = true;
            ItemView.LongClick += ItemViewOnLongClick;
        }

        private void ItemViewOnLongClick(object sender, View.LongClickEventArgs e)
        {
            ExecuteCommandOnItem(LongClick);
        }

        protected virtual void ExecuteCommandOnItem(ICommand command)
        {
            if (command == null)
                return;

            var item = DataContext;
            if (item == null)
                return;

            if (!command.CanExecute(item))
                return;

            command.Execute(item);
        }

        public void OnAttachedToWindow()
        {
            if (_cachedDataContext != null && DataContext == null)
                DataContext = _cachedDataContext;
        }

        public void OnDetachedFromWindow()
        {
            _cachedDataContext = DataContext;
            DataContext = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ItemView != null)
                {
                    ItemView.Click -= ItemViewOnClick;
                    ItemView.LongClick -= ItemViewOnLongClick;
                }
                    
                _bindingContext.ClearAllBindings();
                _cachedDataContext = null;
            }

            base.Dispose(disposing);
        }
    }
}