// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using UnityEngine;

namespace KSPDev.GUIUtils {

/// <summary>Utility class to draw a simple table with the text column contents.</summary>
/// <remarks>
/// <p>
/// This table cannot hold non-string content. It keeps all the columns to be of the same size, and the
/// size is adjusted to the maximum column's size in all the rows. There is a one frame delay between
/// the content change and the column resizing, which may result in flickering if the content
/// changes too frequently. The columns try to take as small space as possible, so defining the
/// minimum size may be a good bet.
/// </p>
/// <p>
/// This class is designed to be called on every frame. It's heavily performance optimized.
/// </p>
/// </remarks>
public class GUILayoutStringTable {
  /// <summary>Tells if the maximum size of the columns should be persistent between the frames.</summary>
  /// <remarks>
  /// <p>
  /// If set to <c>true</c>, then the new frame will use the previous frame's data to determine the minimum column
  /// sizes. Thus, the table could grow, but it never shrinks (unless <see cref="ResetMaxSizes"/> is called). With this
  /// setting set to <c>false</c>, the table size will be recalculated from the scratch on every frame. 
  /// </p>
  /// <p>This property can be modified at any time, but it will have effect on the next frame only.</p>
  /// </remarks>
  /// <seealso cref="ResetMaxSizes"/>
  public bool keepMaxSize { get; set; }

  /// <summary>Index of the currently rendered column.</summary>
  int currentIndex;

  /// <summary>Current frame maximum widths of the columns.</summary>
  float[] columnWidths;

  /// <summary>The maximum widths of the columns from the previous frame.</summary>
  float[] lastFrameColumnWidths;

  /// <summary>Creates a table of the specified column width.</summary>
  /// <remarks>
  /// It's OK to render more columns than reserved. They won't resized, but it's not an error.
  /// </remarks>
  /// <param name="columns">The number of columns to track.</param>
  /// <param name="keepMaxSize">
  /// Tells if the maximum sizes should be persisted. See <see cref="keepMaxSize"/> property for more details.
  /// </param>
  /// <seealso cref="ResetMaxSizes"/>
  public GUILayoutStringTable(int columns, bool keepMaxSize = false) {
    columnWidths = new float[columns];
    lastFrameColumnWidths = new float[columns];
    this.keepMaxSize = keepMaxSize;
  }

  /// <summary>Resets all the accumulated maximum column sizes to zero.</summary>
  /// <remarks>
  /// Only makes sense when the <see cref="keepMaxSize"/> mode is enabled. When this mode is disabled, the column sizes
  /// are updated on every frame.
  /// </remarks>
  public void ResetMaxSizes() {
    columnWidths = new float[columnWidths.Length];
    lastFrameColumnWidths = new float[lastFrameColumnWidths.Length];
  }

  /// <summary>Updates the table state each frame to remember the best column size values.</summary>
  /// <remarks>
  /// This method is only interested in the <c>EventType.Layout</c> phase, so no need to call it on
  /// each GUI event when there is a cheap way to detect it.
  /// </remarks>
  public void UpdateFrame() {
    if (Event.current.type == EventType.Layout) {
      lastFrameColumnWidths = columnWidths;
      columnWidths = new float[lastFrameColumnWidths.Length];
    }
  }

  /// <summary>Tells that a new row is about to be rendered.</summary>
  /// <remarks>Call it before every new row.</remarks>
  public void StartNewRow() {
    currentIndex = 0;
  }

  /// <summary>Adds a text column into the table.</summary>
  /// <param name="text">The text to add.</param>
  /// <param name="style">
  /// The style to apply to the text. If not set, then <c>GUI.skin.label</c> is used.
  /// </param>
  public void AddTextColumn(string text, GUIStyle style = null) {
    AddTextColumn(new GUIContent(text), style ?? GUI.skin.label);
  }

  /// <summary>Adds a text column into the table.</summary>
  /// <param name="text">The text to add.</param>
  /// <param name="message">The localizable message to get the minimum size from.</param>
  /// <param name="style">
  /// The style to apply to the text. If not set, then <c>GUI.skin.label</c> is used.
  /// </param>
  /// <seealso cref="LocalizableMessage"/>
  public void AddTextColumn(string text, LocalizableMessage message, GUIStyle style = null) {
    AddTextColumn(new GUIContent(text), style ?? GUI.skin.label,
                  minWidth: message.guiTags.minWidth, maxWidth: message.guiTags.maxWidth);
  }

  /// <summary>Adds a text column into the table with a value tooltip.</summary>
  /// <param name="text">The text to add.</param>
  /// <param name="tooltip">
  /// The tooltip for the column value. Note, that the tooltip is not handled by the table, it gets
  /// rendered by the Unity GUI functionality, which may need to be configured.
  /// </param>
  /// <param name="style">
  /// The style to apply to the text. If not set, then <c>GUI.skin.label</c> is used.
  /// </param>
  public void AddTextColumn(string text, string tooltip, GUIStyle style = null) {
    AddTextColumn(new GUIContent(text, tooltip), style ?? GUI.skin.label);
  }

  /// <summary>Adds a text column into the table with a value tooltip.</summary>
  /// <param name="text">The text to add.</param>
  /// <param name="tooltip">
  /// The tooltip for the column value. Note, that the tooltip is not handled by the table, it gets
  /// rendered by the Unity GUI functionality, which may need to be configured.
  /// </param>
  /// <param name="message">The localizable message to get the minimum size from.</param>
  /// <param name="style">
  /// The style to apply to the text. If not set, then <c>GUI.skin.label</c> is used.
  /// </param>
  /// <seealso cref="LocalizableMessage"/>
  public void AddTextColumn(string text, string tooltip, LocalizableMessage message,
                            GUIStyle style = null) {
    AddTextColumn(new GUIContent(text, tooltip), style ?? GUI.skin.label,
                  minWidth: message.guiTags.minWidth, maxWidth: message.guiTags.maxWidth);
  }

  /// <summary>Adds a content into the table column.</summary>
  /// <remarks>
  /// When possible, this method should be preferred over the other methods, which are simply the
  /// shortcuts to this one.</remarks>
  /// <param name="content">The text/tooltip content of the column to add.</param>
  /// <param name="style">The style to apply to the text.</param>
  /// <param name="minWidth">The minimum width of the column.</param>
  /// <param name="maxWidth">The maximum width of the column.</param>
  public void AddTextColumn(GUIContent content, GUIStyle style,
                            float minWidth = 0, float maxWidth = float.PositiveInfinity) {
    if (currentIndex >= columnWidths.Length) {
      // This column was not planned by the caller, so simply pass it through.
      GUILayout.Label(content, style);
      return;
    }
    if (Event.current.type == EventType.Layout) {
      // In the layout phase only calculates the size. Don't limit or resize the width of the area. 
      var size = style.CalcSize(content);
      var width = Mathf.Min(Mathf.Max(size.x, minWidth), maxWidth);
      if (width < columnWidths[currentIndex]) {
        width = columnWidths[currentIndex];
      }
      if (keepMaxSize && width < lastFrameColumnWidths[currentIndex]) {
        width = lastFrameColumnWidths[currentIndex];
      }
      columnWidths[currentIndex] = width;
    }
    GUILayout.Label(content, style, GUILayout.Width(columnWidths[currentIndex]));
    currentIndex++;
  }
}

}  // namespace
