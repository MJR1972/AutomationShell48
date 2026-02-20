# How To Work With ObservableCollection

## Goal
Use `ObservableCollection<T>` for UI lists that update automatically.

## Why It Matters
`ObservableCollection<T>` raises collection change notifications so controls like `ListBox` update when items are added/removed.

## 1. Define Collection
```csharp
using System.Collections.ObjectModel;

public ObservableCollection<ProjectItem> Projects { get; } =
    new ObservableCollection<ProjectItem>();
```

## 2. Add Items
```csharp
Projects.Add(new ProjectItem
{
    Name = "Automation Migration",
    Owner = "Platform Team"
});
```

## 3. Remove Items
```csharp
if (SelectedProject != null)
{
    Projects.Remove(SelectedProject);
}
```

## 4. Update Item Fields
Collection notifications do not cover property changes on items.  
For item field edits to refresh UI, item type should inherit `ObservableObject` (or implement `INotifyPropertyChanged`).

```csharp
public class ProjectItem : ObservableObject
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

## 5. Common Pattern
Use a selected item plus form buffer:
1. Select item from list.
2. Copy to form fields.
3. Apply update with explicit command.
