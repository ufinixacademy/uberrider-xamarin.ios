using System;
using System.Collections.Generic;
using CoreLocation;
using Firebase.Database;
using Foundation;
using uberrider.DataModels;
using uberrider.Helpers;

namespace uberrider.EventListener
{
    public class CreateRequestEventListener
    {
        NewTripDetails newTripDetails;
        DatabaseReference requestReference;
        List<AvailableDriver> availableDrivers;
        AvailableDriver selectedDriver;
        DatabaseReference notifyDriverRef;

        //Timer
        System.Timers.Timer RequestTimer = new System.Timers.Timer();
        int timerCounter = 0;
        private bool isDriverAccepted;
        private string status;
        private double fares;



        // Events

        public class DriverAcceptedEventArgs : EventArgs
        {
            public AcceptedDriver acceptedDriver { get; set; }
        }


        public class TripUpdatesEventArgs : EventArgs
        {
            public CLLocationCoordinate2D DriverLocation { get; set; }

            public string Status { get; set; }

            public double Fares { get; set; }
        }

        public event EventHandler<TripUpdatesEventArgs> TripUpdates;

        public event EventHandler<DriverAcceptedEventArgs> DriverAccepted;

        public event EventHandler NoDriverAcceptedRequest;

        public CreateRequestEventListener(NewTripDetails tripDetails)
        {
            newTripDetails = tripDetails;
            RequestTimer.Interval = 1000;
            RequestTimer.Elapsed += RequestTimer_Elapsed;
        }

        private void RequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerCounter++;

            if(timerCounter == 10)
            {
                if (!isDriverAccepted)
                {
                    timerCounter = 0;
                    DatabaseReference timeoutdriverRef = Database.DefaultInstance.GetRootReference().GetChild("driversAvailable/" + selectedDriver.ID + "/ride_id");
                    timeoutdriverRef.SetValue<NSString>((NSString)"timeout");

                    // Pass down request to another driver
                    if (availableDrivers != null)
                    {
                        NotifyDriver(availableDrivers);
                    }
                    else
                    {
                        RequestTimer.Enabled = false;
                        NoDriverAcceptedRequest?.Invoke(this, new EventArgs());
                    }
                }

            }
        }

        public void CreateRequest()
        {
            requestReference = Database.DefaultInstance.GetRootReference().GetChild("rideRequest").GetChildByAutoId();
            newTripDetails.RideID = requestReference.Key;

            var locationNode = new NSDictionary
                (
                "latitude", newTripDetails.PickupLat.ToString(),
                "longitude", newTripDetails.PickupLng.ToString()
                );

            var destinationNode = new NSDictionary
                (
                "latitude", newTripDetails.DestinationLat.ToString(),
                "longitude", newTripDetails.DestinationLng.ToString()
                );

            var tripDetailsNode = new NSDictionary
                (
                "location", locationNode,
                "destination", destinationNode,
                "destination_address", newTripDetails.DestinationAddress,
                "pickup_address", newTripDetails.PickupAddress,
                "rider_id", AppDataHelper.GetUserID(),
                "rider_name", AppDataHelper.GetFullName(),
                "rider_phone", AppDataHelper.GetPhone(),
                "created_at", newTripDetails.TimeStamp.ToString()
                );

            requestReference.SetValue<NSDictionary>(tripDetailsNode);
            requestReference.ObserveEvent(DataEventType.Value, (DataSnapshot snapshot) =>
            {
                //Driver has been assigned
                if(snapshot.GetChildSnapshot("driver_id").GetValue<NSObject>() != NSNull.Null)
                {
                    if(snapshot.GetChildSnapshot("driver_id").GetValue<NSObject>().ToString() != "waiting")
                    {
                        if (!isDriverAccepted)
                        {
                            // Fetch Driver Details from the snapshot
                            AcceptedDriver acceptedDriver = new AcceptedDriver();
                            acceptedDriver.ID = snapshot.GetChildSnapshot("driver_id").GetValue<NSObject>().ToString();
                            acceptedDriver.Fullname = snapshot.GetChildSnapshot("driver_name").GetValue<NSObject>().ToString();
                            acceptedDriver.phone = snapshot.GetChildSnapshot("driver_phone").GetValue<NSObject>().ToString();

                            isDriverAccepted = true;
                            DriverAccepted?.Invoke(this, new DriverAcceptedEventArgs { acceptedDriver = acceptedDriver });
                        }


                        // Gets Trip Status

                        if(snapshot.GetChildSnapshot("status").GetValue<NSObject>() != NSNull.Null)
                        {
                            status = snapshot.GetChildSnapshot("status").GetValue<NSObject>().ToString();
                        }

                        // Get fares
                        if(snapshot.GetChildSnapshot("fares").GetValue<NSObject>() != NSNull.Null)
                        {
                            fares = double.Parse(snapshot.GetChildSnapshot("fares").GetValue<NSObject>().ToString());
                        }


                        if (isDriverAccepted)
                        {
                            // Get driver location upadtes

                            double driverLatitude = 0;
                            double driverLongitude = 0;

                            driverLatitude = double.Parse(snapshot.GetChildSnapshot("driver_location").GetChildSnapshot("latitude").GetValue<NSObject>().ToString());
                            driverLongitude = double.Parse(snapshot.GetChildSnapshot("driver_location").GetChildSnapshot("longitude").GetValue<NSObject>().ToString());

                            CLLocationCoordinate2D driverLocationLatLng = new CLLocationCoordinate2D(driverLatitude, driverLongitude);
                            TripUpdates?.Invoke(this, new TripUpdatesEventArgs { DriverLocation = driverLocationLatLng, Status = status, Fares = fares });
                            
                        }

                    }
                }
            });
        }

        public void CancelRequest()
        {
            if (selectedDriver != null)
            {
                notifyDriverRef = Database.DefaultInstance.GetRootReference().GetChild("driversAvailable/" + selectedDriver.ID + "/ride_id");
                notifyDriverRef.SetValue<NSString>((NSString)"cancelled");
            }
            requestReference.RemoveValue();
            requestReference.RemoveAllObservers();
        }

        public void CancelRequestOnTimeout()
        {
            requestReference.RemoveValue();
            requestReference.RemoveAllObservers();
        }

        public void NotifyDriver(List<AvailableDriver> drivers)
        {
            availableDrivers = drivers;

            if(availableDrivers.Count >=1 && availableDrivers != null)
            {
                selectedDriver = availableDrivers[0];
                notifyDriverRef = Database.DefaultInstance.GetRootReference().GetChild("driversAvailable/" + selectedDriver.ID + "/ride_id");
                notifyDriverRef.SetValue<NSString>((NSString)newTripDetails.RideID);


                // Remove selected driver from the list
                if(availableDrivers.Count > 1)
                {
                    availableDrivers.RemoveAt(0);
                }
                else if(availableDrivers.Count == 1)
                {
                    availableDrivers = null;
                }

                RequestTimer.Enabled = true;
            }
            else
            {
                // No driver accepted

                RequestTimer.Enabled = false;
                NoDriverAcceptedRequest?.Invoke(this, new EventArgs());
            }

        }
    }
}
