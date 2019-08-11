using System;
using CommLiby;
using Microsoft.Maps.MapControl;

namespace CommonLibSL.Map
{
    public class MapAdjust : IEquatable<MapAdjust>
    {
        private readonly GeoType _geoType;
        public MapAdjust(GeoType geoType)
        {
            _geoType = geoType;
        }

        public GeoType GeoType => _geoType;

        public bool Equals(MapAdjust other)
        {
            if (other == null) return false;
            return _geoType == other._geoType;
        }

        public Location GetLoLa(double ALo, double ALa, int AX = 0, int AY = 0)
        {
            if (_geoType != GeoType.WGS84)
            {
                if (_geoType == GeoType.GCJ02)
                {
                    GeoLatLng gcj02 = GPSTool.wgs84togcj02(ALo, ALa);
                    return new Location(gcj02.latitude, gcj02.longitude);
                }
                else if (_geoType == GeoType.BD09)
                {
                    //GeoLatLng gcj02 = GPSTool.wgs84togcj02(ALo, ALa);
                    //GeoLatLng bd09 = GPSTool.gcj02tobd09(gcj02.longitude, gcj02.latitude);
                    GeoLatLng bd09 = GPSTool.bd09togcj02(ALo, ALa);
                    return new Location(bd09.latitude, bd09.longitude);
                }
                else
                {
                    return new Location(ALa, ALo);
                }
                //return new Location(ALa + AY / 1000000.0, ALo + AX / 1000000.0);
            }
            else
                return new Location(ALa, ALo);
        }

        public Location GetLoLa(int ALo, int ALa, int AX, int AY)
        {
            return GetLoLa((ALo * 1.0) / 1000000, (ALa * 1.0) / 1000000, AX, AY);
        }
        public Location GetLoLa(uint ALo, uint ALa, int AX, int AY)
        {
            return GetLoLa((ALo * 1.0) / 1000000, (ALa * 1.0) / 1000000, AX, AY);
        }

        public Location GetLoLa(double aLo, double aLa, short ax, short ay)
        {
            return GetLoLa(aLo, aLa, (int)ax, (int)ay);
        }
    }
}
