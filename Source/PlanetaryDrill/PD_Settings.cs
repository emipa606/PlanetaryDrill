using System.Linq;
using System.Reflection;
using Mlie;
using RimWorld;
using SquirtingElephant.Helpers;
using UnityEngine;
using Verse;

namespace SquirtingElephant.PlanetaryDrill;

public class PD_Settings : Mod
{
    private const float NUMERIC_INPUT_WIDTH = 100f;
    private const float BUTTON_HEIGHT = 22f;
    private const int WORK_AMOUNT_MIN = 1;
    private const int WORK_AMOUNT_MAX = 100000;
    private const int YIELD_AMOUNT_MIN = 1;
    private const float ROW_HEIGHT = 32f;
    private const float ROW_PADDING = 10f;
    private const float ICON_SIZE = ROW_HEIGHT;
    private const float SCROLL_VIEW_PADDING_HORIZONTAL = 10f;
    public static SettingsData Settings;

    public static TableData Table = new TableData(
        new Vector2(10f, 0f), new Vector2(10f, 10f),
        new[] { 225f, 200f, 200f },
        new[] { ROW_HEIGHT });

    private static string currentVersion;

    private Vector2 _scrollPosition = Vector2.zero;

    public PD_Settings(ModContentPack content) : base(content)
    {
        Settings = GetSettings<SettingsData>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.PlanetaryDrill"));
    }

    private float GetScrollViewHeight()
    {
        return (Settings.Drillables.Values.Count + 2) * (ROW_HEIGHT + ROW_PADDING);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var ls = new Listing_Standard();
        ls.Begin(inRect);
        if (currentVersion != null)
        {
            ls.Gap();
            ls.Gap();
            ls.Gap();
            GUI.contentColor = Color.gray;
            ls.Label("SEPD_ModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        SettingsRenderer.CreateRegularSettings(ls, Settings);
        SettingsRenderer.CreateOpenConfigFolderButton(BUTTON_HEIGHT);
        ls.GapLine();
        var drillableScrollStartY =
            SettingsRenderer.CreateDrillableButtons(ls, BUTTON_HEIGHT, ref Table, Settings) + 12f + ROW_HEIGHT;
        SettingsRenderer.CreateRebootNote(ls, ROW_HEIGHT);

        var scrollViewRect = new Rect(
            inRect.x + SCROLL_VIEW_PADDING_HORIZONTAL,
            inRect.y,
            inRect.width - (2 * SCROLL_VIEW_PADDING_HORIZONTAL),
            GetScrollViewHeight());
        Widgets.BeginScrollView(
            new Rect(0, drillableScrollStartY, inRect.width, inRect.height - drillableScrollStartY - ROW_HEIGHT),
            ref _scrollPosition, scrollViewRect);
        SettingsRenderer.CreateDrillableHeaders(Table);

        var rowIdx = 2;
        foreach (var kvp in Settings.Drillables)
        {
            var dd = kvp.Value;
            if (dd == null)
            {
                Settings.Drillables.Remove(kvp.Key);
                Debug.LogWarning(
                    $"Planetary Drill: Removed drillable \"{kvp.Key}\" because it no longer exists (did you remove the mod or did it update?).");
            }
            else
            {
                SettingsRenderer.CreateDrillableSettingsFields(dd, rowIdx++, ref Table, ICON_SIZE,
                    NUMERIC_INPUT_WIDTH, WORK_AMOUNT_MIN, WORK_AMOUNT_MAX, YIELD_AMOUNT_MIN);
            }
        }

        ls.GetRect(Table.TableRect.height);

        Widgets.EndScrollView();

        ls.End();
        ApplySettingsToDefs();

        base.DoSettingsWindowContents(inRect);
    }

    public static void ApplySettingsToDefs()
    {
        Utils.SetResearchBaseCost("PlanetaryDrilling", Settings.DrillResearchCost);

        var planetaryDrillDef = Utils.GetDefByDefName<ThingDef>("SE_PlanetaryDrill");
        if (planetaryDrillDef == null)
        {
            return;
        }

        Utils.SetThingSteelCost("SE_PlanetaryDrill", Settings.DrillSteelCost);
        var dds = Settings.Drillables.Values.Select(dd => dd.CreateDrillRecipe());
        planetaryDrillDef.recipes = dds.ToList();

        var fieldInfo =
            typeof(CompProperties_Power).GetField("basePowerConsumption",
                BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo?.SetValue(planetaryDrillDef.comps.OfType<CompProperties_Power>().First(),
            Settings.DrillPowerConsumption);
    }

    public static void RemoveInvalidSettings()
    {
        var invalidDrillableKeyPairs = Settings.Drillables
            .Where(keyValuePair => keyValuePair.Value == null || !keyValuePair.Value.ThingDefExists())
            .ToList();

        foreach (var invalidKvp in invalidDrillableKeyPairs)
        {
            Settings.Drillables.Remove(invalidKvp.Key);
            Debug.LogWarning(
                $"Planetary Drill: Removed drillable \"{invalidKvp.Key}\" because it no longer exists (did you remove the mod or did it update?).");
        }
    }

    public override string SettingsCategory()
    {
        return "SEPD_SettingsCategory".Translate();
    }
}