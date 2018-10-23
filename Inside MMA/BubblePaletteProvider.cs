using System.Windows.Media;
using Inside_MMA.ViewModels;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.PaletteProviders;
using SciChart.Charting.Visuals.RenderableSeries;

namespace Inside_MMA
{
    public class BubblePaletteProvider : IPointMarkerPaletteProvider
    {

        public void OnBeginSeriesDraw(IRenderableSeries series)
        {
            // OnBeginSeriesDraw is a good place to cache dataseries
        }

        public PointPaletteInfo? OverridePointMarker(IRenderableSeries series, int index, IPointMetadata metadata)
        {
            // Called for every data-point to draw
            // you can access data from series.DataSeries.XValues and YValues
            // the index is the index to the data
            //
            // the metadata is an optional object you can pass in to DataSeries
            // remember to cast it!

            var buysell = ((SciChartViewModel.MyMetadata) metadata).Buysell;
            if (buysell == "B")
            {
                return new PointPaletteInfo
                {
                    Stroke = Colors.DarkGreen
                };
            }
            else if (buysell == "S")
            {
                return new PointPaletteInfo
                {
                    Stroke = Colors.DarkRed
                };
            }
            // Else, use series default stroke
            else
            {
                return null; // default line stroke
            }
        }

    }
}