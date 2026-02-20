# How To Add a New Feature (Using Add-Feature Tool)

## Goal
Generate a new feature slice and all required wiring automatically.

## Tool
Script path:
`tools/Add-Feature.ps1`

## 1. Dry Run First (No Changes)
Preview exactly what will be created and modified:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Add-Feature.ps1 -FeatureName NewView -WhatIf
```

## 2. Run for Real
Create and wire the feature:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Add-Feature.ps1 -FeatureName NewView
```

## 3. What the Tool Creates
1. `AutomationShell48.UI/Features/NewView/NewViewPageView.xaml`
2. `AutomationShell48.UI/Features/NewView/NewViewPageView.xaml.cs`
3. `AutomationShell48.UI/Features/NewView/NewViewPageViewModel.cs`

## 4. What the Tool Updates
1. `AutomationShell48.UI/App.xaml`
   - adds feature xmlns alias
   - adds DataTemplate ViewModel -> View mapping
2. `AutomationShell48.UI/Infrastructure/ShellViewModel.cs`
   - adds feature namespace `using`
   - adds `_viewFactories` entry
   - adds left navigation item
3. `AutomationShell48.UI/AutomationShell48.UI.csproj`
   - adds `<Page Include=...>` for XAML
   - adds `<Compile Include=...>` for ViewModel/code-behind

## 5. Naming Rules
- Input `FeatureName` should be a class-safe name (letters/numbers/underscore).
- Navigation key is generated in lowercase.
- Display label is generated from camel case (example: `NewView` -> `New View`).

## 6. Validation Checklist
1. Run `git diff` and verify expected changes only.
2. Build solution.
3. Run app.
4. Click the new feature in left navigation.
5. Confirm generated page appears.

## 7. If You Need Full Manual Control
Use:
`HowTo_Add_New_Feature_Manual.md`
