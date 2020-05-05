#region license
/* DMagic Orbital Science - Asteroid Science
 * Class to setup asteroid science data
 *
 * Copyright (c) 2014-2016, David Grandy
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
#endregion

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMModuleScienceAnimateGeneric_NS
{
	public class DMAsteroidScienceGen
	{
		private static ModuleAsteroid modAsteroid;
		private string aClass = null;
		private float sciMult = 1f;
		private CelestialBody body = null;

		internal DMAsteroidScienceGen()
		{
			body = FlightGlobals.Bodies[16];
			body.bodyName = "Asteroid";
			asteroidValues();
		}

		public string AClass
		{
			get { return aClass; }
		}

		public float SciMult
		{
			get { return sciMult; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}

		//Alter some of the values to give us asteroid specific results based on asteroid class and current situation
		private void asteroidValues()
		{
			if (AsteroidNear)
				asteroidValues(modAsteroid, 1f);
			else if (AsteroidGrappled)
			{
				ModuleAsteroid asteroidM = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().First();
				asteroidValues(asteroidM, 1.5f);
			}
		}

		private void asteroidValues(ModuleAsteroid m, float mult)
		{
			aClass = asteroidClass(m.prefabBaseURL);
			sciMult = asteroidValue(aClass) * mult;
		}

		private string asteroidClass(string s)
		{
			switch (s[s.Length - 1])
			{
				case 'A':
					return "Class A";
				case 'B':
					return "Class B";
				case 'C':
					return "Class C";
				case 'D':
					return "Class D";
				case 'E':
					return "Class E";
				default:
					return "Class Unholy";
			}
		}

		private float asteroidValue(string aclass)
		{
			switch (aclass)
			{
				case "Class A":
					return 1.5f;
				case "Class B":
					return 3f;
				case "Class C":
					return 5f;
				case "Class D":
					return 8f;
				case "Class E":
					return 10f;
				case "Class Unholy":
					return 30f;
				default:
					return 1f;
			}
		}

		//Are we attached to the asteroid, check if an asteroid part is on our vessel
		public static bool AsteroidGrappled
		{
			get
			{
				if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().Count > 0)
					return true;
				else
					return false;
			}
		}

		//Are we near the asteroid, cycle through existing vessels, only target asteroids within 2km
		public static bool AsteroidNear
		{
			get
			{
				for (int i = FlightGlobals.VesselsLoaded.Count - 1; i >= 0; i--)
				{
					Vessel v = FlightGlobals.VesselsLoaded[i];

					if (v == null)
						continue;

					if (v == FlightGlobals.ActiveVessel)
						continue;

					if (v.mainBody != FlightGlobals.ActiveVessel.mainBody)
						continue;

					ModuleAsteroid m = v.FindPartModulesImplementing<ModuleAsteroid>().FirstOrDefault();

					if (m == null)
						continue;

					double distance = (m.part.transform.position - FlightGlobals.ActiveVessel.transform.position).sqrMagnitude;

					if (distance > (2500 * 2500))
						continue;

					modAsteroid = m;
					return true;
				}

				return false;
			}
		}


	}
}
