using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ColorTweaks.Configuration;
using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Attributes;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ColorTweaks.Affinity_Patches;

[Bind(Location.Menu)]
internal class HSVMagic : IAffinity, IInitializable, IDisposable
{
    private SiraLog _siraLog;
    private static PluginConfig? _config;

    public HSVMagic(SiraLog siraLog, PluginConfig config)
    {
        _siraLog = siraLog;
        _config = config;
    }
    
    public void Initialize()
    {
        
    }

    public void Dispose()
    {
        
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(ColorSaturationValueSlider), "UpdateVisuals")]
    internal void Postfix_UpdateVisuals(Graphic[] ____graphics, float ____hue)
    {
        if (_config == null) return;
        foreach (var graphic in ____graphics)
        {
            graphic.color = Color.HSVToRGB(
                ____hue,
                _config.SaturationMax,
                _config.ValueMax
            );
        }
    }
    
    [AffinityPostfix]
    [AffinityPatch(typeof(HSVPanelController), nameof(HSVPanelController.RefreshSlidersValues))]
    internal void Postfix_RefreshSlidersValues(Vector3 ____hsvColor,
        ColorSaturationValueSlider ____colorSaturationValueSlider)
    {
        if (_config == null) return;
        ____colorSaturationValueSlider.normalizedValue = new Vector2(
            ____hsvColor.y.Remap(_config.SaturationMin, _config.SaturationMax, 0f, 1f),
            ____hsvColor.z.Remap(_config.ValueMin, _config.ValueMax, 0f, 1f)
        );
    }
    
    [AffinityTranspiler]
    [AffinityPatch(typeof(HSVPanelController), nameof(HSVPanelController.HandleColorSaturationOrValueDidChange))]
    internal IEnumerable<CodeInstruction> Transpiler_HandleHSVDidChange(IEnumerable<CodeInstruction> instructions)
    {
        return InjectRemapper(instructions);
    }
    
    [AffinityTranspiler]
    [AffinityPatch(typeof(HSVPanelController), nameof(HSVPanelController.HandleColorHueDidChange))]
    internal IEnumerable<CodeInstruction> Transpiler_HandleHueDidChange(IEnumerable<CodeInstruction> instructions)
    {
        return InjectRemapper(instructions);
    }

    internal IEnumerable<CodeInstruction> InjectRemapper(IEnumerable<CodeInstruction> instructions)
    {
        var res = instructions.ToList();
        var remap = AccessTools.Method(typeof(HSVMagic), nameof(Remap));
        var hsvColor = AccessTools.Field(typeof(HSVPanelController), "_hsvColor");
        var saturation = AccessTools.Field(typeof(Vector3), "y");
        var value = AccessTools.Field(typeof(Vector3), "z");
        
        for (var i = 0; i < res.Count - 1; i++)
        {
            var ci = res[i];
            var next = res[i + 1];

            if (ci.opcode == OpCodes.Ldflda && ci.operand == (object)hsvColor && next.opcode == OpCodes.Ldfld)
            {
                if (next.operand == (object)saturation)
                {
                    res.InsertRange(i + 2, new []
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Call, remap)
                    });
                } else if (next.operand == (object)value)
                {
                    res.InsertRange(i + 2, new []
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Call, remap)
                    });
                }
            }
        }
        
        return res;
    }

    internal static float Remap(float f, int coord)
    {
        if (_config == null) return f;
        
        return coord switch
        {
            0 => f.Remap(0f, 1f, _config.SaturationMin, _config.SaturationMax),
            1 => f.Remap(0f, 1f, _config.ValueMin, _config.ValueMax),
            _ => f
        };
    }
}