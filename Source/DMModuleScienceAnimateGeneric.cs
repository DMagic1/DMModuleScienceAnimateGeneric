#region license
/* DMagic Orbital Science - Module Science Animate Generic
 * Generic module for animated science experiments.
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
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using FinePrint.Utilities;
using KSP.UI.Screens.Flight.Dialogs;

namespace DMModuleScienceAnimateGeneric
{
	public class DMModuleScienceAnimateGeneric : ModuleScienceExperiment, IScienceDataContainer
	{

		#region fields

		[KSPField]
		public string storageFullMessage = "No more samples can be collected";
		[KSPField]
		public string customFailMessage = "Can't conduct experiment here";
		[KSPField]
		public string deployingMessage = null;
		[KSPField]
		public string planetFailMessage = "Can't conduct experiment here";
		[KSPField(isPersistant = true)]
		public bool IsDeployed;
		[KSPField]
		public string animationName = null;
		[KSPField]
		public string sampleAnim = null;
		[KSPField(isPersistant = false)]
		public float animSpeed = 1f;
		[KSPField]
		public string endEventGUIName = "Retract";
		[KSPField]
		public bool showEndEvent = true;
		[KSPField]
		public string startEventGUIName = "Deploy";
		[KSPField]
		public bool showStartEvent = true;
		[KSPField]
		public string toggleEventGUIName = "Toggle";
		[KSPField]
		public bool showToggleEvent = false;
		[KSPField]
		public bool showEditorEvents = true;

		[KSPField]
		public bool experimentAnimation = true;
		[KSPField]
		public bool experimentWaitForAnimation = false;
		[KSPField]
		public float waitForAnimationTime = -1;
		[KSPField]
		public int keepDeployedMode = 0;
		[KSPField]
		public bool oneWayAnimation = false;
		[KSPField]
		public string resourceExperiment = "ElectricCharge";
		[KSPField]
		public float resourceExpCost = 0;
		[KSPField]
		public bool asteroidReports = false;
		[KSPField]
		public int planetaryMask = 524287;
		[KSPField]
		public int experimentsLimit = 1;
		[KSPField(isPersistant = true)]
		public int experimentsReturned = 0;
		[KSPField(isPersistant = true)]
		public int experimentsNumber = 0;
		[KSPField]
		public float labDataBoost = 0;
		[KSPField]
		public bool externalDeploy = false;
		[KSPField]
		public int resetLevel = 0;
		[KSPField]
		public string requiredParts = "";
		[KSPField]
		public string requiredModules = "";
		[KSPField]
		public string requiredPartsMessage = "";
		[KSPField]
		public string requiredModulesMessage = "";
		[KSPField]
		public bool excludeAtmosphere = false;
		[KSPField]
		public string excludeAtmosphereMessage = "This experiment can't be conducted within an atmosphere";

		private Animation anim;
		private Animation anim2;
		private ScienceExperiment scienceExp;
		private bool resourceOn = false;
		private int dataIndex = 0;
		private bool lastInOperableState = false;
		private string failMessage = "";

		private List<string> requiredPartList = new List<string>();
		private List<string> requiredModuleList = new List<string>();

		//Record some default values for Eeloo here to prevent the asteroid science method from screwing them up
		private string bodyNameFixed = "Eeloo";

		private List<ScienceData> initialDataList = new List<ScienceData>();
		private List<ScienceData> storedScienceReportList = new List<ScienceData>();

		private static List<Type> loadedPartModules = new List<Type>();
		private static bool partModulesLoaded = false;

		/// <summary>
		/// For external use to determine if a module can conduct science
		/// </summary>
		/// <param name="MSE">The base ModuleScienceExperiment instance</param>
		/// <returns>True if the experiment can be conducted under current conditions</returns>
		public static bool conduct(ModuleScienceExperiment MSE)
		{
			if (MSE.GetType() != typeof(DMModuleScienceAnimateGeneric))
				return false;

			try
			{
				DMModuleScienceAnimateGeneric DMMod = (DMModuleScienceAnimateGeneric)MSE;
				return DMMod.canConduct();
			}
			catch (Exception e)
			{
				Debug.LogWarning("[DM Module Science Animate] Error in casting ModuleScienceExperiment to DMModuleScienceAnimate; Invalid Part Module... : " + e);
				return false;
			}
		}

		#endregion

		#region PartModule

		public override void OnStart(StartState state)
		{
			if (!partModulesLoaded)
			{
				partModulesLoaded = true;

				try
				{
					loadedPartModules = AssemblyLoader.loadedAssemblies.Where(a => a.types.ContainsKey(typeof(PartModule))).SelectMany(b => b.types[typeof(PartModule)]).ToList();
				}
				catch (Exception e)
				{
					print("[DM Scince Animate Generic] Failure Loading Part Module List: " + e);
					loadedPartModules = new List<Type>();
				}
			}

			base.OnStart(state);
			if (!string.IsNullOrEmpty(animationName) && part.FindModelAnimators(animationName).Length > 0)
				anim = part.FindModelAnimators(animationName).FirstOrDefault();
			if (!string.IsNullOrEmpty(sampleAnim) && part.FindModelAnimators(sampleAnim).Length > 0)
			{
				anim2 = part.FindModelAnimators(sampleAnim).FirstOrDefault();
				secondaryAnimator(sampleAnim, 0f, experimentsNumber * (1f / experimentsLimit), 1f);
			}
			if (state == StartState.Editor) editorSetup();
			else
			{
				setup();
				if (IsDeployed)
				{
					if (anim != null)
						primaryAnimator(1f, 1f, WrapMode.Default, animationName, anim);
				}
			}
		}

		public override void OnSave(ConfigNode node)
		{
			node.RemoveNodes("ScienceData");
			foreach (ScienceData storedData in storedScienceReportList)
			{
				ConfigNode storedDataNode = node.AddNode("ScienceData");
				storedData.Save(storedDataNode);
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			if (node.HasNode("ScienceData"))
			{
				foreach (ConfigNode storedDataNode in node.GetNodes("ScienceData"))
				{
					ScienceData data = new ScienceData(storedDataNode);
					storedScienceReportList.Add(data);
				}
			}
		}

		new public void Update()
		{
			base.Update();

			if (Inoperable)
				lastInOperableState = true;
			else if (lastInOperableState)
			{
				lastInOperableState = false;
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, -1f * animSpeed, experimentsNumber * (1f / experimentsLimit), anim2[sampleAnim].length);
				experimentsNumber = 0;
				experimentsReturned = 0;
				Inoperable = false;
				Deployed = false;
				if (keepDeployedMode == 0) retractEvent();
			}
			eventsCheck();
		}

		private void FixedUpdate()
		{
			if (resourceOn)
			{
				if (PartResourceLibrary.Instance.GetDefinition(resourceExperiment) != null)
				{
					float cost = resourceExpCost * TimeWarp.fixedDeltaTime;
					if (part.RequestResource(resourceExperiment, cost) < cost)
					{
						StopCoroutine("WaitForAnimation");
						resourceOn = false;
						ScreenMessages.PostScreenMessage("Not enough " + resourceExperiment + ", shutting down experiment", 4f, ScreenMessageStyle.UPPER_CENTER);
						if (keepDeployedMode == 0 || keepDeployedMode == 1) retractEvent();
					}
				}
			}
		}

		public override string GetInfo()
		{
			string info = base.GetInfo();
			if (resourceExpCost > 0)
				info += ".\nRequires:\n-" + resourceExperiment + ": " + resourceExpCost.ToString() + "/s for " + waitForAnimationTime.ToString() + "s\n";
			if (experimentsLimit > 1)
				info += ".\nMax Samples: " + experimentsLimit + "\n";
			if (requiredPartList.Count > 0)
			{
				info += ".\nRequired Parts: ";
				for (int i = 0; i < requiredPartList.Count; i++)
				{
					info += requiredPartList[i] + "\n";
				}
			}
			if (requiredModuleList.Count > 0)
			{
				info += ".\nRequired Modules: ";
				for (int i = 0; i < requiredModuleList.Count; i++)
				{
					info += requiredModuleList[i] + "\n";
				}
			}
			if (excludeAtmosphere)
			{
				info += ".\nExperiment Can't Be Run In An Atmosphere";
			}
			if (!rerunnable)
				info += string.Format("Scientist Level For Reset: {0}", resetLevel);
			return info;
		}

		private void setup()
		{
			Events["deployEvent"].guiActive = showStartEvent;
			Events["retractEvent"].guiActive = showEndEvent;
			Events["toggleEvent"].guiActive = showToggleEvent;
			Events["deployEvent"].guiName = startEventGUIName;
			Events["retractEvent"].guiName = endEventGUIName;
			Events["toggleEvent"].guiName = toggleEventGUIName;
			Events["CollectDataExternalEvent"].guiName = collectActionName;
			Events["ResetExperimentExternal"].guiName = resetActionName;
			Events["ResetExperiment"].guiName = resetActionName;
			Events["DeployExperiment"].guiName = experimentActionName;
			Events["DeployExperiment"].guiActiveUnfocused = externalDeploy;
			Events["DeployExperiment"].externalToEVAOnly = externalDeploy;
			Events["DeployExperiment"].unfocusedRange = interactionRange;
			Actions["deployAction"].guiName = startEventGUIName;
			Actions["retractAction"].guiName = endEventGUIName;
			Actions["toggleAction"].guiName = toggleEventGUIName;
			Actions["DeployAction"].guiName = experimentActionName;
			if (!string.IsNullOrEmpty(experimentID))
				scienceExp = ResearchAndDevelopment.GetExperiment(experimentID);
			if (waitForAnimationTime == -1)
			{
				if (anim != null)
					waitForAnimationTime = anim[animationName].length / animSpeed;
				else
					waitForAnimationTime = 1;
			}
			if (labDataBoost == 0)
				labDataBoost = xmitDataScalar / 2;

			requiredPartList = parsePartStringList(requiredParts);
			requiredModuleList = parseModuleStringList(requiredModules);

			if (string.IsNullOrEmpty(requiredPartsMessage) && requiredPartList.Count > 0)
			{
				requiredPartsMessage = "The following parts are required to be on the vessel";

				foreach (string s in requiredPartList)
				{
					requiredPartsMessage += ": " + s;
				}
			}

			if (string.IsNullOrEmpty(requiredModulesMessage) && requiredModuleList.Count > 0)
			{
				requiredModulesMessage = "The following part modules are required to be on the vessel";

				foreach (string s in requiredModuleList)
				{
					requiredModulesMessage += ": " + s;
				}
			}
		}

		private List<string> parsePartStringList(string source)
		{
			List<string> list = new List<string>();

			if (string.IsNullOrEmpty(source))
				return list;

			string[] s = source.Split(',');

			for (int i = 0; i < s.Length; i++)
			{
				string p = s[i];

				AvailablePart a = PartLoader.getPartInfoByName(p.Replace('_', '.'));

				if (a == null)
					continue;

				list.Add(p);
			}

			return list;
		}

		private List<string> parseModuleStringList(string source)
		{
			List<string> list = new List<string>();

			if (string.IsNullOrEmpty(source))
				return list;

			string[] s = source.Split(',');

			if (s.Length <= 0)
				return list;

			for (int i = 0; i < s.Length; i++)
			{
				string m = s[i];

				for (int j = 0; j < loadedPartModules.Count; j++)
				{
					Type t = loadedPartModules[j];

					if (t == null)
						continue;

					if (t.Name == m)
					{
						list.Add(m);
						break;
					}
				}
			}

			return list;
		}

		private void editorSetup()
		{
			Actions["deployAction"].active = showStartEvent;
			Actions["retractAction"].active = showEndEvent;
			Actions["toggleAction"].active = showToggleEvent;
			Actions["deployAction"].guiName = startEventGUIName;
			Actions["retractAction"].guiName = endEventGUIName;
			Actions["toggleAction"].guiName = toggleEventGUIName;
			Actions["ResetAction"].active = experimentsLimit <= 1;
			Actions["DeployAction"].guiName = experimentActionName;
			Events["editorDeployEvent"].guiName = startEventGUIName;
			Events["editorRetractEvent"].guiName = endEventGUIName;
			Events["editorDeployEvent"].active = showEditorEvents;
			Events["editorRetractEvent"].active = false;
		}

		private void eventsCheck()
		{
			Events["ResetExperiment"].active = experimentsLimit <= 1 && storedScienceReportList.Count > 0;
			Events["ResetExperimentExternal"].active = storedScienceReportList.Count > 0 && resettableOnEVA;
			Events["CollectDataExternalEvent"].active = storedScienceReportList.Count > 0 && dataIsCollectable;
			Events["DeployExperiment"].active = !Inoperable;
			Events["DeployExperiment"].guiActiveUnfocused = !Inoperable && externalDeploy;
			Events["ReviewDataEvent"].active = storedScienceReportList.Count > 0;
			Events["ReviewInitialData"].active = initialDataList.Count > 0;
			Events["DeployExperimentExternal"].guiActiveUnfocused = false;
			Events["CleanUpExperimentExternal"].active = Inoperable;
		}

		#endregion

		#region Animators

		private void primaryAnimator(float speed, float time, WrapMode wrap, string name, Animation a)
		{
			if (a!= null)
			{
				a[name].speed = speed;
				if (!a.IsPlaying(name))
				{
					a[name].wrapMode = wrap;
					a[name].normalizedTime = time;
					a.Blend(name, 1);
				}
			}
		}

		private void secondaryAnimator(string whichAnim, float sampleSpeed, float sampleTime, float waitTime)
		{
			if (anim2 != null)
			{
				anim2[whichAnim].speed = sampleSpeed;
				anim2[whichAnim].normalizedTime = sampleTime;
				anim2.Blend(whichAnim, 1f);
				StartCoroutine(WaitForSampleAnimation(whichAnim, waitTime));
			}
		}

		private IEnumerator WaitForSampleAnimation(string whichAnimCo, float waitTimeCo)
		{
			yield return new WaitForSeconds(waitTimeCo);
			anim2[whichAnimCo].enabled = false;
		}

		[KSPEvent(guiActive = true, guiName = "Deploy", active = true)]
		public void deployEvent()
		{
			primaryAnimator(animSpeed * 1f, 0f, WrapMode.Default, animationName, anim);
			IsDeployed = !oneWayAnimation;
			Events["deployEvent"].active = oneWayAnimation;
			Events["retractEvent"].active = showEndEvent;
		}

		[KSPAction("Deploy")]
		public void deployAction(KSPActionParam param)
		{
			deployEvent();
		}

		[KSPEvent(guiActive = true, guiName = "Retract", active = false)]
		public void retractEvent()
		{
			if (oneWayAnimation) return;
			primaryAnimator(-1f * animSpeed, 1f, WrapMode.Default, animationName, anim);
			IsDeployed = false;
			Events["deployEvent"].active = showStartEvent;
			Events["retractEvent"].active = false;
		}

		[KSPAction("Retract")]
		public void retractAction(KSPActionParam param)
		{
			retractEvent();
		}

		[KSPEvent(guiActive = true, guiName = "Toggle", active = true)]
		public void toggleEvent()
		{
			if (IsDeployed) retractEvent();
			else deployEvent();
		}

		[KSPAction("Toggle")]
		public void toggleAction(KSPActionParam Param)
		{
			toggleEvent();
		}

		[KSPEvent(guiActiveEditor = true, guiName = "Deploy", active = true)]
		public void editorDeployEvent()
		{
			deployEvent();
			IsDeployed = false;
			Events["editorDeployEvent"].active = oneWayAnimation;
			Events["editorRetractEvent"].active = !oneWayAnimation;
		}

		[KSPEvent(guiActiveEditor = true, guiName = "Retract", active = false)]
		public void editorRetractEvent()
		{
			retractEvent();
			Events["editorDeployEvent"].active = true;
			Events["editorRetractEvent"].active = false;
		}

		#endregion

		#region Science Events and Actions

		new public void ResetExperiment()
		{
			if (storedScienceReportList.Count > 0)
			{
				if (experimentsLimit > 1)
				{
					if (!string.IsNullOrEmpty(sampleAnim))
						secondaryAnimator(sampleAnim, -1f * animSpeed, experimentsNumber * (1f / experimentsLimit), experimentsNumber * (anim2[sampleAnim].length / experimentsLimit));
					foreach (ScienceData data in storedScienceReportList)
						experimentsNumber--;
					storedScienceReportList.Clear();

					if (experimentsNumber < 0)
						experimentsNumber = 0;
					if (keepDeployedMode == 0)
						retractEvent();
				}
				else
				{
					if (keepDeployedMode == 0)
						retractEvent();
					storedScienceReportList.Clear();
				}
			}

			Deployed = false;
			Inoperable = false;
			lastInOperableState = false;
		}

		new public void ResetAction(KSPActionParam param)
		{
			ResetExperiment();
		}

		new public void CollectDataExternalEvent()
		{
			List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();

			if (EVACont.Count <= 0)
				return;

			if (storedScienceReportList.Count > 0)
			{
				if (EVACont.First().StoreData(new List<IScienceDataContainer> { this }, false))
					DumpAllData(storedScienceReportList);
			}
		}

		new public void ResetExperimentExternal()
		{
			ResetExperiment();
		}

		new public void DeployExperimentExternal()
		{
			DeployExperiment();
		}

		new public void CleanUpExperimentExternal()
		{
			if (!FlightGlobals.ActiveVessel.isEVA)
				return;

			if (FlightGlobals.ActiveVessel.parts[0].protoModuleCrew[0].experienceTrait.TypeName != "Scientist")
			{
				ScreenMessages.PostScreenMessage(string.Format("<b><color=orange>[{0}]: A scientist is needed to reset this experiment.</color></b>", part.partInfo.title), 6f, ScreenMessageStyle.UPPER_LEFT);
				return;
			}

			if (FlightGlobals.ActiveVessel.parts[0].protoModuleCrew[0].experienceLevel < resetLevel)
			{
				ScreenMessages.PostScreenMessage(string.Format("<b><color=orange>[{0}]: A level {1} scientist is required to reset this experiment.</color></b>", part.partInfo.title, resetLevel), 6f, ScreenMessageStyle.UPPER_LEFT);
				return;
			}

			ResetExperiment();

			ScreenMessages.PostScreenMessage(string.Format("<b><color=#99ff00ff>[{0}]: Media Restored. Module is operational again.</color></b>", part.partInfo.title), 6f, ScreenMessageStyle.UPPER_LEFT);
			
		}

		#endregion

		#region Science Experiment Setup

		//Can't use base.DeployExperiment here, we need to create our own science data and control the experiment results page
		new public void DeployExperiment()
		{
			gatherScienceData();
		}

		new public void DeployAction(KSPActionParam param)
		{
			DeployExperiment();
		}

		public void gatherScienceData(bool silent = false)
		{
			if (canConduct())
			{
				if (experimentAnimation && anim != null)
				{
					if (anim.IsPlaying(animationName)) return;
					else
					{
						if (!IsDeployed)
						{
							deployEvent();
							if (!string.IsNullOrEmpty(deployingMessage))
								ScreenMessages.PostScreenMessage(deployingMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
							if (experimentWaitForAnimation)
							{
								if (resourceExpCost > 0) resourceOn = true;
								StartCoroutine("WaitForAnimation", silent);
							}
							else runExperiment(silent);
						}
						else if (resourceExpCost > 0)
						{
							resourceOn = true;
							StartCoroutine("WaitForAnimation", silent);
						}
						else runExperiment(silent);
					}
				}
				else if (resourceExpCost > 0)
				{
					if (!string.IsNullOrEmpty(deployingMessage))
						ScreenMessages.PostScreenMessage(deployingMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
					resourceOn = true;
					StartCoroutine("WaitForAnimation", silent);
				}
				else runExperiment(silent);
			}
			else
				ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
		}

		//In case we need to wait for an animation to finish before running the experiment
		public IEnumerator WaitForAnimation(bool s)
		{
			yield return new WaitForSeconds(waitForAnimationTime);
			resourceOn = false;
			runExperiment(s);
		}

		public void runExperiment(bool silent)
		{
			ScienceData data = makeScience();
			if (data == null)
				Debug.LogError("[DM Module Science Animate] Something Went Wrong Here; Null Science Data Returned; Please Report This On The KSP Forum With Output.log Data");
			else
			{
				GameEvents.OnExperimentDeployed.Fire(data);
				if (experimentsLimit <= 1)
				{
					dataIndex = 0;
					storedScienceReportList.Add(data);
					Deployed = true;
					if (!silent)
						ReviewData();
				}
				else
				{
					initialDataList.Add(data);
					if (experimentsReturned >= experimentsLimit - 1)
						Deployed = true;
					if (silent)
						onKeepInitialData(data);
					else
						initialResultsPage();
				}
				if (keepDeployedMode == 1) retractEvent();
			}
		}

		//Create the science data
		public ScienceData makeScience()
		{
			ExperimentSituations vesselSituation = getSituation();
			string biome = getBiome(vesselSituation);
			CelestialBody mainBody = vessel.mainBody;
			DMAsteroidScienceGen newAsteroid = null;
			bool asteroid = false;

			//Check for asteroids and alter the biome and celestialbody values as necessary
			if (asteroidReports && (DMAsteroidScienceGen.AsteroidGrappled || DMAsteroidScienceGen.AsteroidNear))
			{
				bodyNameFixed = FlightGlobals.Bodies[16].bodyName;
				newAsteroid = new DMAsteroidScienceGen();
				mainBody = newAsteroid.Body;
				biome = newAsteroid.AClass;
				asteroid = true;
			}

			ScienceData data = null;
			ScienceSubject sub = ResearchAndDevelopment.GetExperimentSubject(scienceExp, vesselSituation, mainBody, biome);
			sub.title = scienceExp.experimentTitle + situationCleanup(vesselSituation, biome);

			if (asteroid)
			{
				sub.subjectValue = newAsteroid.SciMult;
				sub.scienceCap = scienceExp.scienceCap * sub.subjectValue;
				mainBody.bodyName = bodyNameFixed;
				asteroid = false;
			}

			if (sub != null)
				data = new ScienceData(scienceExp.baseValue * sub.dataScale, xmitDataScalar, vessel.VesselValues.ScienceReturn.value, sub.id, sub.title, false, part.flightID);
			return data;
		}

		public string getBiome(ExperimentSituations s)
		{
			if (scienceExp.BiomeIsRelevantWhile(s))
			{
				switch (vessel.landedAt)
				{
					case "":
						if (vessel.mainBody.BiomeMap != null)
							return vessel.mainBody.BiomeMap.GetAtt(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad).name;
						else
							return "";
					default:
						return Vessel.GetLandedAtString(vessel.landedAt);
				}
			}
			else return "";
		}

		public bool canConduct()
		{
			failMessage = "";
			if (Inoperable)
			{
				failMessage = "Experiment is no longer functional; must be reset at a science lab or returned to Kerbin";
				return false;
			}
			else if (Deployed)
			{
				failMessage = storageFullMessage;
				return false;
			}
			else if ((experimentsNumber >= experimentsLimit) && experimentsLimit > 1)
			{
				if (!string.IsNullOrEmpty(storageFullMessage))
					failMessage = storageFullMessage;
				return false;
			}
			else if (storedScienceReportList.Count > 0 && experimentsLimit <= 1)
			{
				if (!string.IsNullOrEmpty(storageFullMessage))
					failMessage = storageFullMessage;
				return false;
			}
			else if (!planetaryScienceIndex.planetConfirm(planetaryMask, asteroidReports))
			{
				if (!string.IsNullOrEmpty(planetFailMessage))
					failMessage = planetFailMessage;
				return false;
			}
			else if (!scienceExp.IsAvailableWhile(getSituation(), vessel.mainBody))
			{
				if (!string.IsNullOrEmpty(customFailMessage))
					failMessage = customFailMessage;
				return false;
			}
			else if (excludeAtmosphere && vessel.mainBody.atmosphere)
			{
				if (!string.IsNullOrEmpty(excludeAtmosphereMessage))
					failMessage = excludeAtmosphereMessage;
				return false;
			}
			else if (scienceExp.requireAtmosphere && !vessel.mainBody.atmosphere)
			{
				failMessage = customFailMessage;
				return false;
			}
			else if (requiredPartList.Count > 0)
			{
				for (int i = 0; i < requiredPartList.Count; i++)
				{
					string partName = requiredPartList[i];

					if (string.IsNullOrEmpty(partName))
						continue;

					if (!VesselUtilities.VesselHasPartName(partName, vessel))
					{
						failMessage = requiredPartsMessage;
						return false;
					}
				}
			}
			else if (requiredModuleList.Count > 0)
			{
				for (int i = 0; i < requiredModuleList.Count; i++)
				{
					string moduleName = requiredModuleList[i];

					if (string.IsNullOrEmpty(moduleName))
						continue;

					if (!VesselUtilities.VesselHasModuleName(moduleName, vessel))
					{
						failMessage = requiredModulesMessage;
						return false;
					}
				}
			}

			if (FlightGlobals.ActiveVessel.isEVA)
			{
				if (!ScienceUtil.RequiredUsageExternalAvailable(part.vessel, FlightGlobals.ActiveVessel, (ExperimentUsageReqs)usageReqMaskExternal, scienceExp, ref usageReqMessage))
				{
					failMessage = usageReqMessage;
					return false;
				}
			}

			return true;
		}

		//Get our experimental situation based on the vessel's current flight situation, fix stock bugs with aerobraking and reentry.
		public ExperimentSituations getSituation()
		{
			//Check for asteroids, return values that should sync with existing parts
			if (asteroidReports && DMAsteroidScienceGen.AsteroidGrappled) return ExperimentSituations.SrfLanded;
			if (asteroidReports && DMAsteroidScienceGen.AsteroidNear) return ExperimentSituations.InSpaceLow;
			switch (vessel.situation)
			{
				case Vessel.Situations.LANDED:
				case Vessel.Situations.PRELAUNCH:
					return ExperimentSituations.SrfLanded;
				case Vessel.Situations.SPLASHED:
					return ExperimentSituations.SrfSplashed;
				default:
					if (vessel.mainBody.atmosphere && vessel.altitude < vessel.mainBody.atmosphereDepth)
					{
						if (vessel.altitude < vessel.mainBody.scienceValues.flyingAltitudeThreshold)
							return ExperimentSituations.FlyingLow;
						else
							return ExperimentSituations.FlyingHigh;
					}
					if (vessel.altitude < vessel.mainBody.scienceValues.spaceAltitudeThreshold)
						return ExperimentSituations.InSpaceLow;
					else
						return ExperimentSituations.InSpaceHigh;
			}
		}

		//This is for the title bar of the experiment results page
		public string situationCleanup(ExperimentSituations expSit, string b)
		{
			//Add some asteroid specefic results
			if (asteroidReports && DMAsteroidScienceGen.AsteroidGrappled) return " from the surface of a " + b + " asteroid";
			if (asteroidReports && DMAsteroidScienceGen.AsteroidNear) return " while in space near a " + b + " asteroid";
			if (vessel.landedAt != "") return " from " + b;
			if (b == "")
			{
				switch (expSit)
				{
					case ExperimentSituations.SrfLanded:
						return " from  " + vessel.mainBody.theName + "'s surface";
					case ExperimentSituations.SrfSplashed:
						return " from " + vessel.mainBody.theName + "'s oceans";
					case ExperimentSituations.FlyingLow:
						return " while flying at " + vessel.mainBody.theName;
					case ExperimentSituations.FlyingHigh:
						return " from " + vessel.mainBody.theName + "'s upper atmosphere";
					case ExperimentSituations.InSpaceLow:
						return " while in space near " + vessel.mainBody.theName;
					default:
						return " while in space high over " + vessel.mainBody.theName;
				}
			}
			else
			{
				switch (expSit)
				{
					case ExperimentSituations.SrfLanded:
						return " from " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.SrfSplashed:
						return " from " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.FlyingLow:
						return " while flying over " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.FlyingHigh:
						return " from the upper atmosphere over " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.InSpaceLow:
						return " from space just above " + vessel.mainBody.theName + "'s " + b;
					default:
						return " while in space high over " + vessel.mainBody.theName + "'s " + b;
				}
			}
		}

		#endregion

		#region IScienceDataContainer methods

		ScienceData[] IScienceDataContainer.GetData()
		{
			return storedScienceReportList.ToArray();
		}

		int IScienceDataContainer.GetScienceCount()
		{
			return storedScienceReportList.Count;
		}

		bool IScienceDataContainer.IsRerunnable()
		{
			return IsRerunnable();
		}

		void IScienceDataContainer.ReturnData(ScienceData data)
		{
			ReturnData(data);
		}

		void IScienceDataContainer.ReviewData()
		{
			ReviewData();
		}

		void IScienceDataContainer.ReviewDataItem(ScienceData data)
		{
			ReviewData();
		}

		void IScienceDataContainer.DumpData(ScienceData data)
		{
			DumpData(data);
		}

		new private void ReturnData(ScienceData data)
		{
			if (data == null)
				return;

			storedScienceReportList.Add(data);

			experimentsReturned--;

			if (experimentsReturned < 0)
				experimentsReturned = 0;

			Inoperable = false;

			if (experimentsLimit <= 1)
				Deployed = true;
			else
				Deployed = experimentsNumber >= experimentsLimit;
		}

		private void DumpAllData(List<ScienceData> data)
		{
			foreach (ScienceData d in data)
				experimentsReturned++;
			Inoperable = !IsRerunnable();
			Deployed = Inoperable;
			data.Clear();
		}

		new private void DumpData(ScienceData data)
		{
			if (storedScienceReportList.Count > 0)
			{
				experimentsReturned++;
				Inoperable = !IsRerunnable();
				Deployed = Inoperable;
				storedScienceReportList.Remove(data);
			}
		}

		private void DumpInitialData(ScienceData data)
		{
			if (initialDataList.Count > 0)
			{
				experimentsReturned++;
				Inoperable = !IsRerunnable();
				Deployed = Inoperable;
				initialDataList.Remove(data);
			}
		}

		new private bool IsRerunnable()
		{
			if (rerunnable)
				return true;
			else
				return experimentsReturned < experimentsLimit;
		}

		#endregion

		#region Experiment Results Control

		private void newResultPage()
		{
			if (storedScienceReportList.Count > 0)
			{
				ScienceData data = storedScienceReportList[dataIndex];
				ExperimentResultDialogPage page = new ExperimentResultDialogPage(part, data, data.transmitValue, labDataBoost, (experimentsReturned >= (experimentsLimit - 1)) && !rerunnable, transmitWarningText, true, new ScienceLabSearch(vessel, data), new Callback<ScienceData>(onDiscardData), new Callback<ScienceData>(onKeepData), new Callback<ScienceData>(onTransmitData), new Callback<ScienceData>(onSendToLab));
				ExperimentsResultDialog.DisplayResult(page);
			}
		}

		new public void ReviewData()
		{
			dataIndex = 0;
			foreach (ScienceData data in storedScienceReportList)
			{
				newResultPage();
				dataIndex++;
			}
		}

		new public void ReviewDataEvent()
		{
			ReviewData();
		}

		[KSPEvent(guiActive = true, guiName = "Review Initial Data", active = false)]
		public void ReviewInitialData()
		{
			if (initialDataList.Count > 0)
				initialResultsPage();
		}

		private void initialResultsPage()
		{
			if (initialDataList.Count > 0)
			{
				ScienceData data = initialDataList[0];
				ExperimentResultDialogPage page = new ExperimentResultDialogPage(part, data, data.transmitValue, labDataBoost, (experimentsReturned >= (experimentsLimit - 1)) && !rerunnable, transmitWarningText, true, new ScienceLabSearch(vessel, data), new Callback<ScienceData>(onDiscardInitialData), new Callback<ScienceData>(onKeepInitialData), new Callback<ScienceData>(onTransmitInitialData), new Callback<ScienceData>(onSendInitialToLab));
				ExperimentsResultDialog.DisplayResult(page);
			}
		}

		private void onDiscardData(ScienceData data)
		{
			if (storedScienceReportList.Count > 0)
			{
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, -1f * animSpeed, experimentsNumber * (1f / experimentsLimit), anim2[sampleAnim].length / experimentsLimit);
				storedScienceReportList.Remove(data);
				if (keepDeployedMode == 0) retractEvent();
				experimentsNumber--;
				if (experimentsNumber < 0)
					experimentsNumber = 0;
				Deployed = false;
			}
		}

		private void onKeepData(ScienceData data)
		{
		}

		private void onTransmitData(ScienceData data)
		{
			List<IScienceDataTransmitter> tranList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if (tranList.Count > 0 && storedScienceReportList.Count > 0)
			{
				tranList.OrderBy(ScienceUtil.GetTransmitterScore).First().TransmitData(new List<ScienceData> { data });
				DumpData(data);
			}
			else
				ScreenMessages.PostScreenMessage("No Comms Devices on this vessel. Cannot Transmit Data.", 3f, ScreenMessageStyle.UPPER_CENTER);
		}

		private void onSendToLab(ScienceData data)
		{
			ScienceLabSearch labSearch = new ScienceLabSearch(vessel, data);

			if (labSearch.NextLabForDataFound)
			{
				StartCoroutine(labSearch.NextLabForData.ProcessData(data, null));
				DumpData(data);
			}
			else
				labSearch.PostErrorToScreen();
		}

		private void onDiscardInitialData(ScienceData data)
		{
			if (initialDataList.Count > 0)
			{
				initialDataList.Remove(data);
				if (keepDeployedMode == 0) retractEvent();
				Deployed = false;
			}
		}

		private void onKeepInitialData(ScienceData data)
		{
			if (experimentsNumber >= experimentsLimit)
			{
				ScreenMessages.PostScreenMessage(storageFullMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
				initialResultsPage();
			}
			else if (initialDataList.Count > 0)
			{
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, animSpeed, experimentsNumber * (1f / experimentsLimit), anim2[sampleAnim].length / experimentsLimit);
				storedScienceReportList.Add(data);
				initialDataList.Remove(data);
				experimentsNumber++;
			}
		}

		private void onTransmitInitialData(ScienceData data)
		{
			List<IScienceDataTransmitter> tranList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if (tranList.Count > 0 && initialDataList.Count > 0)
			{
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, animSpeed, experimentsNumber * (1f / experimentsLimit), anim2[sampleAnim].length / experimentsLimit);
				tranList.OrderBy(ScienceUtil.GetTransmitterScore).First().TransmitData(new List<ScienceData> { data });
				DumpInitialData(data);
				experimentsNumber++;
			}
			else
				ScreenMessages.PostScreenMessage("No Comms Devices on this vessel. Cannot Transmit Data.", 3f, ScreenMessageStyle.UPPER_CENTER);
		}

		private void onSendInitialToLab(ScienceData data)
		{
			ScienceLabSearch labSearch = new ScienceLabSearch(vessel, data);

			if (labSearch.NextLabForDataFound)
			{
				StartCoroutine(labSearch.NextLabForData.ProcessData(data, null));
				DumpData(data);
			}
			else
				labSearch.PostErrorToScreen();
		}

		#endregion

	}
}
