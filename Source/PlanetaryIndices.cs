/* DMagic Orbital Science - Planetary Index
 * Class and enum to setup limit experiments to certain planets
 *
 * Copyright (c) 2014, David Grandy
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *  
 */

using System;

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
        All = 1 << 18, //The All value should allow for experiments to be used on any mod planets
    }

    internal class planetaryScience
    {
        internal static PlanetaryIndices planetIndex(int flightGlobalsIndex)
        {
            switch (flightGlobalsIndex)
            {
                case 0:
                    return PlanetaryIndices.Sun;
                case 1:
                    return PlanetaryIndices.Kerbin;
                case 2:
                    return PlanetaryIndices.Mun;
                case 3:
                    return PlanetaryIndices.Minmus;
                case 4:
                    return PlanetaryIndices.Moho;
                case 5:
                    return PlanetaryIndices.Eve;
                case 6:
                    return PlanetaryIndices.Duna;
                case 7:
                    return PlanetaryIndices.Ike;
                case 8:
                    return PlanetaryIndices.Jool;
                case 9:
                    return PlanetaryIndices.Laythe;
                case 10:
                    return PlanetaryIndices.Vall;
                case 11:
                    return PlanetaryIndices.Bop;
                case 12:
                    return PlanetaryIndices.Tylo;
                case 13:
                    return PlanetaryIndices.Gilly;
                case 14:
                    return PlanetaryIndices.Pol;
                case 15:
                    return PlanetaryIndices.Dres;
                case 16:
                    return PlanetaryIndices.Eeloo;
                case 100:
                    return PlanetaryIndices.Asteroid;
                default:
                    return PlanetaryIndices.All;
            }
        }

        //A simple check to see if the specified planets match the active vessel's current planet
        internal static bool planetConfirm(int pMask)
        {
            DMModuleScienceAnimateGeneric obj = new DMModuleScienceAnimateGeneric();
            PlanetaryIndices index = new PlanetaryIndices();
            if (obj.asteroidReports && AsteroidScience.asteroidGrappled() || obj.asteroidReports && AsteroidScience.asteroidNear()) index = planetIndex(100);
            else index = planetIndex(FlightGlobals.ActiveVessel.mainBody.flightGlobalsIndex);
            PlanetaryIndices mask = (PlanetaryIndices)pMask;
            if ((mask & index) == index) return true;
            else return false;
        }

    }
}
