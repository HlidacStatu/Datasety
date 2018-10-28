using System;
using System.Linq;

namespace MostyRSD.Geo
{
    public class JTSK
    {

        private double x;

        private double y;

        public JTSK(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double getX()
        {
            return x;
        }

        public void setX(double x)
        {
            this.x = x;
        }

        public double getY()
        {
            return y;
        }

        public void setY(double y)
        {
            this.y = y;
        }

        /**
         * převádí WGS-84 do S-JTSK
         * 
         * @param WGS84
         * @return JTSK
         */
        public static JTSK convert(Wgs84 wgs84)
        {
            double altitude = wgs84.getAltitude();

            double d2r = Math.PI / 180;
            double a = 6378137.0;
            double f1 = 298.257223563;
            double dx = -570.69;
            double dy = -85.69;
            double dz = -462.84;
            double wx = 4.99821 / 3600 * Math.PI / 180;
            double wy = 1.58676 / 3600 * Math.PI / 180;
            double wz = 5.2611 / 3600 * Math.PI / 180;
            double m = -3.543e-6;

            double latitude = wgs84.getLatitude();
            double longtitude = wgs84.getLongitude();
            double B = latitude * d2r;
            double L = longtitude * d2r;
            double H = altitude;

            double e2 = 1 - Math.Pow(1 - 1 / f1, 2);
            double rho = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(B), 2));
            double x1 = (rho + H) * Math.Cos(B) * Math.Cos(L);
            double y1 = (rho + H) * Math.Cos(B) * Math.Sin(L);
            double z1 = ((1 - e2) * rho + H) * Math.Sin(B);

            double x2 = dx + (1 + m) * (x1 + wz * y1 - wy * z1);
            double y2 = dy + (1 + m) * (-wz * x1 + y1 + wx * z1);
            double z2 = dz + (1 + m) * (wy * x1 - wx * y1 + z1);

            a = 6377397.15508;
            f1 = 299.152812853;
            double ab = f1 / (f1 - 1);
            double p = Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2));
            e2 = 1 - Math.Pow(1 - 1 / f1, 2);
            double th = Math.Atan(z2 * ab / p);
            double st = Math.Sin(th);
            double ct = Math.Cos(th);
            double t = (z2 + e2 * ab * a * (st * st * st)) / (p - e2 * a * (ct * ct * ct));

            B = Math.Atan(t);
            H = Math.Sqrt(1 + t * t) * (p - a / Math.Sqrt(1 + (1 - e2) * t * t));
            L = 2 * Math.Atan(y2 / (p + x2));

            a = 6377397.15508;
            double e = 0.081696831215303;
            double n = 0.97992470462083;
            double rho0 = 12310230.12797036;
            double sinUQ = 0.863499969506341;
            double cosUQ = 0.504348889819882;
            double sinVQ = 0.420215144586493;
            double cosVQ = 0.907424504992097;
            double alpha = 1.000597498371542;
            double k2 = 1.00685001861538;

            double sinB = Math.Sin(B);
            t = (1 - e * sinB) / (1 + e * sinB);
            t = Math.Pow(1 + sinB, 2) / (1 - Math.Pow(sinB, 2)) * Math.Exp(e * Math.Log(t));
            t = k2 * Math.Exp(alpha * Math.Log(t));

            double sinU = (t - 1) / (t + 1);
            double cosU = Math.Sqrt(1 - sinU * sinU);
            double V = alpha * L;
            double sinV = Math.Sin(V);
            double cosV = Math.Cos(V);
            double cosDV = cosVQ * cosV + sinVQ * sinV;
            double sinDV = sinVQ * cosV - cosVQ * sinV;
            double sinS = sinUQ * sinU + cosUQ * cosU * cosDV;
            double cosS = Math.Sqrt(1 - sinS * sinS);
            double sinD = sinDV * cosU / cosS;
            double cosD = Math.Sqrt(1 - sinD * sinD);

            double eps = n * Math.Atan(sinD / cosD);
            rho = rho0 * Math.Exp(-n * Math.Log((1 + sinS) / cosS));

            double CX = rho * Math.Sin(eps);
            double CY = rho * Math.Cos(eps);

            return new JTSK(-CX, -CY);
        }



        public static Wgs84 ToWgs(double X, double Y, double H = 200)
        {
            /*Vypocet zemepisnych
            function jtsk_to_wgs(X,Y,H)
{
  var coord = {wgs84_latitude:"", wgs84_longitude:"", lat: 0, lon: 0, vyska: 0};
 
  /* Přepočet vstupích údajů - vychazi z nejakeho skriptu, ktery jsem nasel na Internetu - nejsem autorem prepoctu. */

            /*Vypocet zemepisnych souradnic z rovinnych souradnic*/
            double a = 6377397.15508;
            double e = 0.081696831215303;
            double n = 0.97992470462083;
            double konst_u_ro = 12310230.12797036;
            double sinUQ = 0.863499969506341;
            double cosUQ = 0.504348889819882;
            double sinVQ = 0.420215144586493;
            double cosVQ = 0.907424504992097;
            double alfa = 1.000597498371542;
            double k = 1.003419163966575;
            double ro = Math.Sqrt(X * X + Y * Y);
            double epsilon = 2 * Math.Atan(Y / (ro + X));
            double D = epsilon / n;
            double S = 2 * Math.Atan(Math.Exp(1 / n * Math.Log(konst_u_ro / ro))) - Math.PI / 2;
            double sinS = Math.Sin(S);
            double cosS = Math.Cos(S);
            double sinU = sinUQ * sinS - cosUQ * cosS * Math.Cos(D);
            double cosU = Math.Sqrt(1 - sinU * sinU);
            double sinDV = Math.Sin(D) * cosS / cosU;
            double cosDV = Math.Sqrt(1 - sinDV * sinDV);
            double sinV = sinVQ * cosDV - cosVQ * sinDV;
            double cosV = cosVQ * cosDV + sinVQ * sinDV;
            double Ljtsk = 2 * Math.Atan(sinV / (1 + cosV)) / alfa;
            double t = Math.Exp(2 / alfa * Math.Log((1 + sinU) / cosU / k));
            double pom = (t - 1) / (t + 1);
            double sinB = pom;
            do
            {
                sinB = pom;
                pom = t * Math.Exp(e * Math.Log((1 + e * sinB) / (1 - e * sinB)));
                pom = (pom - 1) / (pom + 1);
            } while (Math.Abs(pom - sinB) > 1e-15);

            double Bjtsk = Math.Atan(pom / Math.Sqrt(1 - pom * pom));


            /* Pravoúhlé souřadnice ve S-JTSK */
            a = 6377397.15508; double f_1 = 299.152812853;
            double e2 = 1 - (1 - 1 / f_1) * (1 - 1 / f_1); ro = a / Math.Sqrt(1 - e2 * Math.Sin(Bjtsk) * Math.Sin(Bjtsk));
            double x = (ro + H) * Math.Cos(Bjtsk) * Math.Cos(Ljtsk);
            double y = (ro + H) * Math.Cos(Bjtsk) * Math.Sin(Ljtsk);
            double z = ((1 - e2) * ro + H) * Math.Sin(Bjtsk);

            /* Pravoúhlé souřadnice v WGS-84*/
            double dx = 570.69; double dy = 85.69; double dz = 462.84;
            double wz = -5.2611 / 3600 * Math.PI / 180;
            double wy = -1.58676 / 3600 * Math.PI / 180;
            double wx = -4.99821 / 3600 * Math.PI / 180;
            double m = 3.543e-6;
            double xn = dx + (1 + m) * (x + wz * y - wy * z);
            double yn = dy + (1 + m) * (-wz * x + y + wx * z);
            double zn = dz + (1 + m) * (wy * x - wx * y + z);

            /* Geodetické souřadnice v systému WGS-84*/
            a = 6378137.0; f_1 = 298.257223563;
            double a_b = f_1 / (f_1 - 1); double p = Math.Sqrt(xn * xn + yn * yn); e2 = 1 - (1 - 1 / f_1) * (1 - 1 / f_1);
            double theta = Math.Atan(zn * a_b / p);
            double st = Math.Sin(theta);
            double ct = Math.Cos(theta);
            t = (zn + e2 * a_b * a * st * st * st) / (p - e2 * a * ct * ct * ct);
            double B = Math.Atan(t);
            double L = 2 * Math.Atan(yn / (p + xn));
            H = Math.Sqrt(1 + t * t) * (p - a / Math.Sqrt(1 + (1 - e2) * t * t));

            /* Formát výstupních hodnot */
            B = B / Math.PI * 180;
            L = L / Math.PI * 180;

            return new Wgs84(B,L, Math.Round((H) * 100) / 100);
        }

        public static void ToLatLon(double utmX, double utmY, string utmZone, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = utmZone.Last() >= 'N';

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
    }
}

