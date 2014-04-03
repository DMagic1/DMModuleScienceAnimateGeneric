using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DMModuleScienceAnimateGeneric
{
    public class AsteroidScience
    {
        protected static DMModuleScienceAnimateGeneric ModSci = FlightGlobals.ActiveVessel.FindPartModulesImplementing<DMModuleScienceAnimateGeneric>().First();
        
        //Let's make us some asteroid science
        //First construct a new celestial body from an asteroid
        public static CelestialBody AsteroidBody = null;

        public static CelestialBody Asteroid()
        {
            AsteroidBody = new CelestialBody();
            AsteroidBody.bodyName = "Asteroid P2X-459";
            AsteroidBody.use_The_InName = false;
            //asteroidValues();
            return AsteroidBody;
        }

        public static void asteroidValues()
        {
            //Find out how to vary based on asteroid size
            
            AsteroidBody.scienceValues.LandedDataValue = 10f;
            AsteroidBody.scienceValues.InSpaceLowDataValue = 4f;            
        }

        public static ExperimentSituations asteroidSituation()
        {
            if (asteroidGrappled()) return ExperimentSituations.SrfLanded;
            else if (asteroidNear()) return ExperimentSituations.InSpaceLow;
            else return ModSci.getSituation();
        }

        //Are we attached to the asteroid
        public static bool asteroidGrappled()
        {
            if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().Count >= 1)
                return true;
            else return false;
        }

        //Are we near the asteroid - need to figure this out
        public static bool asteroidNear()
        {
            return false;
        }
    }
}
