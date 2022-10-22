using System.Collections.Generic;
using RimWorld;
using SquirtingElephant.Helpers;
using Verse;

namespace SquirtingElephant.PlanetaryDrill;

public class DrillData : IExposable
{
    private const int MAX_ITEM_SPAWN_COUNT = (3 /* pi */ * 12 * 12 /* radius² */ * 2) - 16;

    private int _MaxYieldAmount;

    private ThingDef _ThingDefToDrill;

    /// <summary>
    ///     For the love of god, never change this value after _ThingDefToDrill has been set.
    /// </summary>
    private string ThingDefToDrillName;

    public int WorkAmount;

    /// <summary>
    ///     How many ores are dug up each time.
    /// </summary>
    public int YieldAmount;

    public DrillData(string thingDefToDrillName, int workAmount = 1, int yieldAmount = 1)
    {
        ThingDefToDrillName = thingDefToDrillName;
        WorkAmount = workAmount;
        YieldAmount = yieldAmount;
    }

    public ThingDef ThingDefToDrill
    {
        get
        {
            if (_ThingDefToDrill == null)
            {
                _ThingDefToDrill = Utils.GetDefByDefName<ThingDef>(ThingDefToDrillName);
            }

            return _ThingDefToDrill;
        }
        set
        {
            _ThingDefToDrill = value;
            ThingDefToDrillName = value?.defName;
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
        Scribe_Values.Look(ref ThingDefToDrillName, $"{Constants.SETTINGS_PREFIX}{ThingDefToDrillName}_DefName",
            ThingDefToDrillName, true);
        Scribe_Values.Look(ref WorkAmount, $"{Constants.SETTINGS_PREFIX}{ThingDefToDrillName}_WorkAmount",
            WorkAmount, true);
        Scribe_Values.Look(ref YieldAmount, $"{Constants.SETTINGS_PREFIX}{ThingDefToDrillName}_DrillAmount",
            YieldAmount, true);
    }

    public RecipeDef CreateDrillRecipe()
    {
        var recipeDefName = RecipeDefName;
        var drillRecipe = DefDatabase<RecipeDef>.GetNamed(recipeDefName, false);
        if (drillRecipe == null)
        {
            drillRecipe = CreateDrillRecipeDef();
            DefDatabase<RecipeDef>.Add(drillRecipe);
        }
        else
        {
            drillRecipe.workAmount = WorkAmount;
            drillRecipe.products = CreateProducts();
        }

        return drillRecipe;
    }

    public bool ThingDefExists()
    {
        return Utils.DefExistsByDefName<ThingDef>(ThingDefToDrillName);
    }

    private List<ThingDefCountClass> CreateProducts()
    {
        return new List<ThingDefCountClass> { new ThingDefCountClass(ThingDefToDrill, YieldAmount) };
    }

    private RecipeDef CreateDrillRecipeDef()
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
            ingredients = new List<IngredientCount>(),
            soundWorking = DefDatabase<SoundDef>.GetNamed("Recipe_Machining"),
            label = "SEPD_DrillInfo".Translate(ThingDefToDrill.label),
            description = "SEPD_DrillInfo".Translate(ThingDefToDrill.label),
            products = CreateProducts()
        };

        return rd;
    }
}