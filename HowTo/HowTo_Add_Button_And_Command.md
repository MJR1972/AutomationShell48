# How To Add a Button and Command

## Goal
Add a button in XAML and bind it to a ViewModel command using `RelayCommand`.

## 1. Add Command Field and Property
In your ViewModel:

```csharp
using AutomationShell48.Core.MVVM;

private readonly RelayCommand _saveCommand;

public MyPageViewModel()
{
    _saveCommand = new RelayCommand(Save, CanSave);
}

public RelayCommand SaveCommand => _saveCommand;
```

## 2. Implement Command Methods
```csharp
private void Save()
{
    // Business logic here.
}

private bool CanSave()
{
    return true;
}
```

## 3. Trigger CanExecute Refresh
Whenever dependent state changes:

```csharp
public string Name
{
    get => _name;
    set
    {
        if (SetProperty(ref _name, value))
        {
            _saveCommand.RaiseCanExecuteChanged();
        }
    }
}
```

## 4. Bind Button in XAML
```xml
<Button Content="Save"
        Style="{StaticResource PrimaryButtonStyle}"
        Command="{Binding SaveCommand}" />
```

## 5. Notes
- Keep business logic in the ViewModel, not code-behind.
- Use `SecondaryButtonStyle`/`DangerButtonStyle` for action hierarchy.
