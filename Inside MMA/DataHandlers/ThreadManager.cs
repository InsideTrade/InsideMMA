using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using Inside_MMA.Models;

namespace Inside_MMA.DataHandlers
{
    public static class ThreadManager
    {
        public static Thread AllTradesThread;
        private static readonly object Lock = new object();

        public static void CreateThreads()
        {
            AllTradesThread = new Thread(() =>
            {
                //Create our context, and install it:
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(
                        Dispatcher.CurrentDispatcher));
                // Start the Dispatcher Processing
                Dispatcher.Run();
            });
            AllTradesThread.Name = "AllTrades thread";
            // Set the apartment state
            AllTradesThread.SetApartmentState(ApartmentState.STA);
            // Make the thread a background thread
            AllTradesThread.IsBackground = true;
            // Start the thread
            AllTradesThread.Start();
        }

        public static ObservableCollection<TradeItem> CreateAllTradesCollection()
        {
            ObservableCollection<TradeItem> collection = new ObservableCollection<TradeItem>();
            BindingOperations.EnableCollectionSynchronization(collection, Lock);
            //Dispatcher.FromThread(AllTradesThread)?.Invoke(() =>
            //{
            //    collection = new ObservableCollection<TradeItem>();
            //    BindingOperations.EnableCollectionSynchronization(collection, Lock);
            //});
            return collection;
        }
    }
}