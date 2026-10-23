using System;
using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public class AirTraffic
    {
        readonly HashSet<IAirborne> airborne = new HashSet<IAirborne>();

        public event Action<IAirborne> Registered;
        public event Action<IAirborne> Unregistered;

        public IReadOnlyCollection<IAirborne> All => airborne;

        public bool Register(IAirborne entity)
        {
            if (!airborne.Add(entity))
                return false;
            Registered?.Invoke(entity);
            return true;
        }

        public bool Unregister(IAirborne entity)
        {
            if (!airborne.Remove(entity))
                return false;
            Unregistered?.Invoke(entity);
            return true;
        }
    }
}
