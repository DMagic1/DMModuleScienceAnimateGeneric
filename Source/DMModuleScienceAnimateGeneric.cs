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

		private Animation anim;
		private Animation anim2;
		private ScienceExperiment scienceExp;
		private DMAsteroidScienceGen newAsteroid = null;
		private bool resourceOn = false;
		private int dataIndex = 0;
		private bool lastInOperableState = false;
		private string failMessage = "";

		//Record some default values for Eeloo here to prevent the asteroid science method from screwing them up
		private const string bodyNameFixed = "Eeloo";

		private List<ScienceData> initialDataList = new List<ScienceData>();
		private List<ScienceData> storedScienceReportList = new List<ScienceData>();

		#endregion

		#region PartModule

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			if (part.FindModelAnimators(animationName).Length > 0 && !string.IsNullOrEmpty(animationName))
				anim = part.FindModelAnimators(animationName).First();
			if (part.FindModelAnimators(sampleAnim).Length > 0 && !string.IsNullOrEmpty(sampleAnim))
			{
				anim2 = part.FindModelAnimators(sampleAnim).First();
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

		private void Update()
		{
			if (resourceOn)
			{
				if (PartResourceLibrary.Instance.GetDefinition(resourceExperiment) != null)
				{
					float cost = resourceExpCost * TimeWarp.deltaTime;
					if (part.RequestResource(resourceExperiment, cost) < cost)
					{
						StopCoroutine("WaitForAnimation");
						resourceOn = false;
						ScreenMessages.PostScreenMessage("Not enough " + resourceExperiment + ", shutting down experiment", 4f, ScreenMessageStyle.UPPER_CENTER);
						if (keepDeployedMode == 0 || keepDeployedMode == 1) retractEvent();
					}
				}
			}
			if (Inoperable)
				lastInOperableState = true;
			else if (lastInOperableState)
			{
				lastInOperableState = false;
				if (!string.IsNullOrEmpty(sampleAnim))
					print("");
				experimentsNumber = 0;
				experimentsReturned = 0;
				if (keepDeployedMode == 0) retractEvent();
			}
			eventsCheck();
		}

		public override string GetInfo()
		{
			if (resourceExpCost > 0)
			{
				string info = base.GetInfo();
				info += ".\nRequires:\n-" + resourceExperiment + ": " + resourceExpCost.ToString() + "/s for " + waitForAnimationTime.ToString() + "s\n";
				if (experimentsLimit > 1)
					info += "Max Samples: " + experimentsLimit + "\n";
				return info;
			}
			else return base.GetInfo();
		}

		private void setup()
		{
			Events["deployEvent"].guiActive = showStartEvent;
			Events["retractEvent"].guiActive = showEndEvent;
			Events["toggleEvent"].guiActive = showToggleEvent;
			Events["deployEvent"].guiName = startEventGUIName;
			Events["retractEvent"].guiName = endEventGUIName;
			Events["toggleEvent"].guiName = toggleEventGUIName;
			Events["DeployExperiment"].guiName = experimentActionName;
			Events["DeployExperiment"].guiActiveUnfocused = externalDeploy;
			Events["DeployExperiment"].externalToEVAOnly = externalDeploy;
			Events["DeployExperiment"].unfocusedRange = interactionRange;
			if (waitForAnimationTime == -1)
				waitForAnimationTime = anim[animationName].length / animSpeed;
			if (experimentID != null)
				scienceExp = ResearchAndDevelopment.GetExperiment(experimentID);
			if (FlightGlobals.Bodies[16].bodyName != "Eeloo")
				FlightGlobals.Bodies[16].bodyName = bodyNameFixed;
			if (labDataBoost == 0)
				labDataBoost = xmitDataScalar / 2;
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
			Events["ResetExperimentExternal"].active = storedScienceReportList.Count > 0;
			Events["CollectDataExternalEvent"].active = storedScienceReportList.Count > 0;
			Events["DeployExperiment"].active = !Inoperable;
			Events["DeployExperiment"].guiActiveUnfocused = !Inoperable && externalDeploy;
			Events["ReviewDataEvent"].active = storedScienceReportList.Count > 0;
			Events["ReviewInitialData"].active = initialDataList.Count > 0;
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
					ResetExperimentExternal();
				else
				{
					if (keepDeployedMode == 0) retractEvent();
					storedScienceReportList.Clear();
				}
			}
		}

		new public void ResetAction(KSPActionParam param)
		{
			ResetExperiment();
		}

		new public void CollectDataExternalEvent()
		{
			List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();
			if (storedScienceReportList.Count > 0)
			{
				if (EVACont.First().StoreData(new List<IScienceDataContainer> { this }, false))
					foreach (ScienceData data in storedScienceReportList)
						DumpData(data);
			}
		}

		new public void ResetExperimentExternal()
		{
			if (storedScienceReportList.Count > 0)
			{
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, -1f * animSpeed, experimentsNumber * (1f / experimentsLimit), experimentsNumber * (anim2[sampleAnim].length / experimentsLimit));
				foreach (ScienceData data in storedScienceReportList)
				{
					storedScienceReportList.Remove(data);
					experimentsNumber--;
				}
				if (experimentsNumber < 0)
					experimentsNumber = 0;
				if (keepDeployedMode == 0) retractEvent();
			}
		}

		#endregion

		#region Science Experiment Setup

		//Can't use base.DeployExperiment here, we need to create our own science data and control the experiment results page
		new public void DeployExperiment()
		{
			if (canConduct())
			{
				if (experimentAnimation)
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
								StartCoroutine("WaitForAnimation", waitForAnimationTime);
							}
							else runExperiment();
						}
						else if (resourceExpCost > 0)
						{
							resourceOn = true;
							StartCoroutine("WaitForAnimation", waitForAnimationTime);
						}
						else runExperiment();
					}
				}
				else if (resourceExpCost > 0)
				{
					if (!string.IsNullOrEmpty(deployingMessage))
						ScreenMessages.PostScreenMessage(deployingMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
					resourceOn = true;
					StartCoroutine("WaitForAnimation", waitForAnimationTime);
				}
				else runExperiment();
			}
			else
				ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
		}

		new public void DeployAction(KSPActionParam param)
		{
			DeployExperiment();
		}

		//In case we need to wait for an animation to finish before running the experiment
		public IEnumerator WaitForAnimation(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			resourceOn = false;
			runExperiment();
		}

		public void runExperiment()
		{
			ScienceData data = makeScience();
			if (experimentsLimit <= 1)
			{
				dataIndex = 0;
				storedScienceReportList.Add(data);
				ReviewData();
			}
			else
			{
				initialDataList.Add(data);
				initialResultsPage();
			}
			if (keepDeployedMode == 1) retractEvent();
		}

		//Create the science data
		public ScienceData makeScience()
		{
			ExperimentSituations vesselSituation = getSituation();
			string biome = getBiome(vesselSituation);
			CelestialBody mainBody = vessel.mainBody;
			bool asteroid = false;

			//Check for asteroids and alter the biome and celestialbody values as necessary
			if (asteroidReports && (DMAsteroidScienceGen.asteroidGrappled() || DMAsteroidScienceGen.asteroidNear()))
			{
				newAsteroid = new DMAsteroidScienceGen();
				mainBody = newAsteroid.body;
				biome = newAsteroid.aClass;
				asteroid = true;
			}

			ScienceData data = null;
			ScienceExperiment exp = ResearchAndDevelopment.GetExperiment(experimentID);
			ScienceSubject sub = ResearchAndDevelopment.GetExperimentSubject(exp, vesselSituation, mainBody, biome);
			sub.title = exp.experimentTitle + situationCleanup(vesselSituation, biome);

			if (asteroid)
			{
				sub.subjectValue = newAsteroid.sciMult;
				sub.scienceCap = exp.scienceCap * sub.subjectValue;
				mainBody.bodyName = bodyNameFixed;
				asteroid = false;
			}
			else
			{
				sub.subjectValue = fixSubjectValue(vesselSituation, mainBody, sub.subjectValue);
				sub.scienceCap = exp.scienceCap * sub.subjectValue;
			}

			if (sub != null)
				data = new ScienceData(exp.baseValue * sub.dataScale, xmitDataScalar, 0.5f, sub.id, sub.title);
			return data;
		}

		private float fixSubjectValue(ExperimentSituations s, CelestialBody b, float f)
		{
			float subV = f;
			if (s == ExperimentSituations.SrfLanded) subV = b.scienceValues.LandedDataValue;
			else if (s == ExperimentSituations.SrfSplashed) subV = b.scienceValues.SplashedDataValue;
			else if (s == ExperimentSituations.FlyingLow) subV = b.scienceValues.FlyingLowDataValue;
			else if (s == ExperimentSituations.FlyingHigh) subV = b.scienceValues.FlyingHighDataValue;
			else if (s == ExperimentSituations.InSpaceLow) subV = b.scienceValues.InSpaceLowDataValue;
			else if (s == ExperimentSituations.InSpaceHigh) subV = b.scienceValues.InSpaceHighDataValue;
			return subV;
		}

		public string getBiome(ExperimentSituations s)
		{
			if (scienceExp.BiomeIsRelevantWhile(s))
			{
				switch (vessel.landedAt)
				{
					case "LaunchPad":
						return vessel.landedAt;
					case "Runway":
						return vessel.landedAt;
					case "KSC":
						return vessel.landedAt;
					default:
						return FlightGlobals.currentMainBody.BiomeMap.GetAtt(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad).name;
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
			if (!scienceExp.IsAvailableWhile(getSituation(), vessel.mainBody))
			{
				if (!string.IsNullOrEmpty(customFailMessage))
					failMessage = customFailMessage;
				return false;
			}
			else
				return true;
		}

		//Get our experimental situation based on the vessel's current flight situation, fix stock bugs with aerobraking and reentry.
		public ExperimentSituations getSituation()
		{
			//Check for asteroids, return values that should sync with existing parts
			if (asteroidReports && DMAsteroidScienceGen.asteroidGrappled()) return ExperimentSituations.SrfLanded;
			if (asteroidReports && DMAsteroidScienceGen.asteroidNear()) return ExperimentSituations.InSpaceLow;
			switch (vessel.situation)
			{
				case Vessel.Situations.LANDED:
				case Vessel.Situations.PRELAUNCH:
					return ExperimentSituations.SrfLanded;
				case Vessel.Situations.SPLASHED:
					return ExperimentSituations.SrfSplashed;
				default:
					if (vessel.altitude < (vessel.mainBody.atmosphereScaleHeight * 1000 * Math.Log(1e6)) && vessel.mainBody.atmosphere)
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
			if (asteroidReports && DMAsteroidScienceGen.asteroidGrappled()) return " from the surface of a " + b + " asteroid";
			if (asteroidReports && DMAsteroidScienceGen.asteroidNear()) return " while in space near a " + b + " asteroid";
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

		new private void DumpData(ScienceData data)
		{
			if (storedScienceReportList.Count > 0)
			{
				experimentsReturned++;
				Inoperable = !IsRerunnable();
				storedScienceReportList.Remove(data);
			}
		}

		private void DumpInitialData(ScienceData data)
		{
			if (initialDataList.Count > 0)
			{
				experimentsReturned++;
				Inoperable = !IsRerunnable();
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
				ExperimentResultDialogPage page = new ExperimentResultDialogPage(part, data, data.transmitValue, labDataBoost, (experimentsReturned >= (experimentsLimit - 1)) && !rerunnable, transmitWarningText, true, data.labBoost < 1 && checkLabOps() && xmitDataScalar < 1, new Callback<ScienceData>(onDiscardData), new Callback<ScienceData>(onKeepData), new Callback<ScienceData>(onTransmitData), new Callback<ScienceData>(onSendToLab));
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
				ExperimentResultDialogPage page = new ExperimentResultDialogPage(part, data, data.transmitValue, labDataBoost, (experimentsReturned >= (experimentsLimit - 1)) && !rerunnable, transmitWarningText, true, data.labBoost < 1 && checkLabOps() && xmitDataScalar < 1, new Callback<ScienceData>(onDiscardInitialData), new Callback<ScienceData>(onKeepInitialData), new Callback<ScienceData>(onTransmitInitialData), new Callback<ScienceData>(onSendInitialToLab));
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
			else ScreenMessages.PostScreenMessage("No transmitters available on this vessel.", 4f, ScreenMessageStyle.UPPER_LEFT);
		}

		private void onSendToLab(ScienceData data)
		{
			List<ModuleScienceLab> labList = vessel.FindPartModulesImplementing<ModuleScienceLab>();
			if (checkLabOps() && storedScienceReportList.Count > 0)
				labList.OrderBy(ScienceUtil.GetLabScore).First().StartCoroutine(labList.First().ProcessData(data, new Callback<ScienceData>(onComplete)));
			else ScreenMessages.PostScreenMessage("No operational lab modules on this vessel. Cannot analyze data.", 4f, ScreenMessageStyle.UPPER_CENTER);
		}

		private void onComplete(ScienceData data)
		{
			ReviewData();
		}

		private bool checkLabOps()
		{
			List<ModuleScienceLab> labList = vessel.FindPartModulesImplementing<ModuleScienceLab>();
			for (int i = 0; i < labList.Count; i++)
				if (labList[i].IsOperational())
					return true;
			return false;
		}

		private void onDiscardInitialData(ScienceData data)
		{
			if (initialDataList.Count > 0)
			{
				initialDataList.Remove(data);
				if (keepDeployedMode == 0) retractEvent();
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
				ScreenMessages.PostScreenMessage("No transmitters available on this vessel.", 4f, ScreenMessageStyle.UPPER_LEFT);
		}

		private void onSendInitialToLab(ScienceData data)
		{
			List<ModuleScienceLab> labList = vessel.FindPartModulesImplementing<ModuleScienceLab>();
			if (checkLabOps() && initialDataList.Count > 0)
				labList.OrderBy(ScienceUtil.GetLabScore).First().StartCoroutine(labList.First().ProcessData(data, new Callback<ScienceData>(onInitialComplete)));
			else
				ScreenMessages.PostScreenMessage("No operational lab modules on this vessel. Cannot analyze data.", 4f, ScreenMessageStyle.UPPER_CENTER);
		}

		private void onInitialComplete(ScienceData data)
		{
			initialResultsPage();
		}

		#endregion

	}
}
