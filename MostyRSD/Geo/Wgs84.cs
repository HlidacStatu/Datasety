using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MostyRSD.Geo
{
    public class Wgs84
    {

        private double latitude;

        private double longitude;

        /**
         * nadmořská výška v metrech
         */
        private double altitude = 200;

        public Wgs84(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public Wgs84(double latitude, double longitude, double altitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
        }

        public double getLatitude()
        {
            return latitude;
        }

        public void setLatitude(double latitude)
        {
            this.latitude = latitude;
        }

        public double getLongitude()
        {
            return longitude;
        }

        public void setLongitude(double longitude)
        {
            this.longitude = longitude;
        }

        public double getAltitude()
        {
            return altitude;
        }

        public void setAltitude(double altitude)
        {
            this.altitude = altitude;
        }

    }
}
