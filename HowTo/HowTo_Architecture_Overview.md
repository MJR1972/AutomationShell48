# AutomationShell48 Architecture Overview

## Purpose
This template is a WPF MVVM shell designed for reusable feature slices.  
Core responsibilities are separated across `Core`, `Infrastructure`, and `UI`.

## Solution Layout
- `AutomationShell48.Core`
  - MVVM primitives (`ObservableObject`, `BaseViewModel`, `RelayCommand`)
  - Service contracts
  - Navigation and dialog abstractions
- `AutomationShell48.Infrastructure`
  - Service implementations (logging, settings, theme service support)
- `AutomationShell48.UI`
  - Views, ViewModels, resources, converters, shell composition

## Runtime Composition
`App.xaml.cs` performs composition root responsibilities:
1. Construct infrastructure services.
2. Register services in `AppServices`.
3. Create `ShellViewModel`.
4. Create `MainWindow` and assign `DataContext`.
5. Navigate to last selected page key.

## Navigation Pattern
Navigation is key-based:
1. Shell menu item calls `NavigateCommand` with a `NavigationItem`.
2. `INavigationService.NavigateTo(key)` raises `Navigated`.
3. `ShellViewModel.OnNavigated(key)` resolves a ViewModel factory from `_viewFactories`.
4. `CurrentViewModel` is swapped.
5. `App.xaml` DataTemplates map ViewModel type to View.

## Theme System
Theme resources are loaded from merged dictionaries:
- `Shared/Resources/Light.xaml`
- `Shared/Resources/Dark.xaml`

Always prefer `DynamicResource` for theme-aware UI elements.

## Page Construction Pattern
Each feature page typically includes:
- `FeaturePageView.xaml`
- `FeaturePageView.xaml.cs` (InitializeComponent only)
- `FeaturePageViewModel.cs`

Common structure:
```csharp
public class ExamplePageViewModel : BaseViewModel
{
    public ExamplePageViewModel()
    {
        Title = "Example";
    }
}
```

## Extension Checklist
When adding a new feature:
1. Add feature folder and files.
2. Add ViewModel factory in `ShellViewModel`.
3. Add navigation item in `BuildNavigation`.
4. Add DataTemplate in `App.xaml`.
5. Add project includes in `AutomationShell48.UI.csproj` (classic project style).
