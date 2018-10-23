using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SciChart.Charting.Common.Extensions;

namespace Inside_MMA
{
    public static class DataGridExtensions
    {
        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int columnIndex = 0)
        {
            if (row == null) return null;

            var presenter = row.FindVisualChild<DataGridCellsPresenter>();
            if (presenter == null) return null;

            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (cell != null) return cell;

            // now try to bring into view and retreive the cell
            grid.ScrollIntoView(row, grid.Columns[columnIndex]);
            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);

            return cell;
        }
    }
}