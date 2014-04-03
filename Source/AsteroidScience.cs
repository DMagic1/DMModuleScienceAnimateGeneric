using System;
using System.Collections.Generic;
using System.Collections;
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
            //Inherit values for the CelestialBody from an existing body, Kerbin in this case
            AsteroidBody = FlightGlobals.fetch.bodies[1];
            AsteroidBody.bodyName = "Asteroid P2X-459";
            AsteroidBody.use_The_InName = false;
            asteroidValues(AsteroidBody);
            return AsteroidBody;
        }

        //Alter some of the values to give us asteroid specific results based on asteroid class and current situation
        public static void asteroidValues(CelestialBody body)
        {
            if (asteroidNear())
            { 
                Part asteroidPart = FlightGlobals.activeTarget.FindModulesImplementing<ModuleAsteroid>().First().part;
                float partMass = asteroidPart.mass;
                body.scienceValues.InSpaceLowDataValue = 2f * partMass * 0.75f;
            }
            else if (asteroidGrappled())
            {
                Part asteroidPart = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().First().part;
                float partMass = asteroidPart.mass;
                body.scienceValues.LandedDataValue = 3f * partMass * 0.75f;
            }
            //float asteroidDensity = FlightGlobals.activeTarget.FindModulesImplementing<ModuleAsteroid>().First().density;


            //AsteroidBody.scienceValues.LandedDataValue = 10f;
            //AsteroidBody.scienceValues.InSpaceLowDataValue = 4f;            
        }

        public static ExperimentSituations asteroidSituation()
        {
            if (asteroidGrappled()) return ExperimentSituations.SrfLanded;
            else if (asteroidNear()) return ExperimentSituations.InSpaceLow;
            else return ModSci.getSituation();
        }

        //Are we attached to the asteroid, check if an asteroid part is on our vessel
        public static bool asteroidGrappled()
        {
            if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().Count >= 1)
                return true;
            else return false;
        }

        //Are we near the asteroid, check our target data
        public static bool asteroidNear()
        {
            if (FlightGlobals.activeTarget.FindModulesImplementing<ModuleAsteroid>().Count > 0)
            {
                Part asteroidPart = FlightGlobals.activeTarget.FindModulesImplementing<ModuleAsteroid>().First().part;
                Vector3 asteroidPosition = asteroidPart.transform.position;
                Vector3 vesselPosition = FlightGlobals.ActiveVessel.transform.position;
                double distance = (asteroidPosition - vesselPosition).magnitude;
                if (distance < 1000) return true;
                else return false;
            }
            return false;
        }
    }
}
