﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using SquirtingElephant.Helpers;
using Verse;

namespace SquirtingElephant.PlanetaryDrill;

public class SettingsData : ModSettings
{
    /// <summary>
    ///     Contains everything that the drill is allowed to drill.
    /// </summary>
    public readonly Dictionary<string, DrillData> Drillables = new Dictionary<string, DrillData>();

    public int DrillPowerConsumption = 5000;
    public int DrillResearchCost = 17500;
    public int DrillSteelCost = 3250;

    /// <summary>
    ///     Filters
    /// </summary>
    public bool FilterDestroyOnDrop = true;

    public bool FilterMadeFromStuff = true;
    public bool FilterRottable = true;
    public bool FilterScatterableOnMapGen = true;
    public bool IsFirstRun = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref IsFirstRun, "SEPD_IsFirstRun", true);

        Scribe_Values.Look(ref DrillSteelCost, "SEPD_DrillSteelCost", 3250);
        Scribe_Values.Look(ref DrillResearchCost, "SEPD_DrillResearchCost", 17500);
        Scribe_Values.Look(ref DrillPowerConsumption, "SEPD_DrillPowerConsumption", 5000);

        Scribe_Values.Look(ref FilterDestroyOnDrop, "SEPD_FilterDestroyOnDrop", true);
        Scribe_Values.Look(ref FilterMadeFromStuff, "SEPD_FilterMadeFromStuff", true);
        Scribe_Values.Look(ref FilterScatterableOnMapGen, "SEPD_FilterScatterableOnMapGen", true);
        Scribe_Values.Look(ref FilterRottable, "SEPD_FilterRottable", true);

        var drillableItems = Drillables.Keys.ToList();
        Scribe_Collections.Look(ref drillableItems, "SEPD_Drillables");

        drillableItems.ToList()
            .ForEach(td =>
            {
                if (td != null)
                {
                    if (!Drillables.ContainsKey(td))
                    {
                        Drillables.Add(td, new DrillData(td));
                    }
                }
                else
                {
                    Log.Warning("SEPD_OreMissingWarning".Translate().CapitalizeFirst());
                }
            });
        Drillables.Values.ToList().ForEach(d => d.ExposeData());
    }

    public void ResetDrillableSettings()
    {
        Drillables.Clear();

        AddDefaultDrillable("ChunkGranite", 250, 1);
        AddDefaultDrillable("Steel", 5750, 35);
        AddDefaultDrillable("Plasteel", 11500, 35);
        AddDefaultDrillable("Jade", 6500, 20);
        AddDefaultDrillable("Uranium", 7000, 20);
        AddDefaultDrillable("Gold", 7250, 15);

        PD_Settings.ApplySettingsToDefs();
    }

    private void AddDefaultDrillable(string thingDefName, int workAmount, int yieldAmount)
    {
        if (Utils.GetDefByDefName<ThingDef>(thingDefName, false) != null)
        {
            Drillables.Add(thingDefName, new DrillData(thingDefName, workAmount, yieldAmount));
        }
        else
        {
            Log.Warning(
                $"Planetary Drill: Failed to retrieve {thingDefName}. This def wasn't added to the defaults.");
        }
    }

    public bool CanItemBeDrilledAccordingToSettings(ThingDef d)
    {
        return (d.scatterableOnMapGen || !FilterScatterableOnMapGen)
               && (!d.destroyOnDrop || !FilterDestroyOnDrop)
               && (!d.MadeFromStuff || !FilterMadeFromStuff)
               && (d.GetCompProperties<CompProperties_Rottable>() == null || !FilterRottable);
    }
}