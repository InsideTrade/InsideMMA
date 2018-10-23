using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inside_MMA
{
    public static class CloseableExtensions
    {
        public static void InitCloseAction<TViewModel>(this TViewModel viewModel, Action action)
            where TViewModel : class, ICloseable
        {
            viewModel.CloseAction = action;
        }
    }
    public interface ICloseable
    {
        Action CloseAction { set; get; }
    }
}
