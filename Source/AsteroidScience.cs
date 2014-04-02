using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DMModuleScienceAnimateGeneric
{
    public class AsteroidScience
    {
        protected DMModuleScienceAnimateGeneric ModSci = FlightGlobals.ActiveVessel.FindPartModulesImplementing<DMModuleScienceAnimateGeneric>().First();
        
        //Let's make us some asteroid science
        //First construct a new celestial body from an asteroid
        public CelestialBody AsteroidBody = null;

        public CelestialBody Asteroid()
        {
            AsteroidBody = new CelestialBody();
            AsteroidBody.bodyName = "Asteroid P2X-459";
            AsteroidBody.use_The_InName = false;
            asteroidValues(AsteroidBody);
            return AsteroidBody;
        }

        public void asteroidValues(CelestialBody body)
        {
            //Find out how to vary based on asteroid size
            body.scienceValues.LandedDataValue = 10f;
            body.scienceValues.InSpaceLowDataValue = 4f;            
        }

        public ExperimentSituations asteroidSituation()
        {
            if (asteroidGrappled()) return ExperimentSituations.SrfLanded;
            else if (asteroidNear()) return ExperimentSituations.InSpaceLow;
            else return ModSci.getSituation();
        }

        //Are we attached to the asteroid
        public bool asteroidGrappled()
        {
            if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().Count >= 1)
                return true;
            else return false;
        }

        //Are we near the asteroid - need to figure this out
        public bool asteroidNear()
        {
            return false;
        }
    }
}
