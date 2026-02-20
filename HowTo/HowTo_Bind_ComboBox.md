# How To Bind a ComboBox

## Goal
Bind a `ComboBox` to a list in the ViewModel and track selected value.

## 1. ViewModel Properties
```csharp
using System.Collections.ObjectModel;
using AutomationShell48.Core.MVVM;

public ObservableCollection<string> States { get; } =
    new ObservableCollection<string> { "", "AL", "AK", "AZ", "CA", "TX" };

private string _selectedState;
public string SelectedState
{
    get => _selectedState;
    set => SetProperty(ref _selectedState, value);
}
```

## 2. XAML Binding
```xml
<ComboBox ItemsSource="{Binding States}"
          SelectedItem="{Binding SelectedState, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

## 3. Optional Display/Value Pair
If you bind objects instead of strings:
```xml
<ComboBox ItemsSource="{Binding Locations}"
          DisplayMemberPath="Name"
          SelectedValuePath="Code"
          SelectedValue="{Binding SelectedLocationCode}" />
```

## 4. Notes
- Prefer `SelectedItem` for simple string lists.
- Keep source collections in `ObservableCollection<T>` for runtime updates.
