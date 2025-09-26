using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using MoneyTracker.Application.DTOs;
using MoneyTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoneyTracker.Presentation.Adapters
{
    /// <summary>
    /// Adapter used to display transactions in a RecyclerView.
    /// </summary>
    public class TransactionAdapter : RecyclerView.Adapter
    {
        private List<TransactionDto> _transactions = new();

        // Events for click handling
        public event Action<TransactionDto>? ItemClick;
        public event Action<TransactionDto>? EditClick;
        public event Action<TransactionDto>? DeleteClick;

        public override int ItemCount => _transactions.Count;

        /// <summary>
        /// Updates the list of transactions.
        /// </summary>
        public void UpdateTransactions(IEnumerable<TransactionDto> transactions)
        {
            _transactions = transactions?.ToList() ?? new List<TransactionDto>();
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                ?.Inflate(Resource.Layout.item_transaction, parent, false);

            return new TransactionViewHolder(view!, this);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is TransactionViewHolder transactionHolder && position < _transactions.Count)
            {
                transactionHolder.Bind(_transactions[position]);
            }
        }

        /// <summary>
        /// ViewHolder for each transaction item
        /// </summary>
        private class TransactionViewHolder : RecyclerView.ViewHolder
        {
            private readonly TextView _descriptionText;
            private readonly TextView _amountText;
            private readonly TextView _categoryText;
            private readonly TextView _dateText;
            private readonly View _categoryColor;
            private readonly View _categoryColorIndicator;
            private readonly ImageView _typeIcon;
            private readonly ImageButton _editButton;
            private readonly ImageButton _deleteButton;
            private readonly TransactionAdapter _adapter;

            private TransactionDto? _currentTransaction;

            public TransactionViewHolder(View itemView, TransactionAdapter adapter) : base(itemView)
            {
                _adapter = adapter;
                

                // Initialize views
                _descriptionText = itemView.FindViewById<TextView>(Resource.Id.text_description)!;
                _amountText = itemView.FindViewById<TextView>(Resource.Id.text_amount)!;
                _categoryText = itemView.FindViewById<TextView>(Resource.Id.text_category)!;
                _dateText = itemView.FindViewById<TextView>(Resource.Id.text_date)!;
                _categoryColorIndicator = itemView.FindViewById<View>(Resource.Id.view_category_color)!;
                _typeIcon = itemView.FindViewById<ImageView>(Resource.Id.icon_type)!;
                _editButton = itemView.FindViewById<ImageButton>(Resource.Id.button_edit)!;
                _deleteButton = itemView.FindViewById<ImageButton>(Resource.Id.button_delete)!;

                // Configure event handlers
                itemView.Click += (s, e) =>
                {
                    if (_currentTransaction != null)
                        _adapter.ItemClick?.Invoke(_currentTransaction);
                };

                _editButton.Click += (s, e) =>
                {
                    if (_currentTransaction != null)
                        _adapter.EditClick?.Invoke(_currentTransaction);
                };

                _deleteButton.Click += (s, e) =>
                {
                    if (_currentTransaction != null)
                        _adapter.DeleteClick?.Invoke(_currentTransaction);
                };
            }

            /// <summary>
            /// Binds transaction data to the views
            /// </summary>
            public void Bind(TransactionDto transaction)
            {
                _currentTransaction = transaction;

                // Basic data
                _descriptionText.Text = transaction.Description;
                _amountText.Text = transaction.FormattedAmount;
                _categoryText.Text = transaction.CategoryName;
                _dateText.Text = transaction.FormattedDate;

                // Category color
                try
                {
                    var color = Android.Graphics.Color.ParseColor(transaction.CategoryColor);
                    _categoryColorIndicator.SetBackgroundColor(color);
                }
                catch
                {
                    _categoryColorIndicator.SetBackgroundColor(Android.Graphics.Color.Gray);
                }

                // Icon and color based on type
                if (transaction.Type == TransactionType.Income)
                {
                    _typeIcon.SetImageResource(Android.Resource.Drawable.ArrowUpFloat);
                    _amountText.SetTextColor(Android.Graphics.Color.Green);
                }
                else
                {
                    _typeIcon.SetImageResource(Android.Resource.Drawable.ArrowDownFloat);
                    _amountText.SetTextColor(Android.Graphics.Color.Red);
                }

                // Show notes when available
                if (!string.IsNullOrWhiteSpace(transaction.Notes))
                {
                    _descriptionText.Text = $"{transaction.Description}\n{transaction.Notes}";
                }
            }
        }
    }
}