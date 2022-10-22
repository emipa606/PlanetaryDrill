// Copyright 2019 Squirting Elephant.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace SquirtingElephant.Helpers;

public static class Extensions
{
    public static Rect Add(this Rect r1, Rect r2)
    {
        return new Rect(r1.x + r2.x, r1.y + r2.y, r1.width + r2.width, r1.height + r2.height);
    }

    public static Rect Subtract(this Rect r1, Rect r2)
    {
        return new Rect(r1.x - r2.x, r1.y - r2.y, r1.width - r2.width, r1.height - r2.height);
    }

    public static Rect Add_X(this Rect r, float x)
    {
        return new Rect(r.x + x, r.y, r.width, r.height);
    }

    public static Rect Add_Y(this Rect r, float y)
    {
        return new Rect(r.x, r.y + y, r.width, r.height);
    }

    public static Rect Add_Width(this Rect r, float width)
    {
        return new Rect(r.x, r.y, r.width + width, r.height);
    }

    public static Rect Add_Height(this Rect r, float height)
    {
        return new Rect(r.x, r.y, r.width, r.height + height);
    }

    public static Rect Replace_X(this Rect r, float x)
    {
        return new Rect(x, r.y, r.width, r.height);
    }

    public static Rect Replace_Y(this Rect r, float y)
    {
        return new Rect(r.x, y, r.width, r.height);
    }

    public static Rect Replace_Width(this Rect r, float width)
    {
        return new Rect(r.x, r.y, width, r.height);
    }

    public static Rect Replace_Height(this Rect r, float height)
    {
        return new Rect(r.x, r.y, r.width, height);
    }

    /// <summary>
    ///     Translates and capitalizes the first character.
    /// </summary>
    public static string TC(this string s)
    {
        return s.Translate().CapitalizeFirst();
    }
}

public abstract class TableEntity
{
    protected readonly TableData TableData;

    public string Name = string.Empty;

    public TableEntity(TableData tableData)
    {
        TableData = tableData;
    }

    /// <summary>
    ///     Please do not edit this outside of TableData. This value is calculated.
    /// </summary>
    public Rect Rect { get; set; }
}

public class TableColumn : TableEntity
{
    private float _Width;

    public TableColumn(TableData tableData, float width) : base(tableData)
    {
        Width = width;
    }

    public float Width
    {
        get => _Width;
        set
        {
            if (value == _Width)
            {
                return;
            }

            if (value > 0f)
            {
                _Width = value;
                TableData.Update();
            }
            else
            {
                Log.Error($"TableRow received a value of {value} for its Height.");
            }
        }
    }
}

public class TableRow : TableEntity
{
    private float _Height;

    public TableRow(TableData tableData, float height) : base(tableData)
    {
        Height = height;
        SetFields(tableData);
    }

    public List<TableField> Fields { get; set; } = new List<TableField>();

    public float Height
    {
        get => _Height;
        set
        {
            if (value == _Height)
            {
                return;
            }

            if (value > 0f)
            {
                _Height = value;
                TableData.Update();
            }
            else
            {
                Log.Error($"TableRow received a value of {value} for its Height.");
            }
        }
    }

    private void SetFields(TableData tableData)
    {
        Fields.Clear();
        tableData.Columns.ForEach(c => Fields.Add(new TableField(tableData, c, this)));
    }

    public void UpdateFields()
    {
        Fields.ForEach(f => f.Update());
    }
}

public class TableField : TableEntity
{
    public TableField(TableData tableData, TableColumn column, TableRow row) : base(tableData)
    {
        Column = column;
        Row = row;
        Update();
    }

    public TableColumn Column { get; }

    public TableRow Row { get; }

    public static TableField Invalid => new TableField(null, null, null);

    public void Update()
    {
        Rect = new Rect(Column.Rect.x, Row.Rect.y, Column.Rect.width, Row.Rect.height);
    }
}

public class TableData
{
    private const float DEFAULT_ROW_HEIGHT = 32f;

    private Vector2 _Spacing;

    private Vector2 _TableOffset;

    /// <summary>
    ///     Used privately within methods to temporarily disable the updating without changing the UpdateEnabled setting.
    ///     Please set this value back to true at the end of your method or whatever you are doing.
    /// </summary>
    private bool PrivateUpdateEnabled = true;

    public bool UpdateEnabled = true;

    /// <summary>
    /// </summary>
    /// <param name="tableOffset"></param>
    /// <param name="spacing"></param>
    /// <param name="colWidths"></param>
    /// <param name="rowHeights"></param>
    /// <param name="colCount"></param>
    /// <param name="rowCount">Note: A header-row also counts as 1 rowcount.</param>
    public TableData(Vector2 tableOffset, Vector2 spacing, float[] colWidths, float[] rowHeights, int colCount = -1,
        int rowCount = -1)
    {
        Initialize(tableOffset, spacing, colWidths, rowHeights, colCount, rowCount);
    }

    public float Bottom => TableRect.yMax;

    public Vector2 Spacing
    {
        get => _Spacing;
        set
        {
            if (value == _Spacing)
            {
                return;
            }

            _Spacing = value;
            Update();
        }
    }

    public Vector2 TableOffset
    {
        get => _TableOffset;
        set
        {
            if (value == _TableOffset)
            {
                return;
            }

            _TableOffset = value;
            Update();
        }
    }

    /// <summary>
    ///     Column Datas.
    /// </summary>
    public List<TableColumn> Columns { get; set; } = new List<TableColumn>();

    public List<TableRow> Rows { get; set; } = new List<TableRow>();

    public Rect TableRect { get; private set; } = Rect.zero;

    /// <summary>
    /// </summary>
    /// <param name="tableOffset"></param>
    /// <param name="tableSpacing"></param>
    /// <param name="colWidths"></param>
    /// <param name="rowHeights"></param>
    /// <param name="colCount">
    ///     If this value is greater than <c>colWidths</c> then the last <c>colWidths</c> will be used for
    ///     the extra columns.
    /// </param>
    /// <param name="rowCount">
    ///     If this value is greater than <c>rowHeights</c> then the last <c>rowHeights</c> will be used for
    ///     the extra rows.
    /// </param>
    private void Initialize(Vector2 tableOffset, Vector2 tableSpacing, float[] colWidths, float[] rowHeights,
        int colCount = -1, int rowCount = -1)
    {
        // For performance reasons disable the updating and update after the table initialization instead.
        PrivateUpdateEnabled = false;

        _TableOffset = tableOffset;
        _Spacing = tableSpacing;

        // Add Columns.
        foreach (var colWidth in colWidths)
        {
            AddColumns(colWidth);
        }

        AddColumns(colWidths.Last(), colCount - colWidths.Length);

        // Add Rows.
        foreach (var rowHeight in rowHeights)
        {
            AddRow(rowHeight);
        }

        AddRow(GetLastRowHeight(), rowCount - Rows.Count);

        PrivateUpdateEnabled = true;
        Update();
    }

    public void Update(bool force = false)
    {
        if ((!UpdateEnabled || !PrivateUpdateEnabled) && !force)
        {
            return;
        }

        SetTableRect();
        UpdateColumns();
        UpdateRowsAndFields();
    }

    private float CalcTableWidth()
    {
        if (Columns.Count == 0)
        {
            return 0f;
        }

        var result = Columns[0].Width;
        for (var i = 1; i < Columns.Count; i++)
        {
            result += Columns[i].Width + Spacing.x;
        }

        return result;
    }

    private float CalcTableHeight()
    {
        if (Rows.Count == 0)
        {
            return 0f;
        }

        var result = Rows[0].Height;
        for (var i = 1; i < Rows.Count; i++)
        {
            result += Rows[i].Height + Spacing.y;
        }

        return result;
    }

    private void SetTableRect()
    {
        TableRect = new Rect(TableOffset.x, TableOffset.y, CalcTableWidth(), CalcTableHeight());
    }

    /// <summary>
    ///     Please also call UpdateRowsAndFields() after changing any column through here.
    /// </summary>
    public void UpdateColumns()
    {
        var nextColStart_X = TableRect.x;
        foreach (var col in Columns)
        {
            col.Rect = new Rect(nextColStart_X, TableRect.y, col.Width, TableRect.height);
            nextColStart_X = col.Rect.xMax + Spacing.x;
        }
    }

    public void UpdateRowsAndFields()
    {
        var nextRowStart_Y = TableRect.y;
        foreach (var row in Rows)
        {
            row.Rect = new Rect(TableRect.x, nextRowStart_Y, TableRect.width, row.Height);
            nextRowStart_Y = row.Rect.yMax + Spacing.y;
            row.UpdateFields();
        }
    }

    /// <summary>
    ///     Note: Will do nothing if <c>amount</c> is zero or less.
    /// </summary>
    public void AddRow(float rowHeight, int amount = 1)
    {
        if (amount == 0)
        {
            return;
        }

        PrivateUpdateEnabled = false;
        for (var i = 0; i < amount; i++)
        {
            Rows.Add(new TableRow(this, rowHeight));
        }

        PrivateUpdateEnabled = true;
        Update();
    }

    /// <summary>
    ///     Note: Will do nothing if <c>amount</c> is zero or less.
    /// </summary>
    private void AddColumns(float colWidth, float amount = 1)
    {
        if (amount == 0)
        {
            return;
        }

        PrivateUpdateEnabled = false;
        for (var i = 0; i < amount; i++)
        {
            Columns.Add(new TableColumn(this, colWidth));
        }

        PrivateUpdateEnabled = true;
        Update();
    }

    private void CreateRowsUntil(int rowIdx)
    {
        AddRow(GetLastRowHeight(), rowIdx + 1 - Rows.Count);
    }

    public TableField GetField(int colIdx, int rowIdx)
    {
        if (colIdx >= Columns.Count)
        {
            Log.Error($"Attemped to access a column that's out of bounds. Received: {colIdx}.");
            return TableField.Invalid;
        }

        CreateRowsUntil(rowIdx);
        return Rows[rowIdx].Fields[colIdx];
    }

    private float GetLastRowHeight()
    {
        return Rows.Count > 0 ? Rows.Last().Height : DEFAULT_ROW_HEIGHT;
    }

    public Rect GetRowRect(int rowIdx)
    {
        CreateRowsUntil(rowIdx);
        return Rows[rowIdx].Rect;
    }

    public Rect GetHeaderRect(int colIdx)
    {
        return GetField(colIdx, 0).Rect;
    }

    public Rect GetFieldRect(int colIdx, int rowIdx)
    {
        return GetField(colIdx, rowIdx).Rect;
    }

    /// <summary>
    ///     Will highlight the entire table-row if the mouse is over it. Call this somewhere in DoWindowContents() while a
    ///     Listing_Standard is active.
    /// </summary>
    /// <param name="rowIdx">The current row index to apply this to.</param>
    public void ApplyMouseOverEntireRow(int rowIdx)
    {
        var rowRect = GetRowRect(rowIdx);
        if (Mouse.IsOver(rowRect))
        {
            Widgets.DrawHighlight(rowRect);
        }
    }

#if DEBUG
        private static Texture2D TableRectTexure;
        private static Texture2D ColTexure;
        private static Texture2D RowTexure;
        private static Texture2D FieldTexture;

        /// <summary>
        ///     Notes:
        ///     1. Drawing the Rows bugs (they are not wide enough? Why?)
        ///     2. Sometimes Rimworld complains about more calls to BeginScrollView() than to EndScrollView(). Why? I have no idea
        ///     but it may happen when adding Widgets through a <T> method.
        /// </summary>
        public void DrawTableDebug()
        {
            if (TableRect.width == 0f || TableRect.height == 0f)
            {
                Log.Error("The TableRect its width and/or height are zero.");
                return;
            }

            // Initialize textures.
            var texWidth = (int)TableRect.width;
            var texHeight = (int)TableRect.height;
            Utils.SetupTextureAndColorIt(ref TableRectTexure, texWidth, texHeight, Color.black);
            Utils.SetupTextureAndColorIt(ref ColTexure, texWidth, texHeight, Color.blue);
            Utils.SetupTextureAndColorIt(ref RowTexure, texWidth, texHeight, Color.yellow);
            Utils.SetupTextureAndColorIt(ref FieldTexture, texWidth, texHeight, Color.red);

            // Draw TableRect.
            Widgets.Label(TableRect, new GUIContent(TableRectTexure));
            // Draw all columns.
            Columns.ForEach(c => Widgets.Label(c.Rect, new GUIContent(ColTexure)));
            foreach (var row in Rows)
            {
                // Draw the rows.
                Widgets.Label(row.Rect, new GUIContent(RowTexure));
                // Draw the fields of this row.
                row.Fields.ForEach(f => Widgets.Label(f.Rect, new GUIContent(FieldTexture)));
            }
        }

        public void LogDebugData()
        {
            var rowLocs = string.Empty;
            Rows.ForEach(r => rowLocs += r.Rect + "  ");
            Log.Message(
                $"Table Debug. Table Rect: {TableRect.ToString()}, colCnt: {Columns.Count.ToString()}, rowCnt: {Rows.Count.ToString()}, rowLocs: {rowLocs}");
        }
#endif
}

public static class SeMath
{
    public static int RoundToNearestMultiple(int value, float multiple)
    {
        return (int)(Math.Round(value / multiple) * multiple);
    }

    /// <summary>
    ///     Calculates the X-location of the column on <c>colIdx</c>.
    /// </summary>
    /// <param name="offset_X">X-offset for the first column.</param>
    /// <param name="colWidth">Column Width (each column has the same width).</param>
    /// <param name="spacing_X">Spacing between columns.</param>
    /// <param name="colIdx">Column index for the column that you need the x-location for.</param>
    /// <returns></returns>
    public static float CalcColumn_X(float offset_X, float colWidth, float spacing_X, int colIdx)
    {
        return offset_X + ((colWidth + spacing_X) * colIdx);
    }
}

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

    public static void LogDefNotFoundWarning(string defName, string defType = "Def")
    {
        Log.Warning(string.Format(DEF_NOT_FOUND_FORMAT, defType, defName));
    }

    public static void LogDefNotFoundError(string defName, string defType = "Def")
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

    public static void SetThingStat(string thingDefName, string statDefName, float newValue)
    {
        var def = GetDefByDefName<ThingDef>(thingDefName);
        if (def != null)
        {
            def.statBases.Find(s => s.stat.defName == statDefName).value = newValue;
        }
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

    public static void SetupTextureAndColorIt(ref Texture2D texture, int width, int height, Color color)
    {
        var paintTexture = false;
        if (texture == null)
        {
            texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            paintTexture = true;
        }
        else if (texture.width != width || texture.height != height)
        {
            if (width == 0 || height == 0)
            {
                Log.Error("Received either a 0 for the texture width and/or height.");
            }

            paintTexture = texture.width < width || texture.height < height;
            texture.width = width;
            texture.height = height;
        }

        if (!paintTexture)
        {
            return;
        }

        var textureArray = texture.GetPixels();
        for (var i = 0; i < textureArray.Length; ++i)
        {
            textureArray[i] = color;
        }

        texture.SetPixels(textureArray);
        texture.Apply();
    }

    public static IEnumerable<Building> GetBuildingsByDefName(string defName)
    {
        if (Current.Game == null || Current.Game.CurrentMap == null)
        {
            return Enumerable.Empty<Building>();
        }

        return Current.Game.CurrentMap.listerBuildings.allBuildingsColonist.Where(b => b.def.defName == defName);
    }

    public static void AddRecipeUnique(ThingDef thingDef, RecipeDef recipe)
    {
        if (thingDef.recipes.Any(r => r.defName == recipe.defName))
        {
            thingDef.recipes.Add(recipe);
        }
    }

    /// <summary>
    ///     Looks up recipes in one ThingDef and adds references to them to another ThingDef.recipes.
    /// </summary>
    public static void CopyRecipesFromAToB(string sourceDefName, string destinationDefName)
    {
        var source = GetDefByDefName<ThingDef>(sourceDefName);
        var destination = GetDefByDefName<ThingDef>(destinationDefName);

        foreach (var recipe in source.recipes)
        {
            AddRecipeUnique(destination, recipe);
        }
    }

    public static void AddRecipesToDef(string thingDefName, bool errorOnRecipeNotFound,
        params string[] recipeDefNames)
    {
        if (recipeDefNames.Length == 0)
        {
            return;
        }

        var td = GetDefByDefName<ThingDef>(thingDefName, false);
        if (td == null)
        {
            return;
        }

        foreach (var recipeDefName in recipeDefNames)
        {
            var recipe = GetDefByDefName<RecipeDef>(recipeDefName, errorOnRecipeNotFound);
            if (recipe != null)
            {
                AddRecipeUnique(td, recipe);
            }
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

    public static void EditPowerGenerationValue(string thingDefName, int newPowerGenerationAmount)
    {
        var thingDef = GetDefByDefName<ThingDef>(thingDefName);
        if (thingDef == null)
        {
            return;
        }

        var fieldInfo =
            typeof(CompProperties_Power).GetField("basePowerConsumption",
                BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo?.SetValue(thingDef.comps.OfType<CompProperties_Power>().First(),
            -Math.Abs(newPowerGenerationAmount));
    }

    public static void SetWorkAmount(string recipeDefName, int newWorkAmount)
    {
        var rd = GetDefByDefName<RecipeDef>(recipeDefName);
        if (rd != null)
        {
            rd.workAmount = newWorkAmount;
        }
    }

    public static void SetYieldAmount(string recipeDefName, int newYieldAmount)
    {
        var def = GetDefByDefName<RecipeDef>(recipeDefName);
        def?.products.ForEach(p => p.count = newYieldAmount);
    }

    public static void SetResearchBaseCost(string researchDefName, int newResearchCost)
    {
        var rpd = GetDefByDefName<ResearchProjectDef>(researchDefName);
        if (rpd != null)
        {
            rpd.baseCost = newResearchCost;
        }
    }

    public static void SetThingMaxHp(string thingDefName, int newHP)
    {
        SetThingStat(thingDefName, "MaxHitPoints", newHP);
    }

    public static void SetThingMaxBeauty(string thingDefName, int newBeauty)
    {
        SetThingStat(thingDefName, "Beauty", newBeauty);
    }

    public static void SetThingTurretBurstCooldown(string thingDefName, float newTurretBurstCooldown)
    {
        var def = GetDefByDefName<ThingDef>(thingDefName);
        if (def != null)
        {
            def.building.turretBurstCooldownTime = newTurretBurstCooldown;
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

    public static void SetThingComponentCost(string thingDefName, int newComponentCost)
    {
        var def = GetDefByDefName<ThingDef>(thingDefName);

        var costDef = def?.costList.FirstOrDefault(c => c.thingDef == ThingDefOf.ComponentIndustrial);
        if (costDef != null)
        {
            costDef.count = newComponentCost;
        }
    }

    public static void SetThingComponentSpacerCost(string thingDefName, int newComponentSpacerCost)
    {
        var def = GetDefByDefName<ThingDef>(thingDefName);

        var costDef = def?.costList.FirstOrDefault(c => c.thingDef == ThingDefOf.ComponentSpacer);
        if (costDef != null)
        {
            costDef.count = newComponentSpacerCost;
        }
    }
}