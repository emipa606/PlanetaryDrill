using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SquirtingElephant.Helpers;

public class TableData
{
    private const float DEFAULT_ROW_HEIGHT = 32f;

    private readonly bool UpdateEnabled = true;

    /// <summary>
    ///     Used privately within methods to temporarily disable the updating without changing the UpdateEnabled setting.
    ///     Please set this value back to true at the end of your method or whatever you are doing.
    /// </summary>
    private bool PrivateUpdateEnabled = true;

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
        initialize(tableOffset, spacing, colWidths, rowHeights, colCount, rowCount);
    }

    public float Bottom => TableRect.yMax;

    private Vector2 Spacing { get; set; }

    private Vector2 TableOffset { get; set; }

    /// <summary>
    ///     Column Datas.
    /// </summary>
    public List<TableColumn> Columns { get; } = [];

    public List<TableRow> Rows { get; } = [];

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
    private void initialize(Vector2 tableOffset, Vector2 tableSpacing, float[] colWidths, float[] rowHeights,
        int colCount = -1, int rowCount = -1)
    {
        // For performance reasons disable the updating and update after the table initialization instead.
        PrivateUpdateEnabled = false;

        TableOffset = tableOffset;
        Spacing = tableSpacing;

        // Add Columns.
        foreach (var colWidth in colWidths)
        {
            addColumns(colWidth);
        }

        addColumns(colWidths.Last(), colCount - colWidths.Length);

        // Add Rows.
        foreach (var rowHeight in rowHeights)
        {
            addRow(rowHeight);
        }

        addRow(getLastRowHeight(), rowCount - Rows.Count);

        PrivateUpdateEnabled = true;
        Update();
    }

    public void Update(bool force = false)
    {
        if ((!UpdateEnabled || !PrivateUpdateEnabled) && !force)
        {
            return;
        }

        setTableRect();
        updateColumns();
        updateRowsAndFields();
    }

    private float calcTableWidth()
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

    private float calcTableHeight()
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

    private void setTableRect()
    {
        TableRect = new Rect(TableOffset.x, TableOffset.y, calcTableWidth(), calcTableHeight());
    }

    /// <summary>
    ///     Please also call UpdateRowsAndFields() after changing any column through here.
    /// </summary>
    private void updateColumns()
    {
        var nextColStartX = TableRect.x;
        foreach (var col in Columns)
        {
            col.Rect = new Rect(nextColStartX, TableRect.y, col.Width, TableRect.height);
            nextColStartX = col.Rect.xMax + Spacing.x;
        }
    }

    private void updateRowsAndFields()
    {
        var nextRowStartY = TableRect.y;
        foreach (var row in Rows)
        {
            row.Rect = new Rect(TableRect.x, nextRowStartY, TableRect.width, row.Height);
            nextRowStartY = row.Rect.yMax + Spacing.y;
            row.UpdateFields();
        }
    }

    /// <summary>
    ///     Note: Will do nothing if <c>amount</c> is zero or less.
    /// </summary>
    private void addRow(float rowHeight, int amount = 1)
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
    private void addColumns(float colWidth, float amount = 1)
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

    private void createRowsUntil(int rowIdx)
    {
        addRow(getLastRowHeight(), rowIdx + 1 - Rows.Count);
    }

    private TableField getField(int colIdx, int rowIdx)
    {
        if (colIdx >= Columns.Count)
        {
            Log.Error($"Attemped to access a column that's out of bounds. Received: {colIdx}.");
            return TableField.Invalid;
        }

        createRowsUntil(rowIdx);
        return Rows[rowIdx].Fields[colIdx];
    }

    private float getLastRowHeight()
    {
        return Rows.Count > 0 ? Rows.Last().Height : DEFAULT_ROW_HEIGHT;
    }

    public Rect GetRowRect(int rowIdx)
    {
        createRowsUntil(rowIdx);
        return Rows[rowIdx].Rect;
    }

    public Rect GetFieldRect(int colIdx, int rowIdx)
    {
        return getField(colIdx, rowIdx).Rect;
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
}