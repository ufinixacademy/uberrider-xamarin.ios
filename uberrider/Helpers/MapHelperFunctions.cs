using System;
using System.Net.Http;
using System.Threading.Tasks;
using Acxi.Helpers;
using CoreLocation;
using Google.Maps;
using Newtonsoft.Json;
using Plugin.Connectivity;
using UIKit;

namespace uberrider.Helpers
{
    public class MapHelperFunctions
    {
        MapView map;
        string mapkey;

        public double duration;
        public double distance;
        public string durationString;
        public string distanceString;
        private Marker pickupMarker;
        private bool isRequestingDirection;
        private Marker driverlocationMarker;

        public MapHelperFunctions(string key, MapView gmap)
        {
            mapkey = key;
            map = gmap;
        }

        public async Task<string> GetDirectionJsonAsync(CLLocationCoordinate2D location, CLLocationCoordinate2D destination)
        {
            string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={location.Latitude},{location.Longitude}&destination={destination.Latitude},{destination.Longitude}&mode=driving&key={mapkey}";
            string JsonResponse;

            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            JsonResponse = await client.GetStringAsync(url);

            return JsonResponse;
        }

       public void DrawTripOnMap(string jsonResponse)
        {

            var directionData = JsonConvert.DeserializeObject<DirectionParser>(jsonResponse);
            var points = directionData.routes[0].overview_polyline.points;

            // Draw Polyline on Map
            Google.Maps.Path gmspath = Google.Maps.Path.FromEncodedPath(points);
            Google.Maps.Polyline gmspolyline = Google.Maps.Polyline.FromPath(gmspath);
            gmspolyline.StrokeWidth = 4;
            gmspolyline.StrokeColor = UIColor.FromRGB(6, 144, 193);
            gmspolyline.Geodesic = true;
            gmspolyline.Map = map;

            double startlat = directionData.routes[0].legs[0].start_location.lat;
            double startlng = directionData.routes[0].legs[0].start_location.lng;
            double endlat = directionData.routes[0].legs[0].end_location.lat;
            double endlng = directionData.routes[0].legs[0].end_location.lng;

            pickupMarker = new Marker();
            pickupMarker.Icon = Marker.MarkerImage(UIColor.Green);
            pickupMarker.Title = "Pickup Location";
            pickupMarker.Position = new CLLocationCoordinate2D(startlat, startlng);
            pickupMarker.Map = map;
            pickupMarker.TracksInfoWindowChanges = true;

            driverlocationMarker = new Marker();
            driverlocationMarker.Icon = UIImage.FromBundle("posimarker");
            driverlocationMarker.Title = "Current Location";
            driverlocationMarker.TracksInfoWindowChanges = true;
            driverlocationMarker.Position = new CLLocationCoordinate2D(startlat, startlng);


            var destinationMarker = new Marker()
            {
                Title = "Destination",
                Position = new CLLocationCoordinate2D(endlat, endlng),
                Map = map,
                Icon = Marker.MarkerImage(UIColor.Red)
            };

            Circle circleLocation = new Circle();
            circleLocation.Position = new CLLocationCoordinate2D(startlat, startlng);
            circleLocation.Radius = 8;
            circleLocation.StrokeColor = UIColor.FromRGB(6, 144, 193);
            circleLocation.FillColor = UIColor.FromRGB(6, 144, 193);
            circleLocation.Map = map;

            Circle circleDestination = new Circle();
            circleDestination.Position = new CLLocationCoordinate2D(endlat, endlng);
            circleDestination.Radius = 8;
            circleDestination.StrokeColor = UIColor.FromRGB(6, 144, 193);
            circleDestination.FillColor = UIColor.FromRGB(6, 144, 193);
            circleDestination.Map = map;

            CLLocationCoordinate2D southwest = new CLLocationCoordinate2D(directionData.routes[0].bounds.southwest.lat, directionData.routes[0].bounds.southwest.lng);
            CLLocationCoordinate2D northeast = new CLLocationCoordinate2D(directionData.routes[0].bounds.northeast.lat, directionData.routes[0].bounds.northeast.lng);

            CoordinateBounds bounds = new CoordinateBounds(southwest, northeast);
            CameraUpdate cupdates = CameraUpdate.FitBounds(bounds, 100);
            map.SelectedMarker = pickupMarker;
            map.Animate(cupdates);

            duration = directionData.routes[0].legs[0].duration.value;
            distance = directionData.routes[0].legs[0].distance.value;
            durationString = directionData.routes[0].legs[0].duration.text;
            distanceString = directionData.routes[0].legs[0].distance.text;

        }

        public double EstimateFares()
        {
            double basefare = 5; //USD
            double distnacefare = 1.5; //USD per kilometer
            double timefare = 3; // USD per minute

            double kmfares = (distance / 1000) * distnacefare;
            double minfares = (duration / 60) * timefare;

            double total = kmfares + minfares + basefare;
            double fares = Math.Round(total / 10) * 10;

            return fares;
        }

        public async void UpdateDriverlocationToPickup(CLLocationCoordinate2D firstPosition, CLLocationCoordinate2D secondPosition)
        {
            if (!isRequestingDirection)
            {
                isRequestingDirection = true;

                if (!CrossConnectivity.Current.IsConnected)
                {
                    return;
                }

                string json = await GetDirectionJsonAsync(firstPosition, secondPosition);

                if (!string.IsNullOrEmpty(json))
                {
                    var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);
                    string duration = directionData.routes[0].legs[0].duration.text;
                    pickupMarker.Title = "Pickup Location";
                    pickupMarker.Snippet = "Your Driver is " + duration + " Away";
                    map.SelectedMarker = pickupMarker;
                    isRequestingDirection = false;
                }
            }

            
        }

        public void UpdateDriverArrived()
        {
            pickupMarker.Title = "Pickup Location";
            pickupMarker.Snippet = "your Driver has arrived";
        }

        public async void UpdateLocationToDestination(CLLocationCoordinate2D driverlocation, CLLocationCoordinate2D destination)
        {
            driverlocationMarker.Map = map;
            driverlocationMarker.Position = driverlocation;
            CameraPosition cameraPosition = CameraPosition.FromCamera(driverlocation.Latitude, driverlocation.Longitude, 15);
            map.Animate(cameraPosition);


            if (!isRequestingDirection)
            {
                isRequestingDirection = true;

                if (!CrossConnectivity.Current.IsConnected)
                {
                    return;
                }

                string json = await GetDirectionJsonAsync(driverlocation, destination);

                if (!string.IsNullOrEmpty(json))
                {
                    var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);
                    string duration = directionData.routes[0].legs[0].duration.text;

                    driverlocationMarker.Title = "Current Location";
                    driverlocationMarker.Snippet = "Your Destination is " + duration + " Away";
                    map.SelectedMarker = driverlocationMarker;
                    isRequestingDirection = false;
                }
            }

        }
    }
}
