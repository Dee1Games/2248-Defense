using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private static VibrationManager _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public void DoLightVibration()
    {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }

    public void DoMediumVibration() {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
    }


    public void DoHeavyVibration() {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
    }
}
