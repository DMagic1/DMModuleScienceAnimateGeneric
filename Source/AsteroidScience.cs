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
        private static double distance;

        //Let's make us some asteroid science
        //First construct a new celestial body from an asteroid
        public static CelestialBody AsteroidBody = null;

        public static CelestialBody Asteroid()
        {
            //Inherit values for the CelestialBody from an existing body, Kerbin in this case
            AsteroidBody = FlightGlobals.fetch.bodies[1];
            AsteroidBody.bodyName = "Asteroid";
            asteroidValues(AsteroidBody);
            return AsteroidBody;
        }

        //Alter some of the values to give us asteroid specific results based on asteroid class and current situation
        private static void asteroidValues(CelestialBody body)
        {
            if (asteroidNear())
            {
                List<Vessel> vesselList = FlightGlobals.fetch.vessels;
                foreach (Vessel v in vesselList)
                {
                    if (v.FindPartModulesImplementing<ModuleAsteroid>().Count > 0 && distance < 2000)
                    {
                        Part asteroidPart = v.FindPartModulesImplementing<ModuleAsteroid>().First().part;
                        float partMass = asteroidPart.mass;
                        body.bodyDescription = asteroidClass(partMass);
                        float asteroidDataValue = asteroidValue(body.bodyDescription);
                        body.scienceValues.InSpaceLowDataValue = asteroidDataValue;
                        body.Mass = partMass;
                        break;
                    }
                }
            }
            else if (asteroidGrappled())
            {
                Part asteroidPart = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().First().part;
                float partMass = asteroidPart.mass;
                body.bodyDescription = asteroidClass(partMass);
                float asteroidDataValue = asteroidValue(body.bodyDescription);
                body.scienceValues.LandedDataValue = asteroidDataValue * 2f;
                body.Mass = partMass;
            }                        
        }

        //Need to figure out accurate mass ranges
        private static string asteroidClass(float mass)
        {
            if (mass >= 0.5f && mass < 10f) return "Class A";
            if (mass >= 10f && mass < 50f) return "Class B";
            if (mass >= 50f && mass < 250f) return "Class C";
            if (mass >= 250f && mass < 1500f) return "Class D";
            if (mass >= 1500f && mass < 10000f) return "Class E";
            return "Unknown Class";
        }

        private static float asteroidValue(string aclass)
        {
            switch (aclass)
            {
                case "Class A":
                    return 2f;
                case "Class B":
                    return 4f;
                case "Class C":
                    return 6f;
                case "Class D":
                    return 8f;
                case "Class E":
                    return 10f;
                default:
                    return 1f;
            }
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

        //Are we near the asteroid, cycle through existing vessels, only target asteroids within 2km
        public static bool asteroidNear()
        {
            List<Vessel> vesselList = FlightGlobals.fetch.vessels;
            foreach (Vessel v in vesselList)
            {
                if (v != FlightGlobals.ActiveVessel)
                {
                    if (v.FindPartModulesImplementing<ModuleAsteroid>().Count > 0)
                    {
                        Part asteroidPart = v.FindPartModulesImplementing<ModuleAsteroid>().First().part;
                        Vector3 asteroidPosition = asteroidPart.transform.position;
                        Vector3 vesselPosition = FlightGlobals.ActiveVessel.transform.position;
                        distance = (asteroidPosition - vesselPosition).magnitude;
                        if (distance < 2000) return true;
                        else return false;
                    }
                }
            }
            return false;
        }
    }
}
