using System.IO;
using System.Linq;
using SquirtingElephant.Helpers;
using UnityEngine;
using Verse;

namespace SquirtingElephant.PlanetaryDrill
{
    public class SettingsRenderer
    {
        public static void CreateRegularSettings(Listing_Standard ls, SettingsData settings)
        {
            Utils.MakeTextFieldNumericLabeled(ls, "SEPD_DrillSteelCost", ref settings.DrillSteelCost, 1, 50000);
            Utils.MakeTextFieldNumericLabeled(ls, "SEPD_DrillResearchCost", ref settings.DrillResearchCost, 1, 100000);
            Utils.MakeTextFieldNumericLabeled(ls, "SEPD_DrillPowerConsumption", ref settings.DrillPowerConsumption, 1,
                50000);

            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterDestroyOnDrop", ref settings.FilterDestroyOnDrop);
            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterMadeFromStuff", ref settings.FilterMadeFromStuff);
            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterRottable", ref settings.FilterRottable);
            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterScatterableOnMapGen", ref settings.FilterScatterableOnMapGen);
        }

        public static void CreateDrillableHeaders(TableData tableData)
        {
            Widgets.Label(tableData.GetFieldRect(1, 1), "SEPD_WorkAmount".Translate().CapitalizeFirst());
            Widgets.Label(tableData.GetFieldRect(2, 1), "SEPD_YieldAmount".Translate().CapitalizeFirst());
        }

        public static void CreateOpenConfigFolderButton(float buttonHeight)
        {
            var modconfigFolderPath = Utils.GetModSettingsFolderPath();
            if (Widgets.ButtonText(new Rect(0, 5, 200, buttonHeight),
                "SEPD_OpenConfigFolder".Translate().CapitalizeFirst(), active: Directory.Exists(modconfigFolderPath)))
            {
                Utils.OpenModSettingsFolder();
            }
        }

        public static void CreateRebootNote(Listing_Standard ls, float rowHeight)
        {
            var rowRect = ls.GetRect(rowHeight);
            Widgets.Label(rowRect, "SEPD_RebootMessage".Translate().CapitalizeFirst());
        }

        private static Rect GetSliderField(int colIdx, int rowIdx, TableData tableData, float numericInputWidth)
        {
            var field = tableData.GetFieldRect(colIdx, rowIdx);
            return new Rect(field.x + numericInputWidth + 2, field.y, field.width - numericInputWidth - 2,
                field.height);
        }

        public static void CreateDrillableSettingsFields(DrillData dd, int rowIdx, ref TableData tableData,
            float iconSize,
            float numericInputWidth, float workAmountMin, float workAmountMax, float yieldAmountMin)
        {
            // Icon
            Widgets.ThingIcon(tableData.GetFieldRect(0, rowIdx).Replace_Width(iconSize), dd.ThingDefToDrill);

            // ThingDef.label
            Widgets.Label(tableData.GetFieldRect(0, rowIdx).Add_X(iconSize + 2), dd.ThingDefToDrill.label);

            // Work Amount
            var bufferWorkAmount = dd.WorkAmount.ToString();
            Widgets.TextFieldNumeric(tableData.GetFieldRect(1, rowIdx).Replace_Width(numericInputWidth),
                ref dd.WorkAmount, ref bufferWorkAmount, workAmountMin, workAmountMax);
            var count = (int)Widgets.HorizontalSlider(GetSliderField(1, rowIdx, tableData, numericInputWidth),
                dd.WorkAmount, workAmountMin, workAmountMax);
            dd.WorkAmount = count;

            // Yield Amount
            var bufferYieldAmount = dd.YieldAmount.ToString();
            Widgets.TextFieldNumeric(tableData.GetFieldRect(2, rowIdx).Replace_Width(numericInputWidth),
                ref dd.YieldAmount, ref bufferYieldAmount, yieldAmountMin, dd.MaxYieldAmount);
            var countYield = (int)Widgets.HorizontalSlider(GetSliderField(2, rowIdx, tableData, numericInputWidth),
                dd.YieldAmount, yieldAmountMin, dd.MaxYieldAmount);
            dd.YieldAmount = countYield;

            // Row Mouse Hover
            tableData.ApplyMouseOverEntireRow(rowIdx);
            TooltipHandler.TipRegion(tableData.GetRowRect(rowIdx).LeftHalf(), dd.ThingDefToDrill.description);
        }

        /// <summary>
        ///     Creates the drillable buttons and returns the bottom location of those buttons.
        /// </summary>
        public static float CreateDrillableButtons(Listing_Standard ls, float buttonHeight, ref TableData tableData,
            SettingsData settings)
        {
            var buttonRow = ls.GetRect(buttonHeight);

            var addDrillableBtnRect = new Rect(buttonRow.x, buttonRow.y, buttonRow.width / 3, buttonRow.height);
            var removeDrillableBtnRect = new Rect(addDrillableBtnRect.xMax, addDrillableBtnRect.y,
                addDrillableBtnRect.width, addDrillableBtnRect.height);
            var resetDrillableBtnRect = new Rect(removeDrillableBtnRect.xMax, addDrillableBtnRect.y,
                addDrillableBtnRect.width, addDrillableBtnRect.height);

            CreateAndAddDrillableButton(addDrillableBtnRect, settings);
            CreateRemoveDrillableButton(removeDrillableBtnRect, ref tableData, settings);
            CreateResetDrillableButton(resetDrillableBtnRect, ref tableData, settings);

            return addDrillableBtnRect.yMax;
        }

        private static void CreateAndAddDrillableButton(Rect inRect, SettingsData settings)
        {
            if (!Widgets.ButtonText(inRect, "SEPD_AddDrillable".Translate().CapitalizeFirst(),
                active: Mineables.AllMineables.Except(settings.Drillables.Values.Select(d => d.ThingDefToDrill)).Any()))
            {
                return;
            }

            {
                Find.WindowStack.Add(new FloatMenu(
                    Mineables.AllMineables
                        .Where(settings.CanItemBeDrilledAccordingToSettings)
                        .Except(settings.Drillables.Values.Select(d =>
                            d.ThingDefToDrill)) // Don't display items that were already added.
                        .Select(m => new FloatMenuOption(m.label,
                            () => settings.Drillables.Add(m.defName, new DrillData(m.defName, 1000)), m))
                        .ToList()));
            }
        }

        private static void CreateRemoveDrillableButton(Rect inRect, ref TableData tableData, SettingsData settings)
        {
            if (!Widgets.ButtonText(inRect, "SEPD_RemoveDrillable".Translate().CapitalizeFirst(),
                active: settings.Drillables.Any()))
            {
                return;
            }

            Find.WindowStack.Add(new FloatMenu(
                settings.Drillables
                    .Select(kvp => new FloatMenuOption(kvp.Value.ThingDefToDrill.label,
                        () => settings.Drillables.Remove(kvp.Key), kvp.Value.ThingDefToDrill))
                    .ToList()));
            tableData.Rows.Remove(tableData.Rows.Last());
        }

        private static void CreateResetDrillableButton(Rect inRect, ref TableData tableData, SettingsData settings)
        {
            if (!Widgets.ButtonText(inRect, "SEPD_ResetDrillables".Translate().CapitalizeFirst()))
            {
                return;
            }

            while (tableData.Rows.Count > 1)
            {
                tableData.Rows.Remove(tableData.Rows.Last());
            }

            settings.ResetDrillableSettings();
        }
    }
}