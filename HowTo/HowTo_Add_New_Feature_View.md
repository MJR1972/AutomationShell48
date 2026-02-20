# How To Add a New Feature View

## Goal
Create a new feature page and wire it into shell navigation.

## 1. Create Feature Files
Create a folder under `AutomationShell48.UI/Features`:

```text
Features/
  MyFeature/
    MyFeaturePageView.xaml
    MyFeaturePageView.xaml.cs
    MyFeaturePageViewModel.cs
```

## 2. Create ViewModel
Derive from `BaseViewModel` and set the page title:

```csharp
using AutomationShell48.Core.MVVM;

namespace AutomationShell48.UI.Features.MyFeature
{
    public class MyFeaturePageViewModel : BaseViewModel
    {
        public MyFeaturePageViewModel()
        {
            Title = "My Feature";
        }
    }
}
```

## 3. Create View
Use existing shared styles:

```xml
<UserControl x:Class="AutomationShell48.UI.Features.MyFeature.MyFeaturePageView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <TextBlock Text="My Feature" Style="{StaticResource H1TextStyle}" />
    </Grid>
</UserControl>
```

## 4. Register DataTemplate
Edit `AutomationShell48.UI/App.xaml`:

```xml
xmlns:myFeature="clr-namespace:AutomationShell48.UI.Features.MyFeature"

<DataTemplate DataType="{x:Type myFeature:MyFeaturePageViewModel}">
    <myFeature:MyFeaturePageView />
</DataTemplate>
```

## 5. Add Navigation Factory
Edit `AutomationShell48.UI/Infrastructure/ShellViewModel.cs`:

```csharp
["myfeature"] = () => new MyFeaturePageViewModel(),
```

## 6. Add Menu Item
In `BuildNavigation()`:

```csharp
general.Items.Add(new NavigationItem("myfeature", "My Feature", "IconInfo"));
```

## 7. Add Project Includes (Classic .csproj)
Because this project is not SDK-style, update `AutomationShell48.UI.csproj`:
- Add `<Page Include="Features\MyFeature\MyFeaturePageView.xaml" .../>`
- Add `<Compile Include="Features\MyFeature\MyFeaturePageViewModel.cs" />`
- Add `<Compile Include="Features\MyFeature\MyFeaturePageView.xaml.cs"> ... </Compile>`

## 8. Verify
1. Run app.
2. Click new nav item.
3. Confirm view loads in content area.
