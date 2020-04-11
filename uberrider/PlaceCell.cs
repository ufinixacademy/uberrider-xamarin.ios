using Foundation;
using System;
using uberrider.DataModels;
using UIKit;

namespace uberrider
{
    public partial class PlaceCell : UITableViewCell
    {
        public PlaceCell (IntPtr handle) : base (handle)
        {
        }

        internal void UpdateCell(Prediction prediction)
        {
            placeTitleText.Text = prediction.structured_formatting.main_text;
            placeAddressText.Text = prediction.structured_formatting.secondary_text;
        }
    }
}