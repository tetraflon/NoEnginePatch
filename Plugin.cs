using BepInEx;
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
        "Fighter1",
        "engine_R",
        "engine_L",
        "engine",
        "propHub",
        "hub_FL",
        "hub_FR",
        "hub_RL",
        "hub_RR",
        "engine_FL",
        "engine_FR",
        "engine_RL",
        "engine_RR",
        "engine_L",
        "engine_R",
        "hub_L",
        "hub_R",
        "hstab_R",
        "hstab_L",
        "rotorhub",
        "hub_L",
        "hub_U",
        "tailrotor",
        "COIN"
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
        done = true;
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
                setEngineParam(turbojet, 200000, 1020, 1020);
                Logger.LogInfo($"Modified engine of FS-12.");
            }
            else if (prefab.name == "engine") //FS20的引擎
            {
                Logger.LogInfo($"Engine of FS-20 found.");
                setEngineParam(turbojet, 200000, 1020, 1020);
                Logger.LogInfo($"Modified engine of FS-20.");

            }
            else if (prefab.name == "engine_L" || prefab.name == "engine_R")
            {
                //使用引擎最大推力辅助判断这是谁的引擎
                if (turbojet.maxThrust == 100000) //KR引擎
                {
                    Logger.LogInfo($"Engine of KR-67 found.");
                    setEngineParam(turbojet, 200000, 1020, 1020);
                    Logger.LogInfo($"Modified engine of KR-67.");
                }
                else if (turbojet.maxThrust == 28000) //TA30引擎
                {
                    Logger.LogInfo($"Engine of T/A-30 found.");
                    setEngineParam(turbojet, 100000, 260, 260);
                    Logger.LogInfo($"Modified engine of T/A-30.");
                }
                else if (turbojet.maxThrust == 210000) //SFB81引擎
                {
                    Logger.LogInfo($"Engine of SFB-81 found.");
                    setEngineParam(turbojet, 630000, 1020, 1020);
                    Logger.LogInfo($"Modified engine of SFB-81.");
                }
                else if (turbojet.maxThrust == 65000) //EW25引擎
                {
                    Logger.LogInfo($"Engine of EW-25 found.");
                    setEngineParam(turbojet, 260000, 1020, 1020);
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
                setCSPropParam(csprop, 15000000, 1200000, 3000);
                Logger.LogInfo($"Modified engine of CI-22.");
            }
            if (prefab.name == "hub_FL" || prefab.name == "hub_FR" || prefab.name == "hub_RL" || prefab.name == "hub_RR") //VL-49的引擎
            {
                Logger.LogInfo($"Engine of VL-49 found.");
                setCSPropParam(csprop, 94500000, 8000000, 700);
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
                turbine.maxPower = 15000000f;
                Logger.LogInfo($"Enchanted engine of CI-22.");
            }
            if (prefab.name == "engine_FL" || prefab.name == "engine_FR" || prefab.name == "engine_RL" || prefab.name == "engine_RR")
            {
                turbine.maxPower = 100000000f;
                Logger.LogInfo($"Enchanted engine of VL-49.");
            }

            if (prefab.name == "engine_L" || prefab.name == "engine_R")
            {
                turbine.maxPower = 34000000f;
                Logger.LogInfo($"Enchanted engine of NOTA10.");
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
        Type jetType = target.GetType();
        FieldInfo maxspeedField = jetType.GetField("maxSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        if (maxspeedField != null) maxspeedField.SetValue(target, maxSpeed);
        FieldInfo maxvectorspeedField = jetType.GetField("thrustVectoringMaxAirspeed", BindingFlags.NonPublic | BindingFlags.Instance);
        if (maxvectorspeedField != null) maxvectorspeedField.SetValue(target, maxVectorSpeed);
    }

    private void setCSPropParam(ConstantSpeedProp target, float maxPower, float maxTorque, float maxRPM)
    {
        Type propType = target.GetType();
        FieldInfo nominalPowerField = propType.GetField("nominalPower", BindingFlags.NonPublic | BindingFlags.Instance);
        if (nominalPowerField != null) nominalPowerField.SetValue(target, maxPower);
        FieldInfo propTorqueLimitField = propType.GetField("propTorqueLimit", BindingFlags.NonPublic | BindingFlags.Instance);
        if (propTorqueLimitField != null) propTorqueLimitField.SetValue(target, maxTorque);
        //FieldInfo bladeDragField = propType.GetField("bladeDrag", BindingFlags.NonPublic | BindingFlags.Instance);
        //if (bladeDragField != null) bladeDragField.SetValue(target, 0f);
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

