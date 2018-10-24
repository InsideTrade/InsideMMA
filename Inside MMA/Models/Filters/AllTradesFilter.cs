using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using InsideDB.Annotations;
using Newtonsoft.Json;
using SciChart.Core.Extensions;

namespace Inside_MMA.Models.Filters
{
    /// <summary>
    /// Осуществляет фильтрацию CollectionViewSource представления AllTrades.
    /// </summary>
    public class AllTradesFilter : INotifyPropertyChanged
    {
        private TimeSpan? _fromTime;
        private TimeSpan? _toTime;
        private bool _isTimeFilterActive;
        private bool _showAll = true;
        private bool _showBuy;
        private bool _showSell;
        private int _filterSize;
        private bool _isSizeFilterActive;
        private int _selectSize;
        private bool _isSelectingSize;
        private string _selectPrice = "0";
        private bool _isSelectingPrice;
        private bool _isMiOnly;
        private int _minOiDelta;
        private int _minEatenSize = 1000;
        private bool _filtersApplied;
        private int _eatenSize = 2000;
        private string _hideSize;
        private bool _isHideSize;

        public AllTradesFilter()
        {
            // to be called by json cast
        }

        public AllTradesFilter(CollectionViewSource items, AllTradesFilter filter)
        {
            Items = items;
            if (filter == null) return;
            FromTime = filter.FromTime;
            ToTime = filter.ToTime;
            IsTimeFilterActive = filter.IsTimeFilterActive;
            ShowAll = filter.ShowAll;
            ShowBuy = filter.ShowBuy;
            ShowSell = filter.ShowSell;
            FilterSize = filter.FilterSize;
            IsSizeFilterActive = filter.IsSizeFilterActive;
            SelectSize = filter.SelectSize;
            IsSelectingSize = filter.IsSelectingSize;
            SelectPrice = filter.SelectPrice;
            IsSelectingPrice = filter.IsSelectingPrice;
            IsMiOnly = filter.IsMiOnly;
            MinEatenSize = filter.MinEatenSize;
            MinOiDelta = filter.MinOiDelta;
            EatenSize = filter.EatenSize;
            HideSize = filter.HideSize;
            IsHideSize = filter.IsHideSize;
        }

        [JsonIgnore]
        public CollectionViewSource Items { get; set; }

        public TimeSpan? FromTime
        {
            get { return _fromTime; }
            set
            {
                if (value.Equals(_fromTime)) return;
                _fromTime = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;

                Items.Filter -= FromTimeFilter;
                Items.Filter += FromTimeFilter;
            }
        }

        public TimeSpan? ToTime
        {
            get { return _toTime; }
            set
            {
                if (value.Equals(_toTime)) return;
                _toTime = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= ToTimeFilter;
                Items.Filter += ToTimeFilter;
            }
        }

        public bool IsTimeFilterActive
        {
            get { return _isTimeFilterActive; }
            set
            {
                if (value == _isTimeFilterActive) return;
                _isTimeFilterActive = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= FromTimeFilter;
                Items.Filter -= ToTimeFilter;
                if (!_isTimeFilterActive)
                {
                    Items.Filter -= FromTimeFilter;
                    Items.Filter -= ToTimeFilter;
                    return;
                }

                if (_fromTime != null)
                    Items.Filter += FromTimeFilter;
                if (_toTime != null)
                    Items.Filter += ToTimeFilter;
            }
        }

        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                if (value == _showAll) return;
                _showAll = value;
                OnPropertyChangedAndCheck();

                if (!_showAll) return;
                if (Items == null) return;
                Items.Filter -= BuyFilter;
                Items.Filter -= SellFilter;
            }
        }

        public bool ShowBuy
        {
            get { return _showBuy; }
            set
            {
                if (value == _showBuy) return;
                _showBuy = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;

                Items.Filter -= BuyFilter;
                Items.Filter -= SellFilter;
                if (_showBuy)
                    Items.Filter += BuyFilter;
                else
                    Items.Filter -= BuyFilter;
            }
        }

        public bool ShowSell
        {
            get { return _showSell; }
            set
            {
                if (value == _showSell) return;
                _showSell = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;

                Items.Filter -= BuyFilter;
                Items.Filter -= SellFilter;
                if (_showSell)
                    Items.Filter += SellFilter;
                else
                    Items.Filter -= SellFilter;
            }
        }

        public int FilterSize
        {
            get { return _filterSize; }
            set
            {
                if (value == _filterSize) return;
                _filterSize = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;

                Items.Filter -= SizeFilter;
                Items.Filter += SizeFilter;
            }
        }

        public bool IsSizeFilterActive
        {
            get { return _isSizeFilterActive; }
            set
            {
                if (value == _isSizeFilterActive) return;
                _isSizeFilterActive = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= SizeFilter;

                if (_isSizeFilterActive)
                    Items.Filter += SizeFilter;
                else
                    Items.Filter -= SizeFilter;
            }
        }

        public int SelectSize
        {
            get { return _selectSize; }
            set
            {
                if (value.Equals(_selectSize)) return;
                _selectSize = value;
                OnPropertyChanged();

                if (Items == null) return;

                Items.Filter -= SizeSelect;
                Items.Filter += SizeSelect;
            }
        }

        public bool IsSelectingSize
        {
            get { return _isSelectingSize; }
            set
            {
                if (value == _isSelectingSize) return;
                _isSelectingSize = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= SizeSelect;
                if (_isSelectingSize)
                    Items.Filter += SizeSelect;
                else
                    Items.Filter -= SizeSelect;
            }
        }

        public string SelectPrice
        {
            get { return _selectPrice; }
            set
            {
                if (value.Equals(_selectPrice)) return;
                _selectPrice = value;
                OnPropertyChanged();

                if (Items == null) return;

                Items.Filter -= SizeSelect;
                Items.Filter += SizeSelect;
            }
        }

        public bool IsSelectingPrice
        {
            get { return _isSelectingPrice; }
            set
            {
                if (value == _isSelectingPrice) return;
                _isSelectingPrice = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= PriceSelect;

                if (_isSelectingPrice)
                    Items.Filter += PriceSelect;
                else
                    Items.Filter -= PriceSelect;
            }
        }

        public bool IsMiOnly
        {
            get { return _isMiOnly; }
            set
            {
                if (value == _isMiOnly) return;
                _isMiOnly = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= MiOnlyFilter;

                if (_isMiOnly)
                    Items.Filter += MiOnlyFilter;
                else
                    Items.Filter -= MiOnlyFilter;
            }
        }

        public int MinEatenSize
        {
            get { return _minEatenSize; }
            set
            {
                if (value == _minEatenSize) return;
                _minEatenSize = value;
                OnPropertyChanged();
            }
        }

        public int MinOiDelta
        {
            get { return _minOiDelta; }
            set
            {
                if (value == _minOiDelta) return;
                _minOiDelta = value;
                OnPropertyChanged();
            }
        }

        public int EatenSize
        {
            get { return _eatenSize; }
            set
            {
                if (value == _eatenSize) return;
                _eatenSize = value;
                OnPropertyChanged();
            }
        }

        public bool FiltersApplied
        {
            get { return _filtersApplied; }
            set
            {
                _filtersApplied = value;
                OnPropertyChanged();
            }
        }

        public string HideSize
        {
            get => _hideSize;
            set
            {
                _hideSize = value;
                OnPropertyChangedAndCheck();
            }
        }

        public bool IsHideSize
        {
            get { return _isHideSize; }
            set
            {
                if (value == _isHideSize) return;
                _isHideSize = value;
                OnPropertyChangedAndCheck();

                if (Items == null) return;
                Items.Filter -= HideSizeFilter;

                if (_isHideSize)
                    Items.Filter += HideSizeFilter;
                else
                    Items.Filter -= HideSizeFilter;
            }
        }


        private void PriceSelect(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (double.Parse(SelectPrice) != 0 && src.Price != double.Parse(SelectPrice))
                e.Accepted = false;
        }

        private void SizeFilter(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;           
            else if (src.Quantity < _filterSize)
                e.Accepted = false;
        }

        private void SizeSelect(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (_selectSize != 0 && src.Quantity != _selectSize)
                e.Accepted = false;
        }

        private void MiOnlyFilter(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (!src.IsMul)
                e.Accepted = false;
        }

        private void BuyFilter(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (src.Buysell != "B")
                e.Accepted = false;
        }

        private void SellFilter(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (src.Buysell != "S")
                e.Accepted = false;
        }

        private void FromTimeFilter(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (DateTime.Parse(src.Time).TimeOfDay < _fromTime)
                e.Accepted = false;
        }

        private void ToTimeFilter(object sender, FilterEventArgs e)
        {
            var src = e.Item as TradeItem;
            if (src == null)
                e.Accepted = false;
            else if (DateTime.Parse(src.Time).TimeOfDay > _toTime)
                e.Accepted = false;
        }

        private void HideSizeFilter(object sender, FilterEventArgs e)
        {
            try
            {
                var src = e.Item as TradeItem;
                int[] arr = _hideSize.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                if (src == null)
                    e.Accepted = false;
                //else if (src.Quantity == arr[])
                //    e.Accepted = false;
                for (int i = 0; i < arr.Count(); i++)
                {
                    if (src.Quantity == arr[i])
                        e.Accepted = false;
                }
            }
            catch(Exception)
            {

            }
        }
        
        /// <summary>
        /// Check if any filter checkbox or 'Show all' radio button are checked.
        /// </summary>
        private void CheckIfFiltersApplied()
        {
            FiltersApplied = !ShowAll || IsSizeFilterActive || IsSelectingPrice || IsSelectingSize || IsTimeFilterActive || IsMiOnly || IsHideSize;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChangedAndCheck([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            CheckIfFiltersApplied();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
