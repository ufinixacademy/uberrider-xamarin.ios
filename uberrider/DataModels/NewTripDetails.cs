using System;
namespace uberrider.DataModels
{
    public class NewTripDetails
    {
       public string DestinationAddress { get; set; }
        public string PickupAddress { get; set; }
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public double DestinationLat { get; set; }
        public double DestinationLng { get; set; }
        public string DurationString { get; set; }
        public double DurationValue { get; set; }
        public string DistanceString { get; set; }
        public double DistanceValue { get; set; }
        public DateTime TimeStamp { get; set; }
        public double EstimateFare { get; set; }
        public string RideID { get; set; }
        public string PaymentMethod { get; set; }
    }
}
