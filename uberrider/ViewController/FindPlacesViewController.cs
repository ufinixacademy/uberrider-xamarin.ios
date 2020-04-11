using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using uberrider.DataModels;
using uberrider.Helpers;
using UIKit;

namespace uberrider
{
    public partial class FindPlacesViewController : UIViewController
    {

        List<Prediction> predictionList;

        public FindPlacesViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            searchLoader.Hidden = true;
            searchbox.BecomeFirstResponder();
            searchbox.TextChanged += Searchbox_TextChanged;
            searchbox.CancelButtonClicked += Searchbox_CancelButtonClicked;
        }

        private void Searchbox_CancelButtonClicked(object sender, EventArgs e)
        {
            DismissModalViewController(true);
        }

        private async void Searchbox_TextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
           if(e.SearchText.Length > 1)
            {
                searchLoader.Hidden = false;
                string url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={e.SearchText}&key=AIzaSyAZQBaY-ugQuwCWr4NkD-bybK7urElvNyY&sessiontoken=123254251&components=country:ng";
                var handler = new HttpClientHandler();
                HttpClient client = new HttpClient(handler);
                string result = await client.GetStringAsync(url);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultObject = JObject.Parse(result);
                    var predictionString = resultObject["predictions"].ToString();
                    predictionList = JsonConvert.DeserializeObject<List<Prediction>>(predictionString);

                    Console.WriteLine("Prediction Count = " + predictionList.Count);
                    placesTableView.Hidden = false;
                    searchLoader.Hidden = true;
                    SetupTable();
                }
              
            }
        }


        void SetupTable()
        {
            placesDatasource datasource = new placesDatasource(predictionList);
            placesTableView.Source = datasource;
            datasource.OnClicked += Datasource_OnClicked;
            placesTableView.ReloadData();
        }

        private async void Datasource_OnClicked(object sender, placesDatasource.CellClickedEventArgs e)
        {
            searchLoader.Hidden = false;
            string place_id = e.thisPrediction.place_id;
            Console.WriteLine("Place ID = " + place_id);
            searchbox.Text = e.thisPrediction.structured_formatting.main_text;

            string url = $"https://maps.googleapis.com/maps/api/place/details/json?placeid={place_id}&key=AIzaSyDeM71ofK0N0kRHM-kHHg3ABK2yxWGcbQY";
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);
            searchLoader.Hidden = true;


            if (!string.IsNullOrEmpty(result))
            {
                var thisPlace = JsonConvert.DeserializeObject<PlaceAttribute>(result);
                AppDataHelper.thisPlace = thisPlace;
                DismissViewController(true, null);
            }
        }
    }
}