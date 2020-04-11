using AVFoundation;
using CoreGraphics;
using CoreLocation;
using Foundation;
using Google.Maps;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using uberrider.DataModels;
using uberrider.EventListener;
using uberrider.Helpers;
using UIKit;

namespace uberrider
{
    public partial class MainViewController : UIViewController
    {
        //Flags
        int addressRequest = 1;
        // 1 = Set Address as Pickup Location
        // 2 = Set Address as Destination Location

        // Flag track when app is requesting for a trip or is normal
        int tripStage = 0;
         

        CLLocationManager locationManager = new CLLocationManager();
        private string pickupAddress;
        private CLLocationCoordinate2D pickuplocationLatLng;
        private string destinationAddress;
        private CLLocationCoordinate2D destinationLatLng;

        MapHelperFunctions mapHelper;
        private CLLocationCoordinate2D currentLocation;
        private NewTripDetails newTripDetails;
        private CreateRequestEventListener requestListener;
        private FindDriverListener findDriverListener;
        private AcceptedDriver acceptedDriver;
        private bool drawonce;

        public MainViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if(AppDataHelper.thisPlace != null)
            {
                if(addressRequest == 1)
                {
                    pickupAddress = AppDataHelper.thisPlace.result.name;
                    pickupButtonBar.SetTitle(pickupAddress, UIControlState.Normal);

                    // Move Marker to Pickup Point
                    pickuplocationLatLng = new CLLocationCoordinate2D(AppDataHelper.thisPlace.result.geometry.location.lat, AppDataHelper.thisPlace.result.geometry.location.lng);
                    CameraPosition cp = CameraPosition.FromCamera(pickuplocationLatLng.Latitude, pickuplocationLatLng.Longitude, 15);
                    googleMap.Animate(cp);

                    // Set Address request and change center marker color
                    centerMarker.Image = centerMarker.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    centerMarker.TintColor = UIColor.FromRGBA(64, 183, 76, 255);
                }
                else if (addressRequest == 2)
                {
                    destinationAddress = AppDataHelper.thisPlace.result.name;
                    destinationButtonBar.SetTitle(destinationAddress, UIControlState.Normal);

                    // Move Marker to Pickup Point
                    destinationLatLng = new CLLocationCoordinate2D(AppDataHelper.thisPlace.result.geometry.location.lat, AppDataHelper.thisPlace.result.geometry.location.lng);
                    CameraPosition cp = CameraPosition.FromCamera(destinationLatLng.Latitude, destinationLatLng.Longitude, 15);
                    googleMap.Animate(cp);

                    // Set Address request and change center marker color
                    centerMarker.Image = centerMarker.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    centerMarker.TintColor = UIColor.FromRGBA(251, 24, 24, 255);
                    favouritePlacesButton.Hidden = true;
                    doneButton.Hidden = false;
                }
            }
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ApplyStylesToViews();

            locationManager.RequestAlwaysAuthorization();
            locationManager.RequestWhenInUseAuthorization();
            locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            locationManager.LocationsUpdated += LocationManager_LocationsUpdated;
            locationManager.StartUpdatingLocation();

            mapHelper = new MapHelperFunctions("AIzaSyAZQBaY-ugQuwCWr4NkD-bybK7urElvNyY", googleMap);

            pickupButtonBar.TouchUpInside += PickupButtonBar_TouchUpInside;
            destinationButtonBar.TouchUpInside += DestinationButtonBar_TouchUpInside;
            doneButton.TouchUpInside += DoneButton_TouchUpInside;
            requestCabButton.TouchUpInside += RequestCabButton_TouchUpInside;
            cancelRequestButton.TouchUpInside += CancelRequestButton_TouchUpInside;

            menuButton.UserInteractionEnabled = true;
            menuButton.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                if(tripStage == 0)
                {
                    // display our side menu
                }
                else if (tripStage == 1)
                {
                    UIView.Animate(0.2, HideTripDetailsView);
                    ClearTripOnMap();
                }
            }));

        }

        private void CancelRequestButton_TouchUpInside(object sender, EventArgs e)
        {
           if(requestListener != null)
            {
                requestListener.CancelRequest();
                requestListener = null;
                requestCabView.Hidden = true;
                overlay.Hidden = true;
            }
        }

        private void RequestCabButton_TouchUpInside(object sender, EventArgs e)
        {

            newTripDetails = new NewTripDetails();
            newTripDetails.DestinationAddress = destinationAddress;
            newTripDetails.PickupAddress = pickupAddress;
            newTripDetails.PickupLat = pickuplocationLatLng.Latitude;
            newTripDetails.PickupLng = pickuplocationLatLng.Longitude;
            newTripDetails.DestinationLat = destinationLatLng.Latitude;
            newTripDetails.DestinationLng = destinationLatLng.Longitude;
            newTripDetails.DistanceString = mapHelper.distanceString;
            newTripDetails.DistanceValue = mapHelper.distance;
            newTripDetails.DurationString = mapHelper.durationString;
            newTripDetails.DurationValue = mapHelper.duration;
            newTripDetails.EstimateFare = mapHelper.EstimateFares();
            newTripDetails.TimeStamp = DateTime.Now;
            newTripDetails.PaymentMethod = "cash";

            estimateFaresRequestText.Text = "$" + mapHelper.EstimateFares().ToString();

            overlay.Hidden = false;
            requestCabView.Hidden = false;

            requestListener = new CreateRequestEventListener(newTripDetails);
            requestListener.NoDriverAcceptedRequest += RequestListener_NoDriverAcceptedRequest;
            requestListener.DriverAccepted += RequestListener_DriverAccepted;
            requestListener.TripUpdates += RequestListener_TripUpdates;
            requestListener.CreateRequest();

            findDriverListener = new FindDriverListener(pickuplocationLatLng);
            findDriverListener.FindDrivers();
            findDriverListener.DriversFound += FindDriverListener_DriversFound;
            findDriverListener.DriverNotFound += FindDriverListener_DriverNotFound;


        }

        private void RequestListener_TripUpdates(object sender, CreateRequestEventListener.TripUpdatesEventArgs e)
        {
           if(e.Status == "accepted")
            {
                tripStatusText.Text = "Coming";
                mapHelper.UpdateDriverlocationToPickup(e.DriverLocation, pickuplocationLatLng);
            }
           else if(e.Status == "arrived")
            {
                tripStatusText.Text = "Arrived";
                mapHelper.UpdateDriverArrived();
                //
                AVAudioPlayer player = AVAudioPlayer.FromUrl(NSUrl.FromFilename("Sounds/alertios.aiff"));
                player.PrepareToPlay();
                player.Play();
            }
           else if(e.Status == "ontrip")
            {
                tripStatusText.Text = "On Trip";
                mapHelper.UpdateLocationToDestination(e.DriverLocation, destinationLatLng);
            }
           else if(e.Status == "ended")
            {
                faresAmountText.Text = "$" + e.Fares.ToString();
                overlay.Hidden = false;
                makePaymentView.Hidden = false;
                UIView.Animate(0.2, HideTripControlPanel);
                makePaymentButton.TouchUpInside += (i, args) =>
                {
                    overlay.Hidden = true;
                    makePaymentView.Hidden = true;
                    ClearTripOnMap();
                };
            }
        }

        private void RequestListener_DriverAccepted(object sender, CreateRequestEventListener.DriverAcceptedEventArgs e)
        {
            acceptedDriver = e.acceptedDriver;

            if (!requestCabView.Hidden)
            {
                overlay.Hidden = true;
                requestCabView.Hidden = true;
            }

            driverNameText.Text = acceptedDriver.Fullname;
            tripStatusText.Text = "Coming";
            UIView.Animate(0.2, HideTripDetailsView);
            UIView.Animate(0.2, ShowTripControlPanel);
        }

        private void RequestListener_NoDriverAcceptedRequest(object sender, EventArgs e)
        {

            InvokeOnMainThread(() =>
            {

                if (!requestCabView.Hidden)
                {
                    //Hide Views
                    overlay.Hidden = true;
                    requestCabView.Hidden = true;
                    requestListener.CancelRequestOnTimeout();

                    // Display Alert
                    string alertstring = "No available driver accepted your request";
                    var alert = UIAlertController.Create("Alert", alertstring, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }

            });

           
        }

        private void FindDriverListener_DriverNotFound(object sender, EventArgs e)
        {
            // No driver
        }

        private void FindDriverListener_DriversFound(object sender, FindDriverListener.DriversFoundEventArgs e)
        {
           if(requestListener != null)
            {
                requestListener.NotifyDriver(e.Drivers);
            }
        }

        private async void DoneButton_TouchUpInside(object sender, EventArgs e)
        {
            if(CrossConnectivity.Current.IsConnected == true)
            {
                doneButton.SetTitle("Please wait...", UIControlState.Normal);
                doneButton.UserInteractionEnabled = false;

                string directionJson = await mapHelper.GetDirectionJsonAsync(pickuplocationLatLng, destinationLatLng);

                if (!string.IsNullOrEmpty(directionJson))
                {
                    mapHelper.DrawTripOnMap(directionJson);

                    //Set estimated fares and time
                    faresText.Text = "$" + (mapHelper.EstimateFares() - 10).ToString() + " - " + (mapHelper.EstimateFares() + 10).ToString();
                    ETAText.Text = mapHelper.durationString;

                    UIView.Animate(0.2, ShowTripDetailsView);
                    TripDrawnOnMap();

                    doneButton.SetTitle("Done", UIControlState.Normal);
                    doneButton.UserInteractionEnabled = true;
                }
               
            }
            else
            {
                var alert = UIAlertController.Create("Alert", "No intenernet connectivity", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
           
        }


        void ShowTripDetailsView()
        {
            CGRect frame = tripDetailsView.Frame;
            frame.Y = View.Bounds.GetMaxY() - tripDetailsView.Frame.Size.Height;
            tripDetailsView.Frame = frame;
        }

        void HideTripDetailsView()
        {
            CGRect frame = tripDetailsView.Frame;
            frame.Y = View.Bounds.GetMaxY();
            tripDetailsView.Frame = frame;
        }

        void TripDrawnOnMap()
        {
            centerMarker.Hidden = true;
            menuButton.Image = UIImage.FromBundle("arrowback");
            destinationButtonBar.UserInteractionEnabled = false;
            pickupButtonBar.UserInteractionEnabled = false;

            tripStage = 1;

        }

        void ShowTripControlPanel()
        {
            CGRect frame = tripControlPanel.Frame;
            frame.Y = View.Bounds.GetMaxY() - tripControlPanel.Frame.Size.Height;
            tripControlPanel.Frame = frame;
        }

        void HideTripControlPanel()
        {
            CGRect frame = tripControlPanel.Frame;
            frame.Y = View.Bounds.GetMaxY();
            tripControlPanel.Frame = frame;
        }

       async void ClearTripOnMap()
        {
            destinationButtonBar.UserInteractionEnabled = true;
            pickupButtonBar.UserInteractionEnabled = true;
            centerMarker.Hidden = false;

            googleMap.Clear();
            destinationButtonBar.SetTitle("Set destination", UIControlState.Normal);
          

            doneButton.Hidden = true;
            favouritePlacesButton.Hidden = false;

            menuButton.Image = UIImage.FromBundle("menu_small");
            tripStage = 0;

            centerMarker.Image = centerMarker.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            centerMarker.TintColor = UIColor.FromRGBA(64, 183, 76, 255);

            CameraPosition cameraPosition = CameraPosition.FromCamera(currentLocation, 15);
            googleMap.Animate(cameraPosition);

            pickupAddress = await FindCordinateAddress(currentLocation);
            pickupButtonBar.SetTitle(pickupAddress, UIControlState.Normal);
            pickuplocationLatLng = currentLocation;
        }

        private void DestinationButtonBar_TouchUpInside(object sender, EventArgs e)
        {
            addressRequest = 2;
            FindPlacesViewController findPlacesViewController = this.Storyboard.InstantiateViewController("FindPlacesViewController") as FindPlacesViewController;
            findPlacesViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(findPlacesViewController, true, null);
        }

        private void PickupButtonBar_TouchUpInside(object sender, EventArgs e)
        {
            addressRequest = 1;
            FindPlacesViewController findPlacesViewController = this.Storyboard.InstantiateViewController("FindPlacesViewController") as FindPlacesViewController;
            findPlacesViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            PresentViewController(findPlacesViewController, true, null);
        }

        private async void LocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {


            //currentLocation = new CLLocationCoordinate2D(e.Locations[0].Coordinate.Latitude, e.Locations[0].Coordinate.Longitude);

            //CameraPosition cameraPosition = CameraPosition.FromCamera(e.Locations[0].Coordinate, 15);
            //googleMap.Animate(cameraPosition);

            //pickuplocationLatLng = new CLLocationCoordinate2D(e.Locations[0].Coordinate.Latitude, e.Locations[0].Coordinate.Longitude);
            //pickupButtonBar.SetTitle("Fetching Address...", UIControlState.Normal);

            //pickupAddress = await FindCordinateAddress(pickuplocationLatLng);
            //pickupButtonBar.SetTitle(pickupAddress, UIControlState.Normal);



            // Updated to fix on physical devces.
            currentLocation = new CLLocationCoordinate2D(e.Locations[0].Coordinate.Latitude, e.Locations[0].Coordinate.Longitude);
          
            if (!drawonce)
            {
                CameraPosition cameraPosition = CameraPosition.FromCamera(e.Locations[0].Coordinate, 15);
                googleMap.Animate(cameraPosition);
                drawonce = true;

                pickuplocationLatLng = new CLLocationCoordinate2D(e.Locations[0].Coordinate.Latitude, e.Locations[0].Coordinate.Longitude);
                pickupButtonBar.SetTitle("Fetching Address...", UIControlState.Normal);

                pickupAddress = await FindCordinateAddress(pickuplocationLatLng);
                pickupButtonBar.SetTitle(pickupAddress, UIControlState.Normal);

            }


        }

        async Task<string> FindCordinateAddress(CLLocationCoordinate2D position)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={position.Latitude.ToString()},{position.Longitude}&key=AIzaSyAZQBaY-ugQuwCWr4NkD-bybK7urElvNyY";

            string jsonResponse = "";
            string placeAddress = "";

            // Check internet connectivity

            if (!CrossConnectivity.Current.IsConnected)
            {
                placeAddress = "No internet connectvity";
            }
            else
            {
                var handler = new HttpClientHandler();
                HttpClient client = new HttpClient(handler);
                jsonResponse = await client.GetStringAsync(url);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    var resultObject = JObject.Parse(jsonResponse);
                    string status = resultObject["status"].ToString();

                    if(status.ToLower() == "ok")
                    {
                        placeAddress = resultObject["results"][0]["formatted_address"].ToString();
                    }
                }
                
            }

            return placeAddress;

        }


        void ApplyStylesToViews()
        {
            locationBar.Layer.ShadowOpacity = 0.2f;
            locationBar.Layer.ShadowRadius = 5;
            locationBar.Layer.BorderWidth = 1;
            locationBar.Layer.BorderColor = UIColor.FromRGB(193, 193, 193).CGColor;

            favouritePlacesButton.Layer.ShadowOpacity = 0.2f;
            favouritePlacesButton.Layer.ShadowRadius = 1;
        }
    }
}
