[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string]$FeatureName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-FeatureName {
    param([string]$Name)

    $clean = ($Name -replace '[^A-Za-z0-9_]', '')
    if ([string]::IsNullOrWhiteSpace($clean)) {
        throw "FeatureName must contain at least one letter or number."
    }

    if ($clean[0] -match '\d') {
        $clean = "F$clean"
    }

    return $clean
}

function Split-CamelCase {
    param([string]$InputText)

    $withSpaces = [regex]::Replace($InputText, '(?<=[a-z0-9])([A-Z])', ' $1')
    return ($withSpaces -replace '_', ' ').Trim()
}

function Write-FileIfMissing {
    param(
        [string]$Path,
        [string]$Content
    )

    if (Test-Path $Path) {
        Write-Host "Skipping existing file: $Path"
        return
    }

    if ($PSCmdlet.ShouldProcess($Path, "Create file")) {
        Set-Content -Path $Path -Value $Content -Encoding UTF8
        Write-Host "Created: $Path"
    }
}

function Update-FileText {
    param(
        [string]$Path,
        [scriptblock]$UpdateBlock
    )

    if (!(Test-Path $Path)) {
        throw "Required file not found: $Path"
    }

    $original = Get-Content -Path $Path -Raw -Encoding UTF8
    $updated = & $UpdateBlock $original

    if ($updated -eq $original) {
        Write-Host "No changes needed: $Path"
        return
    }

    if ($PSCmdlet.ShouldProcess($Path, "Update file")) {
        Set-Content -Path $Path -Value $updated -Encoding UTF8
        Write-Host "Updated: $Path"
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$uiRoot = Join-Path $repoRoot "AutomationShell48.UI"

if (!(Test-Path $uiRoot)) {
    throw "Could not find UI project folder at: $uiRoot"
}

$feature = Normalize-FeatureName -Name $FeatureName
$displayName = Split-CamelCase -InputText $feature
$navKey = $feature.ToLowerInvariant()
$alias = $navKey
$featureNamespace = "AutomationShell48.UI.Features.$feature"
$featureFolder = Join-Path $uiRoot "Features\$feature"
$viewClass = "${feature}PageView"
$viewModelClass = "${feature}PageViewModel"

if ($alias[0] -match '\d') {
    $alias = "f$alias"
}

if ($PSCmdlet.ShouldProcess($featureFolder, "Create feature folder")) {
    New-Item -Path $featureFolder -ItemType Directory -Force | Out-Null
}

$viewXamlPath = Join-Path $featureFolder "$viewClass.xaml"
$viewCodeBehindPath = Join-Path $featureFolder "$viewClass.xaml.cs"
$viewModelPath = Join-Path $featureFolder "$viewModelClass.cs"

$viewXaml = @"
<UserControl x:Class="$featureNamespace.$viewClass"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <StackPanel>
            <TextBlock Text="$displayName" Style="{StaticResource H1TextStyle}" />
            <TextBlock Margin="0,6,0,18"
                       Style="{StaticResource BodyTextStyle}"
                       Text="$displayName page scaffolded by tools/Add-Feature.ps1." />

            <Border Style="{StaticResource CardSurfaceStyle}">
                <StackPanel>
                    <TextBlock Text="$displayName Overview" Style="{StaticResource H3TextStyle}" />
                    <TextBlock Margin="0,10,0,0"
                               Style="{StaticResource BodyTextStyle}"
                               Text="Add page-specific UI and commands in $viewModelClass." />
                </StackPanel>
            </Border>
        </StackPanel>

        <Border Background="#66000000" Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibilityConverter}}">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                <ProgressBar IsIndeterminate="True" Width="240" Height="10" />
                <TextBlock Margin="0,10,0,0" Text="{Binding BusyMessage}" Foreground="White" />
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
"@

$viewCodeBehind = @"
using System.Windows.Controls;

namespace $featureNamespace
{
    public partial class $viewClass : UserControl
    {
        public $viewClass()
        {
            InitializeComponent();
        }
    }
}
"@

$viewModelCode = @"
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;

namespace $featureNamespace
{
    public class $viewModelClass : BaseViewModel
    {
        private readonly ILogger _logger;

        public $viewModelClass(ILogger logger)
        {
            _logger = logger;
            Title = "$displayName";
            _logger?.Info("$displayName view loaded.");
        }
    }
}
"@

Write-FileIfMissing -Path $viewXamlPath -Content $viewXaml
Write-FileIfMissing -Path $viewCodeBehindPath -Content $viewCodeBehind
Write-FileIfMissing -Path $viewModelPath -Content $viewModelCode

$appXamlPath = Join-Path $uiRoot "App.xaml"
$shellPath = Join-Path $uiRoot "Infrastructure\ShellViewModel.cs"
$csprojPath = Join-Path $uiRoot "AutomationShell48.UI.csproj"

Update-FileText -Path $appXamlPath -UpdateBlock {
    param($text)

    $xmlnsLine = "xmlns:$alias=`"clr-namespace:$featureNamespace`""
    if ($text -notmatch [regex]::Escape($xmlnsLine)) {
        $text = $text -replace '(\s+xmlns:coreDialogs=)', "             $xmlnsLine`r`n`$1"
    }

    $dataTemplate = @"
            <DataTemplate DataType="{x:Type ${alias}:$viewModelClass}">
                <${alias}:$viewClass />
            </DataTemplate>

"@

    if ($text -notmatch [regex]::Escape("DataType=`"{x:Type ${alias}:$viewModelClass}`"")) {
        $text = $text -replace '(\s+<DataTemplate DataType="\{x:Type coreDialogs:MessageDialogViewModel\}">)', "$dataTemplate`$1"
    }

    return $text
}

Update-FileText -Path $shellPath -UpdateBlock {
    param($text)

    $usingLine = "using $featureNamespace;"
    if ($text -notmatch [regex]::Escape($usingLine)) {
        $text = $text -replace '(using AutomationShell48\.UI\.Features\.Users;)', "`$1`r`n$usingLine"
    }

    $factoryLine = "                [`"$navKey`"] = () => new $viewModelClass(_logger),"
    if ($text -notmatch [regex]::Escape("`"$navKey`"")) {
        $text = $text -replace '(\s+\["settings"\]\s*=\s*\(\)\s*=>\s*new SettingsPageViewModel\([^\r\n]*\),)', "$factoryLine`r`n`$1"
    }

    $navLine = "            general.Items.Add(new NavigationItem(`"$navKey`", `"$displayName`", `"IconInfo`"));"
    if ($text -notmatch [regex]::Escape("NavigationItem(`"$navKey`"")) {
        $text = $text -replace '(            general\.Items\.Add\(new NavigationItem\("projects", "Projects", "IconFolder"\)\);)', "`$1`r`n$navLine"
    }

    return $text
}

Update-FileText -Path $csprojPath -UpdateBlock {
    param($text)

    $pageInclude = @"
    <Page Include="Features\$feature\$viewClass.xaml">
      <Generator>MSBuild:Compile</Generator>
      <SubType>Designer</SubType>
    </Page>
"@

    if ($text -notmatch [regex]::Escape("Features\$feature\$viewClass.xaml")) {
        $text = $text -replace '(\s*<Page Include="Features\\About\\AboutPageView\.xaml">)', "$pageInclude`r`n`$1"
    }

    $vmCompile = "    <Compile Include=`"Features\$feature\$viewModelClass.cs`" />"
    if ($text -notmatch [regex]::Escape("Features\$feature\$viewModelClass.cs")) {
        $text = $text -replace '(\s*<Compile Include="Infrastructure\\ShellViewModel\.cs"\s*/>)', "$vmCompile`r`n`$1"
    }

    $viewCompile = @"
    <Compile Include="Features\$feature\$viewClass.xaml.cs">
      <DependentUpon>$viewClass.xaml</DependentUpon>
    </Compile>
"@
    if ($text -notmatch [regex]::Escape("Features\$feature\$viewClass.xaml.cs")) {
        $text = $text -replace '(\s*<Compile Include="Features\\About\\AboutPageView\.xaml\.cs">)', "$viewCompile`r`n`$1"
    }

    return $text
}

Write-Host ""
Write-Host "Feature scaffold complete for: $feature"
Write-Host "Navigation key: $navKey"
Write-Host "Created files under: Features\$feature"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1) Review generated files."
Write-Host "2) Build solution."
Write-Host "3) Run app and click '$displayName' in left navigation."
