using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NuclearOption.Networking;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace NoEnginePatch;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class NoEnginePatch : BaseUnityPlugin
{
    private readonly string[] targetPrefabNames =
    {
        "COIN",
        "Fighter1",
        "engine",
        "engine_FL",
        "engine_FR",
        "engine_L",
        "engine_R",
        "engine_RL",
        "engine_RR",
        "hub_FL",
        "hub_FR",
        "hub_L",
        "hub_R",
        "hub_RL",
        "hub_RR",
        "hub_U",
        "hstab_L",
        "hstab_R",
        "propHub",
        "rotorhub",
        "tailrotor"
    };

    private readonly string[] targetPartNames =
    {
        "CAS1",
        "COIN",
        "Darkreach",
        "EW1",
        "Fighter1",
        "Multirole1",
        "QuadVTOL1",
        "SmallFighter1",
        "UtilityHelo1",
        "aileron_L",
        "aileron_R",
        "canard_L",
        "canard_R",
        "canopyFrame",
        "cheek_L",
        "cheek_R",
        "cockpit",
        "cockpit_F",
        "cockpit_R",
        "engine",
        "engine_FL",
        "engine_FR",
        "engine_L",
        "engine_Mount_L",
        "engine_Mount_R",
        "engine_R",
        "engine_RL",
        "engine_RR",
        "elevator",
        "elevator_L",
        "elevator_R",
        "exhaust",
        "flap_FL",
        "flap_FR",
        "flap_L",
        "flap_R",
        "flap_RL",
        "flap_RR",
        "floor",
        "floor_F",
        "floor_FF",
        "floor_R",
        "fuelTank_F",
        "fuselage",
        "fuselage_F",
        "fuselage_FL",
        "fuselage_FR",
        "fuselage_L",
        "fuselage_R",
        "fuselage_RL",
        "fuselage_RR",
        "fuselage_RRL",
        "fuselage_RRR",
        "gearBay_L",
        "gearBay_R",
        "gearbay_L",
        "gearbay_R",
        "gearbox_F",
        "gearbox_R",
        "hub_FL",
        "hub_FR",
        "hub_L",
        "hub_R",
        "hub_RL",
        "hub_RR",
        "hub_U",
        "hstab",
        "hstab_L",
        "hstab_R",
        "intake",
        "intake_F",
        "intake_L",
        "intake_R",
        "liftFan",
        "nacelle_L",
        "nacelle_R",
        "nozzle",
        "nozzle_L",
        "nozzle_R",
        "nose",
        "pylon_L",
        "pylon_R",
        "propHub",
        "ramp",
        "roof_F",
        "rudder",
        "rudder_L",
        "rudder_R",
        "sponson_L",
        "sponson_R",
        "tail",
        "tail_L",
        "tail_R",
        "tailboom_L",
        "tailboom_R",
        "trainer",
        "vstab1",
        "vstab1_L",
        "vstab1_R",
        "vstab2",
        "vstab2_L",
        "vstab2_R",
        "vstab_L",
        "vstab_R",
        "wall_L",
        "wall_R",
        "wall_RL",
        "wall_RR",
        "weaponBay_L",
        "weaponBay_R",
        "weaponbay_L",
        "weaponbay_R",
        "wing1_FL",
        "wing1_FR",
        "wing1_L",
        "wing1_R",
        "wing1_RL",
        "wing1_RR",
        "wing1a_L",
        "wing1a_R",
        "wing1b_L",
        "wing1b_R",
        "wing2_FL",
        "wing2_FR",
        "wing2_L",
        "wing2_R",
        "wing2_RL",
        "wing2_RR",
        "wing2a_L",
        "wing2a_R",
        "wing2b_L",
        "wing2b_R",
        "wing3_L",
        "wing3_R",
        "wingRoot_L",
        "wingRoot_R",
        "wingroot1_L",
        "wingroot1_R",
        "wingroot2_L",
        "wingroot2_R",
        "wingtip_L",
        "wingtip_R"
    };

    private bool done = false;
    void Update()
    {
        if (done) return;
        var allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
        var matchedPrefabs = allPrefabs
            .Where(go => targetPrefabNames.Any(t => go.name.Equals(t, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (matchedPrefabs.Count == 0)
        {
            Logger.LogInfo("Waiting for target prefabs to load...");
            return;
        }
        foreach (var prefab in matchedPrefabs)
        {
            ModifyPrefab(prefab);
        }

        Logger.LogInfo($"Searched {matchedPrefabs.Count} target prefabs.");
        var matchedParts = allPrefabs
            .Where(go => targetPartNames.Any(t => go.name.Equals(t, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var part in matchedParts)
        {
            ModifyPart(part);
        }
        done = true;
    }
    private void ModifyPart(GameObject part)
    {
        Logger.LogInfo($"Searching part: {part.name}");
        var aeropart = part.GetComponent<AeroPart>();
        if (aeropart == null)
        {
            Debug.LogError("Cannot find field 'flyByWire' in Part Joint");
            return;
        }
        aeropart.wingArea *= 3f; //增大机翼面积，提升升力
        PartJoint[] partJoints = aeropart.joints;
        if (partJoints == null)
        {
            Debug.LogError("Cannot find field 'joints' in AeroPart");
            return;
        }
        foreach (PartJoint partjoint in partJoints)
        {
            partjoint.breakForce = float.MaxValue;
            partjoint.breakTorque = float.MaxValue;
        }
    }
    private void ModifyPrefab(GameObject prefab)
    {
        Logger.LogInfo($"Searching prefab: {prefab.name}");
        var turbojet = prefab.GetComponent<Turbojet>();
        if (turbojet != null)
        {
            if (prefab.name == "Fighter1") //FS12的引擎
            {
                Logger.LogInfo($"Engine of FS-12 found.");
                setEngineParam(turbojet, 400000, 1020, 1020);
                Logger.LogInfo($"Modified engine of FS-12.");
            }
            else if (prefab.name == "engine") //FS20的引擎
            {
                Logger.LogInfo($"Engine of FS-20 found.");
                setEngineParam(turbojet, 400000, 1020, 1020);
                Logger.LogInfo($"Modified engine of FS-20.");

            }
            else if (prefab.name == "engine_L" || prefab.name == "engine_R")
            {
                //使用引擎最大推力辅助判断这是谁的引擎
                if (turbojet.maxThrust == 100000) //KR引擎
                {
                    Logger.LogInfo($"Engine of KR-67 found.");
                    setEngineParam(turbojet, 300000, 2020, 2020);
                    Logger.LogInfo($"Modified engine of KR-67.");
                }
                else if (turbojet.maxThrust == 28000) //TA30引擎
                {
                    Logger.LogInfo($"Engine of T/A-30 found.");
                    setEngineParam(turbojet, 100000, 1020, 1020);
                    Logger.LogInfo($"Modified engine of T/A-30.");
                }
                else if (turbojet.maxThrust == 210000) //SFB81引擎
                {
                    Logger.LogInfo($"Engine of SFB-81 found.");
                    setEngineParam(turbojet, 840000, 1020, 1020);
                    Logger.LogInfo($"Modified engine of SFB-81.");
                }
                else if (turbojet.maxThrust == 65000) //EW25引擎
                {
                    Logger.LogInfo($"Engine of EW-25 found.");
                    setEngineParam(turbojet, 650000, 1020, 1020);
                    Logger.LogInfo($"Modified engine of EW-25.");
                }
            }
        }
        var csprop = prefab.GetComponent<ConstantSpeedProp>();
        if (csprop != null)
        {
            if (prefab.name == "propHub") //CI-22的引擎
            {
                Logger.LogInfo($"Engine of CI-22 found.");
                setCSPropParam(csprop, 30000000, 1200000, 3000);
                Logger.LogInfo($"Modified engine of CI-22.");
            }
            if (prefab.name == "hub_FL" || prefab.name == "hub_FR" || prefab.name == "hub_RL" || prefab.name == "hub_RR") //VL-49的引擎
            {
                Logger.LogInfo($"Engine of VL-49 found.");
                setCSPropParam(csprop, 9450000, 8000000, 700);
                Logger.LogInfo($"Modified engine of VL-49.");
            }
        }

        var propfan = prefab.GetComponent<PropFan>();
        if (propfan != null)
        {
            if (prefab.name == "hub_R" || prefab.name == "hub_L") //NOTA-10的引擎
            {
                Logger.LogInfo($"Engine of NO TA-10 found.");
                setPropFanParam(propfan, 34000000);
                Logger.LogInfo($"Modified engine of NO TA-10.");
            }
        }
        var turbine = prefab.GetComponent<TurbineEngine>();
        if (turbine != null)
        {
            if (prefab.name == "COIN")
            {   
                var aircraft = prefab.GetComponent<Aircraft>();
                if (aircraft != null)
                {
                    aircraft.controlsFilter.flyByWire.Enabled = false;
                }
                turbine.maxPower = 30000000f;
                Logger.LogInfo($"Enchanted engine of CI-22.");
            }
            if (prefab.name == "engine_FL" || prefab.name == "engine_FR" || prefab.name == "engine_RL" || prefab.name == "engine_RR")
            {
                turbine.maxPower = 94500000f;
                Logger.LogInfo($"Enchanted engine of VL-49.");
            }

            if (prefab.name == "engine_L" || prefab.name == "engine_R")
            {
                turbine.maxPower = 34000000f;
                Logger.LogInfo($"Enchanted engine of NOTA10.");
            }
        }
        var ductedFan = prefab.GetComponent<DuctedFan>();
        if (ductedFan != null)
        {
            if (prefab.name == "hstab_L" || prefab.name == "hstab_R")
            {
                ductedFan.maxThrust *= 10f;
                ductedFan.nominalPower *= 10f;
                ductedFan.maxPower *= 10f;
                Logger.LogInfo($"Enchanted lift fan of QuadVTOL1.");
            }
        }


    }
    private void setEngineParam(Turbojet target, float maxThrust, float maxSpeed, float maxVectorSpeed)
    /*
     * maxThrust: 引擎的最大出力，单位N（牛）
     * maxSpeed: 引擎的最大速度，软限制，接近/达到/超过该值时会减少引擎出力
     * maxVectorSpeed: 阈值限制，速度(m/s)超过该值锁定矢量系统。由于无矢量机型的推力方向变换矢量为0，所以对非矢量引擎，该值无效，推荐设置为0
     */
    {
        target.maxThrust = maxThrust;
        target.maxSpeed = maxSpeed;
        target.thrustVectoringMaxAirspeed = maxVectorSpeed;
    }

    private void setCSPropParam(ConstantSpeedProp target, float maxPower, float maxTorque, float maxRPM)
    {
        Type propType = target.GetType();
        FieldInfo nominalPowerField = propType.GetField("nominalPower", BindingFlags.NonPublic | BindingFlags.Instance);
        if (nominalPowerField != null) nominalPowerField.SetValue(target, maxPower);
        FieldInfo propTorqueLimitField = propType.GetField("propTorqueLimit", BindingFlags.NonPublic | BindingFlags.Instance);
        if (propTorqueLimitField != null) propTorqueLimitField.SetValue(target, maxTorque);
        FieldInfo bladeDragField = propType.GetField("bladeDrag", BindingFlags.NonPublic | BindingFlags.Instance);
        if (bladeDragField != null) bladeDragField.SetValue(target, 0f);
        FieldInfo bladeStrengthField = propType.GetField("bladeStrength", BindingFlags.NonPublic | BindingFlags.Instance);
        if (bladeStrengthField != null) bladeStrengthField.SetValue(target, 200000);
        FieldInfo bladeEfficiencyField = propType.GetField("bladeEfficiency", BindingFlags.NonPublic | BindingFlags.Instance);
        if (bladeEfficiencyField != null) bladeEfficiencyField.SetValue(target, 1f);
        FieldInfo rpmLimitField = propType.GetField("rpmLimit", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rpmLimitField != null) rpmLimitField.SetValue(target, maxRPM);
    }
    private void setPropFanParam(PropFan target, float maxPower)
    {
        Type propType = target.GetType();
        FieldInfo nominalPowerField = propType.GetField("nominalPower", BindingFlags.NonPublic | BindingFlags.Instance);
        if (nominalPowerField != null) nominalPowerField.SetValue(target, maxPower);
    }
}

