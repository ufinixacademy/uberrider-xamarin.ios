using System;
using System.Collections.Generic;
using Foundation;
using uberrider.DataModels;
using UIKit;

namespace uberrider
{
    internal class placesDatasource : UITableViewSource
    {
        private List<Prediction> predictionList;

        public event EventHandler<CellClickedEventArgs> OnClicked;

        public class CellClickedEventArgs : EventArgs
        {
            public Prediction thisPrediction { get; set; }
        }

        public placesDatasource(List<Prediction> predictionList)
        {
            this.predictionList = predictionList;
        }

      
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (PlaceCell)tableView.DequeueReusableCell("placecell", indexPath);
            var prediction = predictionList[indexPath.Row];
            cell.UpdateCell(prediction);
            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedPrediction = predictionList[indexPath.Row];
            OnClicked?.Invoke(this, new CellClickedEventArgs { thisPrediction = selectedPrediction });
        }


        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return predictionList.Count;
           
        }

    }
}