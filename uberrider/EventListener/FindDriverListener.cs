using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Firebase.Database;
using Foundation;
using uberrider.DataModels;

namespace uberrider.EventListener
{
    public class FindDriverListener
    {
        CLLocationCoordinate2D pickupLocation;
        DatabaseReference findDriverRef;

        List<AvailableDriver> availableDrivers = new List<AvailableDriver>();


        //Events
        public class DriversFoundEventArgs: EventArgs
        {
            public List<AvailableDriver> Drivers { get; set; }
        }

        public event EventHandler<DriversFoundEventArgs> DriversFound;
        public event EventHandler DriverNotFound;

        public FindDriverListener( CLLocationCoordinate2D _pickupLocation)
        {
            pickupLocation = _pickupLocation;
            findDriverRef = Database.DefaultInstance.GetRootReference().GetChild("driversAvailable");
        }

     public void FindDrivers()
        {
            findDriverRef.ObserveSingleEvent(DataEventType.Value, (DataSnapshot snapshot) =>
            {
                if(snapshot.GetValue<NSObject>() != null)
                {
                    //Converts Datasnapshot to a dictionary to retrieve the keys;
                    var snapShotData = snapshot.GetValue<NSDictionary>();

                    foreach (NSString key in snapShotData.Keys)
                    {
                        if(snapshot.GetChildSnapshot(key).GetChildSnapshot("ride_id").GetValue<NSObject>() != NSNull.Null)
                        {
                            string ride_id = snapshot.GetChildSnapshot(key).GetChildSnapshot("ride_id").GetValue<NSObject>().ToString();

                            if(ride_id == "waiting")
                            {
                                // Fetch Location Coordinates
                                string latitudeString = snapshot.GetChildSnapshot(key).GetChildSnapshot("location").GetChildSnapshot("latitude").GetValue<NSObject>().ToString();
                                string longitudeString = snapshot.GetChildSnapshot(key).GetChildSnapshot("location").GetChildSnapshot("longitude").GetValue<NSObject>().ToString();
                                CLLocationCoordinate2D driverLocation = new CLLocationCoordinate2D(double.Parse(latitudeString), double.Parse(longitudeString));

                                // Compute Distance Between Pickup Location and Driver Location (KM)
                                double distanceFromPickup = (Google.Maps.GeometryUtils.Distance(pickupLocation, driverLocation)) / 1000;

                                if(distanceFromPickup <= 50)
                                {
                                    // available driver
                                    AvailableDriver driver = new AvailableDriver();
                                    driver.ID = key;
                                    driver.DistanceFromPickup = distanceFromPickup;
                                    availableDrivers.Add(driver);
                                }
                            }
                        }
                    }

                    if(availableDrivers.Count > 0)
                    {
                        // Sort drivers to closest distance
                        availableDrivers = availableDrivers.OrderBy(o => o.DistanceFromPickup).ToList();
                        DriversFound?.Invoke(this, new DriversFoundEventArgs { Drivers = availableDrivers });
                    }
                    else
                    {
                        DriverNotFound.Invoke(this, new EventArgs());
                    }
                }
            });
        }
    }
}
