#region license
/* DMagic Orbital Science - DMSciAnimAPI
 * Static utilities class for interacting with other mods
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


using System;
using System.Collections.Generic;
using System.Linq;
using DMModuleScienceAnimateGeneric_NM;

namespace DMModuleScienceAnimateGeneric
{
	public static class DMSciAnimAPI
	{
		/// <summary>
		/// Use to determine whether an experiment can be conducted at this time. This returns the same value as the internal check used when an experiment is deplyed from the right-click menu.
		/// </summary>
		/// <param name="isc">The science experiment module must be cast as a IScienceDataContianer.</param>
		/// <returns>Returns true if the experiment can be performed; will return false if the science module is not of the right type.</returns>
		public static bool experimentCanConduct(IScienceDataContainer isc)
		{
			if (isc == null)
				return false;

			Type t = isc.GetType();

			if (t == typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
			{
                DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)isc;
				return DMMod.canConduct();
			}

			return false;
		}

		/// <summary>
		/// Uses the internal method for conducting an experiment; the experiment cannot be forced and must first pass the "canConduct". All associated animations and other functions will be called. Optinally run the experiment without opening the results window.
		/// </summary>
		/// <param name="isc">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <param name="silent">Set to true to prevent the science results dialog from opening.</param>
		/// <returns>Returns true if the science module is of the right type and the gather science method is called.</returns>
		public static bool deployDMExperiment(IScienceDataContainer isc, bool silent = false)
		{
			if (isc == null)
				return false;

			Type t = isc.GetType();

			if (t == typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
			{
                DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)isc;
				DMMod.gatherScienceData(silent);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Calls the internal method for getting the Experiment Situation for a certain experiment.
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <returns>Returns the Experiment Situation value for that experiment; returns InSpaceHigh if the experiment is not of the right type.</returns>
		public static ExperimentSituations getExperimentSituation(ModuleScienceExperiment mse)
		{
			if (mse == null)
				return ExperimentSituations.InSpaceHigh;

			Type t = mse.GetType();

			if (t == typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
			{
                DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)mse;
				return DMMod.getSituation();
			}

			return ExperimentSituations.InSpaceHigh;
		}

		/// <summary>
		/// Calls the internal method for getting the biome for a certain experiment.
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <param name="sit">The current Experiment Situation value; see getExperimentSituation.</param>
		/// <returns>Returns the biome string for that experiment; returns an empty string if the experiment is not of the right type.</returns>
		public static string getBiome(ModuleScienceExperiment mse, ExperimentSituations sit)
		{
			if (mse == null)
				return "";

			Type t = mse.GetType();

			if (t == typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
			{
                DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)mse;
				return DMMod.getBiome(sit);
			}

			return "";
		}

		/// <summary>
		/// Check if an experiment can be conducted on asteroids.
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <returns>Returns false if the module is not of the right type or if the experiment cannot be conducted with asteroids.</returns>
		public static bool isAsteroidExperiment(ModuleScienceExperiment mse)
		{
			if (mse == null)
				return false;

			Type t = mse.GetType();

			if (t == typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
			{
                DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)mse;
				return DMMod.asteroidReports;
			}

			return false;
		}

		/// <summary>
		/// Check if an experiment can be conducted on asteroids.
		/// </summary>
		/// <param name="dms">The science experiment module must be cast as a DMModuleScienceAnimateGeneric.</param>
		/// <returns>Returns false if the module is not of the right type or if the experiment cannot be conducted with asteroids.</returns>
		public static bool isAsteroidExperiment(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric dms)
		{
			if (dms == null)
				return false;

			return dms.asteroidReports;
		}

		/// <summary>
		/// Check to see if an asteroid is within loading distance.
		/// </summary>
		/// <returns>Returns true if an asteroid is detected within loading distance (2.5km)</returns>
		public static bool isAsteroidNear()
		{
			return DMAsteroidScienceGen.AsteroidNear;
		}

		/// <summary>
		/// Check to see if an asteroid is within loading distance and an experiment is valid for asteroid science.
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <returns>Returns false if the module is not of the right type, if the experiment cannot be conducted with asteroids, or if no asteroids are within loading distance (2.5km)</returns>
		public static bool isAsteroidNear(ModuleScienceExperiment mse)
		{
			if (!isAsteroidExperiment(mse))
				return false;

			return isAsteroidNear();
		}

		/// <summary>
		/// Check to see if an asteroid is grappled to the current vessel.
		/// </summary>
		/// <returns>Returns true if an asteroid is attached to the current vessel.</returns>
		public static bool isAsteroidGrappled()
		{
			return DMAsteroidScienceGen.AsteroidGrappled;
		}

		/// <summary>
		/// Check to see if an asteroid is grappled to the current vessel and an experiment is valid for asteroid science
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <returns>Returns false if the module is not of the right type, if the experiment cannot be conducted with asteroids, or if no asteroid is attached to the current vessel.</returns>
		public static bool isAsteroidGrappled(ModuleScienceExperiment mse)
		{
			if (!isAsteroidExperiment(mse))
				return false;

			return isAsteroidGrappled();
		}

		/// <summary>
		/// Get the ScienceSubject for an asteroid experiment.
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <returns>Returns the ScienceSubject for that specific asteroid and experiment; returns null if the module is not of the right type, the experiment is not suitable for astroids, if no asteroids are detected, or if the current asteroid situation is not suitable for the experiment.</returns>
		public static ScienceSubject getAsteroidSubject(ModuleScienceExperiment mse)
		{
			if (mse == null)
				return null;

			Type t = mse.GetType();

			if (t != typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
				return null;

            DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)mse;

			if (!isAsteroidExperiment(DMMod))
				return null;

			if (DMMod.scienceExp == null)
				return null;

			if (DMAsteroidScienceGen.AsteroidGrappled)
			{
				if ((DMMod.scienceExp.situationMask & (int)ExperimentSituations.SrfLanded) == 0)
					return null;

				string oldBodyName = FlightGlobals.Bodies[16].bodyName;

				DMAsteroidScienceGen newAsteroid = new DMAsteroidScienceGen();
				ScienceSubject sub = new ScienceSubject(DMMod.scienceExp, ExperimentSituations.SrfLanded, newAsteroid.Body, newAsteroid.AClass);
				sub.subjectValue = newAsteroid.SciMult;
				sub.scienceCap = DMMod.scienceExp.scienceCap * sub.subjectValue;
				newAsteroid.Body.bodyName = oldBodyName;
				return sub;
			}
			else if (DMAsteroidScienceGen.AsteroidNear)
			{
				if ((DMMod.scienceExp.situationMask & (int)ExperimentSituations.InSpaceLow) == 0)
					return null;

				string oldBodyName = FlightGlobals.Bodies[16].bodyName;

				DMAsteroidScienceGen newAsteroid = new DMAsteroidScienceGen();
				ScienceSubject sub = new ScienceSubject(DMMod.scienceExp, ExperimentSituations.InSpaceLow, newAsteroid.Body, newAsteroid.AClass);
				sub.subjectValue = newAsteroid.SciMult;
				sub.scienceCap = DMMod.scienceExp.scienceCap * sub.subjectValue;
				newAsteroid.Body.bodyName = oldBodyName;
				return sub;
			}

			return null;
		}

		/// <summary>
		/// Get the ScienceSubject for an asteroid experiment.
		/// </summary>
		/// <param name="mse">The science experiment module must be cast as a ModuleScienceExperiment.</param>
		/// <param name="sit">The current Experiment Situation value; see the getExperimentSituation method above.</param>
		/// <returns>Returns the ScienceSubject for that specific asteroid, experiment, and ExperimentSituation; returns null if the module is not of the right type, the experiment is not suitable for astroids, if no asteroids are detected, or if the current asteroid situation is not suitable for the experiment.</returns>
		public static ScienceSubject getAsteroidSubject(ModuleScienceExperiment mse, ExperimentSituations sit)
		{
			if (mse == null)
				return null;

			Type t = mse.GetType();

			if (t != typeof(DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric))
				return null;

            DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric_NM.DMModuleScienceAnimateGeneric)mse;

			if (!isAsteroidExperiment(DMMod))
				return null;

			if (DMMod.scienceExp == null)
				return null;

			if (sit == ExperimentSituations.InSpaceLow)
			{
				if (!isAsteroidNear())
					return null;
			}
			else if (sit == ExperimentSituations.SrfLanded)
			{
				if (!isAsteroidGrappled())
					return null;
			}
			else
				return null;

			if ((DMMod.scienceExp.situationMask & (int)sit) == 0)
				return null;

			string oldBodyName = FlightGlobals.Bodies[16].bodyName;

			DMAsteroidScienceGen newAsteroid = new DMAsteroidScienceGen();
			ScienceSubject sub = new ScienceSubject(DMMod.scienceExp, sit, newAsteroid.Body, newAsteroid.AClass);
			sub.subjectValue = newAsteroid.SciMult;
			sub.scienceCap = DMMod.scienceExp.scienceCap * sub.subjectValue;
			newAsteroid.Body.bodyName = oldBodyName;
			return sub;
		}

	}
}
