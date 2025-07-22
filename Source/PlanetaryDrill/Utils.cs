// Copyright 2019 Squirting Elephant.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace SquirtingElephant.Helpers;

public static class Utils
{
    public enum ESide
    {
        LeftHalf,
        RightHalf,
        BothSides
    }

    private const string DEF_NOT_FOUND_FORMAT =
        "Unable to find {0}: {1}. Please ensure that this def exists in the database and that the database was loaded before trying to locate this.";

    private const string SINGLE_WHITE_SPACE = " ";

    private static void LogDefNotFoundError(string defName, string defType = "Def")
    {
        Log.Error(string.Format(DEF_NOT_FOUND_FORMAT, defType, defName));
    }

    public static T GetDefByDefName<T>(string defName, bool errorOnNotFound = true) where T : Def
    {
        var def = DefDatabase<T>.GetNamed(defName, errorOnNotFound);
        if (def != null)
        {
            return def;
        }

        if (errorOnNotFound)
        {
            LogDefNotFoundError(defName, typeof(T).Name);
        }

        return null;
    }

    public static bool DefExistsByDefName<T>(string defName) where T : Def
    {
        return DefDatabase<T>.GetNamed(defName, false) != null;
    }

    /// <summary>
    ///     Note that this method assumes that windows is installed on the C-drive.
    /// </summary>
    public static string GetModSettingsFolderPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return
                $@"C:\Users\{Environment.UserName}\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config";
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? "~/Library/Application Support/RimWorld/Config"
            : "~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config"; // Unix
    }

    public static void OpenModSettingsFolder()
    {
        var path = GetModSettingsFolderPath();
        if (Directory.Exists(path))
        {
            Process.Start(path);
        }
        else
        {
            Log.Error($"Unable to open path: {path}. This error is not problematic and doesn't hurt your game.");
        }
    }

    private static Rect GetRectFor(Listing_Standard ls, ESide side, float rowHeight)
    {
        switch (side)
        {
            case ESide.LeftHalf:
                return ls.GetRect(rowHeight).LeftHalf();
            case ESide.RightHalf:
                return ls.GetRect(rowHeight).RightHalf();
            case ESide.BothSides:
                return ls.GetRect(rowHeight);
            default:
                throw new ArgumentException("Unexpected value", nameof(side));
        }
    }

    public static void MakeCheckboxLabeled(Listing_Standard ls, string translationKey, ref bool checkedSetting,
        ESide side = ESide.RightHalf, float rowHeight = 32f)
    {
        var boxRect = GetRectFor(ls, side, rowHeight);
        Widgets.CheckboxLabeled(boxRect, translationKey.Translate().CapitalizeFirst() + SINGLE_WHITE_SPACE,
            ref checkedSetting);
    }

    public static void MakeTextFieldNumericLabeled<T>(Listing_Standard ls, string translationKey, ref T setting,
        float min = 1, float max = 1000, ESide side = ESide.RightHalf, float rowHeight = 32f) where T : struct
    {
        var boxRect = GetRectFor(ls, side, rowHeight);
        var buffer = setting.ToString();
        Widgets.TextFieldNumericLabeled(boxRect, translationKey.Translate().CapitalizeFirst() + SINGLE_WHITE_SPACE,
            ref setting, ref buffer, min, max);
    }

    public static void SetResearchBaseCost(string researchDefName, int newResearchCost)
    {
        var rpd = GetDefByDefName<ResearchProjectDef>(researchDefName);
        if (rpd != null)
        {
            rpd.baseCost = newResearchCost;
        }
    }

    public static void SetThingSteelCost(string thingDefName, int newSteelCost)
    {
        var def = GetDefByDefName<ThingDef>(thingDefName);

        var costDef = def?.costList.FirstOrDefault(c => c.thingDef == ThingDefOf.Steel);
        if (costDef != null)
        {
            costDef.count = newSteelCost;
        }
    }
}