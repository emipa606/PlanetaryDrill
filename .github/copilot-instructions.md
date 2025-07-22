# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose

This mod enables the implementation of Planetary Drilling in RimWorld. It provides players with the ability to drill and mine specific resources from the planet's surface using a customizable drill recipe mechanic. By integrating richly defined settings and systems, this mod enriches the gameplay experience for those seeking deeper resource management strategies.

## Key Features and Systems

- **Drillable Resources**: The introduction of `DrillData` allows mod users to define resources that can be drilled, specifying work and yield amounts.
- **Customizable Settings**: Through the `PD_Settings` and `SettingsData` classes, users can access and modify the settings related to drillable items, allowing personalized configurations.
- **Visual Enhancements**: The `SettingsRenderer` class provides a customized user interface for mod settings.
- **Utilities and Extensions**: The `Utils` namespace provides a suite of helper functions for table management and mathematical calculations, useful for expanding UI or data management within the mod.

## Coding Patterns and Conventions

- Utilize **static classes** for utility-related methods where instantiating objects isn't necessary, as seen in `Constants` and `Utils`.
- Follow the **public and private method** access pattern to ensure encapsulation for critical mod functions.
- Adopt **self-documenting** code practices by using descriptive method names, such as `CanItemBeDrilledAccordingToSettings` and `CreateDrillRecipeDef`, which indicate their functionality clearly.
- Embrace **object-oriented programming**, as demonstrated in the inheritance structures within the `TableEntity` classes that create an expandable system for table entities.

## XML Integration

Note that the XML files are critical for defining in-game objects and def entries. Since the XML parsing failed in the summary, make sure that:
- XML files conform to a valid schema and are correctly formatted.
- The `ThingDef` names and attributes in the XML align with those referenced within C# code, ensuring seamless integration and usage across the mod.

## Harmony Patching

While the code summary didn’t explicitly list any Harmony usage, it’s typical in RimWorld modding for method patches. Ensure:
- **Harmony patches** are declared correctly, targeting methods in RimWorld assemblies for desired modifications.
- Use **prefix and postfix** methods to inject code before or after original methods respectfully and make meta-programming adjustments needed for the gameplay.
- Keep patches modular and scoped to limit performance overhead and conflicts with other mods.

## Suggestions for Copilot

When using GitHub Copilot with this project:
- Encourage Copilot to assist with repetitive tasks like `public void ExposeData()` for serialization, adhering to the RimWorld modding standards for compatibility.
- Leverage Copilot to produce boilerplate code, particularly when extending settings UI with the `SettingsRenderer`.
- Utilize Copilot’s capability to suggest similar patterns for method creation, enhancing `DrillData` logic or expanding utilities in `Utils`.
- Explore its ability to auto-complete XML files if new ThingDefs or Recipes are added, ensuring correct schema adherence.

By following these guidelines, you can ensure the mod is well-structured, enhancing both its functionality and maintainability for future improvements or updates.
