using System.Collections.Generic;
using RimWorld;
using SquirtingElephant.Helpers;
using Verse;

namespace SquirtingElephant.PlanetaryDrill;

public class DrillData(string thingDefToDrillName, int workAmount = 1, int yieldAmount = 1)
    : IExposable
{
    private const int MAX_ITEM_SPAWN_COUNT = (3 /* pi */ * 12 * 12 /* radius² */ * 2) - 16;

    private int _MaxYieldAmount;

    private ThingDef _ThingDefToDrill;

    /// <summary>
    ///     For the love of god, never change this value after _ThingDefToDrill has been set.
    /// </summary>
    private string ThingDefToDrillName = thingDefToDrillName;

    public int WorkAmount = workAmount;

    /// <summary>
    ///     How many ores are dug up each time.
    /// </summary>
    public int YieldAmount = yieldAmount;

    public ThingDef ThingDefToDrill
    {
        get
        {
            _ThingDefToDrill ??= Utils.GetDefByDefName<ThingDef>(ThingDefToDrillName);

            return _ThingDefToDrill;
        }
    }

    private string RecipeDefName => $"SEPD_{ThingDefToDrillName}_DrillRecipe";

    public int MaxYieldAmount
    {
        get
        {
            if (_MaxYieldAmount == 0 && _ThingDefToDrill != null)
            {
                _MaxYieldAmount = MAX_ITEM_SPAWN_COUNT * _ThingDefToDrill.stackLimit;
            }

            return _MaxYieldAmount;
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref ThingDefToDrillName, $"{Constants.SettingsPrefix}{ThingDefToDrillName}_DefName",
            ThingDefToDrillName, true);
        Scribe_Values.Look(ref WorkAmount, $"{Constants.SettingsPrefix}{ThingDefToDrillName}_WorkAmount",
            WorkAmount, true);
        Scribe_Values.Look(ref YieldAmount, $"{Constants.SettingsPrefix}{ThingDefToDrillName}_DrillAmount",
            YieldAmount, true);
    }

    public RecipeDef CreateDrillRecipe()
    {
        var recipeDefName = RecipeDefName;
        var drillRecipe = DefDatabase<RecipeDef>.GetNamed(recipeDefName, false);
        if (drillRecipe == null)
        {
            drillRecipe = createDrillRecipeDef();
            DefDatabase<RecipeDef>.Add(drillRecipe);
        }
        else
        {
            drillRecipe.workAmount = WorkAmount;
            drillRecipe.products = createProducts();
        }

        return drillRecipe;
    }

    public bool ThingDefExists()
    {
        return Utils.DefExistsByDefName<ThingDef>(ThingDefToDrillName);
    }

    private List<ThingDefCountClass> createProducts()
    {
        return [new ThingDefCountClass(ThingDefToDrill, YieldAmount)];
    }

    private RecipeDef createDrillRecipeDef()
    {
        var rd = new RecipeDef
        {
            defName = RecipeDefName,
            defaultIngredientFilter = new ThingFilter(),
            effectWorking = DefDatabase<EffecterDef>.GetNamed("Smith"),
            workSkill = DefDatabase<SkillDef>.GetNamed("Mining"),
            workSpeedStat = DefDatabase<StatDef>.GetNamed("MiningSpeed"),
            workSkillLearnFactor = 0.5f,
            jobString = "SEPD_DrillingInfo".Translate(ThingDefToDrill.label),
            workAmount = WorkAmount,
            ingredients = [],
            soundWorking = DefDatabase<SoundDef>.GetNamed("Recipe_Machining"),
            label = "SEPD_DrillInfo".Translate(ThingDefToDrill.label),
            description = "SEPD_DrillInfo".Translate(ThingDefToDrill.label),
            products = createProducts()
        };

        return rd;
    }
}