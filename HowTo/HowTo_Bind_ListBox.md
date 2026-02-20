# How To Bind a ListBox

## Goal
Display a collection and bind selected item for edit/detail workflows.

## 1. ViewModel Properties
```csharp
using System.Collections.ObjectModel;

public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

private User _selectedUser;
public User SelectedUser
{
    get => _selectedUser;
    set => SetProperty(ref _selectedUser, value);
}
```

## 2. XAML Binding
```xml
<ListBox ItemsSource="{Binding Users}"
         SelectedItem="{Binding SelectedUser, Mode=TwoWay}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DisplayLabel}" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

## 3. React to Selection in ViewModel
```csharp
public User SelectedUser
{
    get => _selectedUser;
    set
    {
        if (SetProperty(ref _selectedUser, value))
        {
            LoadSelectedUserIntoForm();
        }
    }
}
```

## 4. Notes
- Use `ItemTemplate` for custom row content.
- Make item model implement `INotifyPropertyChanged` for live row refresh.
