using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eCat.OrchidJet.Models
{
    public class Vehicle
    {

        public long IndexID { get; set; }
        public string IntId { get; set; }
        public string ExtId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Enginecode { get; set; }
        public string EngineSize { get; set; }
        public string VehicleType { get; set; }
        public string Mark { get; set; }
        public string NumberOfCylinder { get; set; }
        public string DriveType { get; set; }
        public string FuelType { get; set; }
        public string TransmissionType { get; set; }
        public string Trim { get; set; }
        public string Body { get; set; }
        public string Bhp { get; set; }
        public string CC { get; set; }
        public string Valves { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string DateRange { get; set; }
        public string fromYearint { get; set; }
        public string toYearint { get; set; }
        public string fromMonthint { get; set; }
        public string toMonthint { get; set; }
        public string TrimBody { get; set; }
        public string Cam { get; set; }
        public string Kw { get; set; }
        public string HP { get; set; }


        public string getDateRange(string datefrm, string dateTo)
        {
            try
            {
                var Fromyear = DateTime.Parse(datefrm).Year.ToString();
                var ToYear = DateTime.Parse(dateTo).Year.ToString();
                return (Fromyear + "-" + ToYear);
            }
            catch
            {
                return "01/1111";
            }
        }

        public int getStartDateComparer(string date)
        {
            try
            {
                string comp = DateTime.Parse(date).Year.ToString();
                int Date;
                int.TryParse(comp, out Date);
                return Date;
            }
            catch
            {
                return 1111;
            }
        }
    }
}