using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;

namespace DMModuleScienceAnimateGeneric
{
    [Flags]
    internal enum PlanetaryIndices
    {
        Sun = 1 << 0,
        Kerbin = 1 << 1,
        Mun = 1 << 2,
        Minmus = 1 << 3,
        Moho = 1 << 4,
        Eve = 1 << 5,
        Duna = 1 << 6,
        Ike = 1 << 7,
        Jool = 1 << 8,
        Laythe = 1 << 9,
        Vall = 1 << 10,
        Bop = 1 << 11,
        Tylo = 1 << 12,
        Gilly = 1 << 13,
        Pol = 1 << 14,
        Dres = 1 << 15,
        Eeloo = 1 << 16,
        Asteroid = 1 << 17,
        All = 1 << 18,
    }

    internal class planetaryScience
    {
        internal static PlanetaryIndices planetIndex(int flightGlobalsIndex)
        {
            switch (flightGlobalsIndex)
            {
                case 1:
                    return PlanetaryIndices.Sun;
                case 2:
                    return PlanetaryIndices.Kerbin;
                case 3:
                    return PlanetaryIndices.Mun;
                case 4:
                    return PlanetaryIndices.Minmus;
                case 5:
                    return PlanetaryIndices.Moho;
                case 6:
                    return PlanetaryIndices.Eve;
                case 7:
                    return PlanetaryIndices.Duna;
                case 8:
                    return PlanetaryIndices.Ike;
                case 9:
                    return PlanetaryIndices.Jool;
                case 10:
                    return PlanetaryIndices.Laythe;
                case 11:
                    return PlanetaryIndices.Vall;
                case 12:
                    return PlanetaryIndices.Bop;
                case 13:
                    return PlanetaryIndices.Tylo;
                case 14:
                    return PlanetaryIndices.Gilly;
                case 15:
                    return PlanetaryIndices.Pol;
                case 16:
                    return PlanetaryIndices.Dres;
                case 17:
                    return PlanetaryIndices.Eeloo;
                case 18:
                    return PlanetaryIndices.Asteroid;
                default:
                    return PlanetaryIndices.All;
            }
        }

        internal static bool planetConfirm(uint pMask)
        {
            DMModuleScienceAnimateGeneric obj = new DMModuleScienceAnimateGeneric();
            PlanetaryIndices index = new PlanetaryIndices();
            if (obj.asteroidReports && AsteroidScience.asteroidGrappled() || obj.asteroidReports && AsteroidScience.asteroidNear()) index = planetIndex(18);
            else index = planetIndex(FlightGlobals.ActiveVessel.mainBody.flightGlobalsIndex);
            PlanetaryIndices mask = (PlanetaryIndices)pMask;
            if ((mask & index) == index) return true;
            else return false;
        }

    }
}
