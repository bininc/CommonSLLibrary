using CommonLibSL.Model;
using Microsoft.Maps.MapControl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using CommLiby;

namespace CommonLibSL.Map
{
    public class MapLayer : Microsoft.Maps.MapControl.MapLayer
    {
        public event Action<MapCar> ActiveMapCarChanged;
        public event Action<MapCar, Location> MapCarLocationChanged;
        public event EventHandler MapAdjustChanged;
        private MapCar _activeCar;
        private readonly Dictionary<SLCar, MapCar> _dicCreatedCars = new Dictionary<SLCar, MapCar>();
        private bool _isShowCarLicense;
        private List<Border> _listCarLicense = new List<Border>();
        private MapAdjust _mapAdjust;

        public MapLayer()
        {

        }

        public MapCar ActiveCar
        {
            get { return _activeCar; }
            set
            {
                //if (_activeCar == value) return;
                _activeCar = value;
                if (ActiveMapCarChanged != null)
                    ActiveMapCarChanged(_activeCar);
            }
        }


        public MapAdjust MapAdjust
        {
            get { return _mapAdjust; }
            set
            {
                if (value != null && !value.Equals(_mapAdjust))
                {
                    _mapAdjust = value;
                    MapAdjustChanged?.Invoke(_mapAdjust, new EventArgs());
                }
            }
        }

        public Dictionary<SLCar, MapCar>.ValueCollection CreatedMapCars { get { return _dicCreatedCars.Values; } }

        public bool IsShowCarLicense
        {
            get { return _isShowCarLicense; }
            set
            {
                _isShowCarLicense = value;
                if (_isShowCarLicense)
                {
                    _listCarLicense.ForEach(l => l.Visibility = Visibility.Visible);
                }
                else
                {
                    _listCarLicense.ForEach(l => l.Visibility = Visibility.Collapsed);
                }
            }
        }

        public MapCar GetMapCar(SLCar car, bool autoCreate = false, object tag = null)
        {
            MapCar mcar = null;
            if (car == null) return mcar;

            if (_dicCreatedCars.ContainsKey(car))
            {
                mcar = _dicCreatedCars[car];
            }
            else
            {
                if (autoCreate)
                {
                    mcar = new MapCar(car, this, tag);
                    mcar.LocationChanged += Mcar_LocationChanged;
                    _dicCreatedCars.Add(car, mcar);
                }
            }
            return mcar;
        }

        private void Mcar_LocationChanged(MapCar arg1, Location arg2)
        {
            MapCarLocationChanged(arg1, arg2);
        }

        public void AddCarLicense(Border border, Location location, PositionOrigin positionOrigin)
        {
            if (!IsShowCarLicense)
                border.Visibility = Visibility.Collapsed;
            else
                border.Visibility = Visibility.Visible;
            try
            {
                _listCarLicense.Add(border);
                this.AddChild(border, location, positionOrigin);
            }
            catch { }
        }

        public void RemoveCarLicense(Border border)
        {
            try
            {
                this.Children.Remove(border);
                _listCarLicense.Remove(border);
            }
            catch { }
        }
    }
}
