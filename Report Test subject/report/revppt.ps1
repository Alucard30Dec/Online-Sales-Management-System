Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-OfficeColor {
    param(
        [int]$Red,
        [int]$Green,
        [int]$Blue
    )

    return $Red + (256 * $Green) + (65536 * $Blue)
}

function Add-TextShape {
    param(
        $Slide,
        [double]$Left,
        [double]$Top,
        [double]$Width,
        [double]$Height,
        [string]$Text,
        [double]$FontSize,
        [int]$FontColor,
        [string]$FontName = 'Segoe UI',
        [bool]$Bold = $false,
        [int]$Align = 1,
        [Nullable[int]]$FillColor = $null,
        [Nullable[int]]$LineColor = $null,
        [double]$FillTransparency = 0,
        [switch]$Rounded,
        [int]$Margin = 8
    )

    if ($FillColor -ne $null -or $LineColor -ne $null -or $Rounded.IsPresent) {
        $shapeType = if ($Rounded.IsPresent) { 5 } else { 1 }
        $shape = $Slide.Shapes.AddShape($shapeType, $Left, $Top, $Width, $Height)
    }
    else {
        $shape = $Slide.Shapes.AddTextbox(1, $Left, $Top, $Width, $Height)
    }

    if ($FillColor -ne $null) {
        $shape.Fill.Visible = -1
        $shape.Fill.Solid()
        $shape.Fill.ForeColor.RGB = [int]$FillColor
        $shape.Fill.Transparency = $FillTransparency
    }
    else {
        $shape.Fill.Visible = 0
    }

    if ($LineColor -ne $null) {
        $shape.Line.Visible = -1
        $shape.Line.ForeColor.RGB = [int]$LineColor
        $shape.Line.Weight = 1
    }
    else {
        $shape.Line.Visible = 0
    }

    $shape.TextFrame.TextRange.Text = $Text
    $shape.TextFrame.TextRange.Font.Name = $FontName
    $shape.TextFrame.TextRange.Font.Size = $FontSize
    $shape.TextFrame.TextRange.Font.Bold = if ($Bold) { -1 } else { 0 }
    $shape.TextFrame.TextRange.Font.Color.RGB = $FontColor
    $shape.TextFrame.TextRange.ParagraphFormat.Alignment = $Align
    $shape.TextFrame.WordWrap = -1
    $shape.TextFrame.MarginLeft = $Margin
    $shape.TextFrame.MarginRight = $Margin
    $shape.TextFrame.MarginTop = $Margin
    $shape.TextFrame.MarginBottom = $Margin

    return $shape
}

function Add-Rectangle {
    param(
        $Slide,
        [double]$Left,
        [double]$Top,
        [double]$Width,
        [double]$Height,
        [int]$FillColor,
        [Nullable[int]]$LineColor = $null,
        [double]$FillTransparency = 0,
        [switch]$Rounded
    )

    $shapeType = if ($Rounded.IsPresent) { 5 } else { 1 }
    $shape = $Slide.Shapes.AddShape($shapeType, $Left, $Top, $Width, $Height)
    $shape.Fill.Visible = -1
    $shape.Fill.Solid()
    $shape.Fill.ForeColor.RGB = $FillColor
    $shape.Fill.Transparency = $FillTransparency

    if ($LineColor -ne $null) {
        $shape.Line.Visible = -1
        $shape.Line.ForeColor.RGB = [int]$LineColor
        $shape.Line.Weight = 1
    }
    else {
        $shape.Line.Visible = 0
    }

    return $shape
}

function Add-PicturePanel {
    param(
        $Slide,
        [string]$ImagePath,
        [double]$Left,
        [double]$Top,
        [double]$Width,
        [double]$Height,
        [string]$Caption,
        [int]$CaptionColor,
        [int]$BorderColor,
        [int]$SurfaceColor
    )

    Add-Rectangle -Slide $Slide -Left $Left -Top $Top -Width $Width -Height $Height -FillColor $SurfaceColor -LineColor $BorderColor -Rounded | Out-Null
    if (Test-Path $ImagePath) {
        $pic = $Slide.Shapes.AddPicture((Resolve-Path $ImagePath).Path, 0, -1, $Left + 8, $Top + 8, $Width - 16, $Height - 36)
        $pic.LockAspectRatio = -1
    }
    else {
        Add-TextShape -Slide $Slide -Left ($Left + 10) -Top ($Top + 10) -Width ($Width - 20) -Height ($Height - 46) -Text "Missing image`n$ImagePath" -FontSize 14 -FontColor $CaptionColor -FontName 'Segoe UI' | Out-Null
    }
    Add-TextShape -Slide $Slide -Left ($Left + 8) -Top ($Top + $Height - 26) -Width ($Width - 16) -Height 18 -Text $Caption -FontSize 11 -FontColor $CaptionColor -FontName 'Segoe UI' | Out-Null
}

function Add-SectionHeader {
    param(
        $Slide,
        [string]$StepLabel,
        [string]$Title,
        [string]$Subtitle,
        [int]$AccentColor,
        [int]$TitleColor,
        [int]$SubtitleColor,
        [double]$SlideWidth
    )

    Add-Rectangle -Slide $Slide -Left 48 -Top 32 -Width 36 -Height 6 -FillColor $AccentColor | Out-Null
    Add-TextShape -Slide $Slide -Left 48 -Top 40 -Width 190 -Height 34 -Text $StepLabel -FontSize 11 -FontColor $AccentColor -FontName 'Segoe UI' -Bold $true | Out-Null
    Add-TextShape -Slide $Slide -Left 48 -Top 66 -Width ($SlideWidth - 96) -Height 60 -Text $Title -FontSize 24 -FontColor $TitleColor -FontName 'Bahnschrift SemiBold' -Bold $true | Out-Null
    Add-TextShape -Slide $Slide -Left 48 -Top 122 -Width ($SlideWidth - 96) -Height 30 -Text $Subtitle -FontSize 11 -FontColor $SubtitleColor -FontName 'Segoe UI' | Out-Null
}

function Add-MetricCard {
    param(
        $Slide,
        [double]$Left,
        [double]$Top,
        [double]$Width,
        [double]$Height,
        [string]$Value,
        [string]$Label,
        [int]$AccentColor,
        [int]$SurfaceColor,
        [int]$PrimaryTextColor,
        [int]$SecondaryTextColor,
        [int]$BorderColor
    )

    Add-Rectangle -Slide $Slide -Left $Left -Top $Top -Width $Width -Height $Height -FillColor $SurfaceColor -LineColor $BorderColor -Rounded | Out-Null
    Add-Rectangle -Slide $Slide -Left $Left -Top $Top -Width 8 -Height $Height -FillColor $AccentColor -Rounded | Out-Null
    Add-TextShape -Slide $Slide -Left ($Left + 20) -Top ($Top + 18) -Width ($Width - 28) -Height 34 -Text $Value -FontSize 24 -FontColor $PrimaryTextColor -FontName 'Bahnschrift SemiBold' -Bold $true | Out-Null
    Add-TextShape -Slide $Slide -Left ($Left + 20) -Top ($Top + 54) -Width ($Width - 28) -Height 26 -Text $Label -FontSize 11 -FontColor $SecondaryTextColor -FontName 'Segoe UI' | Out-Null
}

function Add-Chip {
    param(
        $Slide,
        [double]$Left,
        [double]$Top,
        [double]$Width,
        [double]$Height,
        [string]$Text,
        [int]$FillColor,
        [int]$TextColor
    )

    Add-TextShape -Slide $Slide -Left $Left -Top $Top -Width $Width -Height $Height -Text $Text -FontSize 10 -FontColor $TextColor -FontName 'Segoe UI' -Bold $true -Align 2 -FillColor $FillColor -Rounded -Margin 4 | Out-Null
}

function Add-ComparisonCard {
    param(
        $Slide,
        [double]$Left,
        [double]$Top,
        [double]$Width,
        [double]$Height,
        [string]$CaseId,
        [string]$Title,
        [string]$Expected,
        [string]$Actual,
        [string]$Evidence,
        [string]$Status,
        [int]$StatusColor,
        [int]$SurfaceColor,
        [int]$BorderColor,
        [int]$PrimaryTextColor,
        [int]$SecondaryTextColor
    )

    Add-Rectangle -Slide $Slide -Left $Left -Top $Top -Width $Width -Height $Height -FillColor $SurfaceColor -LineColor $BorderColor -Rounded | Out-Null
    Add-Rectangle -Slide $Slide -Left $Left -Top $Top -Width $Width -Height 8 -FillColor $StatusColor -Rounded | Out-Null
    Add-TextShape -Slide $Slide -Left ($Left + 14) -Top ($Top + 16) -Width ($Width - 92) -Height 20 -Text $CaseId -FontSize 12 -FontColor $SecondaryTextColor -FontName 'Segoe UI' -Bold $true | Out-Null
    Add-Chip -Slide $Slide -Left ($Left + $Width - 74) -Top ($Top + 14) -Width 58 -Height 20 -Text $Status -FillColor $StatusColor -TextColor (Get-OfficeColor 255 255 255)
    Add-TextShape -Slide $Slide -Left ($Left + 14) -Top ($Top + 34) -Width ($Width - 28) -Height 32 -Text $Title -FontSize 12 -FontColor $PrimaryTextColor -FontName 'Bahnschrift SemiBold' -Bold $true | Out-Null
    Add-TextShape -Slide $Slide -Left ($Left + 14) -Top ($Top + 68) -Width ($Width - 28) -Height 38 -Text "Expected`n$Expected" -FontSize 9.5 -FontColor $SecondaryTextColor -FontName 'Segoe UI' | Out-Null
    Add-TextShape -Slide $Slide -Left ($Left + 14) -Top ($Top + 108) -Width ($Width - 28) -Height 42 -Text "Actual`n$Actual" -FontSize 9.5 -FontColor $SecondaryTextColor -FontName 'Segoe UI' | Out-Null
    Add-TextShape -Slide $Slide -Left ($Left + 14) -Top ($Top + $Height - 26) -Width ($Width - 28) -Height 16 -Text "Evidence: $Evidence" -FontSize 8.5 -FontColor $SecondaryTextColor -FontName 'Segoe UI' | Out-Null
}

function Get-CaseRow {
    param(
        [array]$Rows,
        [string]$CaseId
    )

    $row = $Rows | Where-Object { $_.'Test Case ID' -eq $CaseId } | Select-Object -First 1
    if (-not $row) {
        throw "Missing result row for $CaseId"
    }
    return $row
}

function Short-IssueNumber {
    param([string]$IssueUrl)

    if (-not $IssueUrl -or $IssueUrl -eq 'N/A') {
        return 'N/A'
    }

    return '#' + ($IssueUrl.TrimEnd('/') -split '/')[-1]
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$reportRoot = Join-Path $repoRoot 'Report Test subject'
$workingRoot = Join-Path $reportRoot 'Powered by GPT'
$cleanRoot = Join-Path $reportRoot 'SV00123-ATU-A01'

$outputPath = Join-Path $PSScriptRoot 'Powered by GPT - Software Quality Verification - Presentation.pptx'
$workingPptx = Join-Path $workingRoot 'MainReport\Powered by GPT - Software Quality Verification - Presentation.pptx'
$cleanPptx = Join-Path $cleanRoot 'MainReport\Powered by GPT - Software Quality Verification - Presentation.pptx'

$workingSourceAssets = Join-Path $workingRoot 'MainReport\SourceAssets'
$workingPresentationContent = Join-Path $workingSourceAssets 'Powered by GPT - Software Quality Verification - Presentation Content.md'
$workingPresentationNotes = Join-Path $workingSourceAssets 'Powered by GPT - Software Quality Verification - Presentation Notes.md'
$rootPresentationContent = Join-Path $PSScriptRoot 'Powered by GPT - Software Quality Verification - Presentation Content.md'
$rootPresentationNotes = Join-Path $PSScriptRoot 'Powered by GPT - Software Quality Verification - Presentation Notes.md'

$finalResults = Import-Csv (Join-Path $workingRoot 'TestResults\FinalResults\OSMS-Final-Results.csv')
$uiCases = Import-Csv (Join-Path $workingRoot 'TestCases\UI\OSMS-UI-Test-Cases.csv')
$metricsRows = Import-Csv (Join-Path $workingRoot 'TestResults\Metrics\OSMS-Test-Metrics-Summary.csv')
$defectRows = Import-Csv (Join-Path $workingRoot 'TestResults\Defects\OSMS-Defect-Register.csv') | Where-Object { $_.'Record Type' -eq 'Confirmed Defect' }

$metricMap = @{}
foreach ($row in $metricsRows) {
    $metricMap[$row.Metric] = $row.Value
}

$totalCases = [int]$metricMap['Total Test Cases']
$executedCases = [int]$metricMap['Executed Test Cases']
$passCases = [int]$metricMap['Pass']
$failCases = [int]$metricMap['Fail']
$defectCount = [int]$metricMap['Confirmed Defects']
$scenarioCount = [int]$metricMap['Scenario Count In Phase 3']
$passRate = [double]$metricMap['Pass Rate On Executed %']
$failRate = [double]$metricMap['Fail Rate On Executed %']

$ownerCounts = $uiCases | Group-Object Owner | Sort-Object Name
$interfaceCounts = $finalResults | Group-Object Interface
$adminUiCount = ($interfaceCounts | Where-Object Name -eq 'Admin UI' | Select-Object -ExpandProperty Count)
$publicUiCount = ($interfaceCounts | Where-Object Name -eq 'Public UI' | Select-Object -ExpandProperty Count)
$apiCount = ($interfaceCounts | Where-Object Name -eq 'API' | Select-Object -ExpandProperty Count)

$authRow = Get-CaseRow -Rows $finalResults -CaseId 'TC-UI-AUTH-001'
$previewRow = Get-CaseRow -Rows $finalResults -CaseId 'TC-UI-IMP-002'
$purchaseFailRow = Get-CaseRow -Rows $finalResults -CaseId 'TC-UI-PUR-002'
$invoiceFailRow = Get-CaseRow -Rows $finalResults -CaseId 'TC-UI-INV-001'

$colors = @{
    DarkBg      = Get-OfficeColor 13 24 43
    DarkSurface = Get-OfficeColor 23 39 66
    Accent      = Get-OfficeColor 16 121 217
    AccentSoft  = Get-OfficeColor 219 238 255
    Teal        = Get-OfficeColor 22 163 152
    Surface     = Get-OfficeColor 246 248 252
    White       = Get-OfficeColor 255 255 255
    Border      = Get-OfficeColor 225 231 239
    Ink         = Get-OfficeColor 24 36 56
    Slate       = Get-OfficeColor 88 100 125
    Danger      = Get-OfficeColor 196 61 61
    Warning     = Get-OfficeColor 214 137 16
    Success     = Get-OfficeColor 42 127 98
    AccentSoft2 = Get-OfficeColor 237 247 255
}

$evidenceRoot = Join-Path $workingRoot 'TestResults\Evidence'
$imageAuth = Join-Path $evidenceRoot 'UI\automation\20260411_143741_TC-UI-AUTH-001-success.png'
$imagePreview = Join-Path $evidenceRoot 'UI\automation\20260406_054115_TC-UI-IMP-002-preview.png'
$imageNewman = Join-Path $evidenceRoot 'Report\OSMS-Newman-Full-Run-Snippet.png'
$imageMetrics = Join-Path $evidenceRoot 'Report\OSMS-Test-Metrics-Summary.png'
$imageIssue = Join-Path $evidenceRoot 'Defects\BUG-20260406-001-github-issue.png'

Copy-Item $workingPresentationContent $rootPresentationContent -Force
Copy-Item $workingPresentationNotes $rootPresentationNotes -Force

$powerPoint = $null
$presentation = $null

try {
    $powerPoint = New-Object -ComObject PowerPoint.Application
    $powerPoint.Visible = -1
    $presentation = $powerPoint.Presentations.Add()
    $presentation.PageSetup.SlideSize = 7
    $presentation.PageSetup.SlideWidth = 960
    $presentation.PageSetup.SlideHeight = 540
    $slideWidth = [double]$presentation.PageSetup.SlideWidth
    $slideHeight = [double]$presentation.PageSetup.SlideHeight

    # Slide 1
    $slide = $presentation.Slides.Add(1, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.DarkBg | Out-Null
    Add-Rectangle -Slide $slide -Left ($slideWidth - 220) -Top -40 -Width 260 -Height 260 -FillColor $colors.Accent -FillTransparency 0.72 | Out-Null
    Add-Rectangle -Slide $slide -Left -60 -Top 360 -Width 280 -Height 280 -FillColor $colors.Teal -FillTransparency 0.82 | Out-Null
    Add-TextShape -Slide $slide -Left 58 -Top 70 -Width 520 -Height 52 -Text 'Online Sales Management System' -FontSize 28 -FontColor $colors.White -FontName 'Bahnschrift SemiBold' -Bold $true | Out-Null
    Add-TextShape -Slide $slide -Left 58 -Top 126 -Width 420 -Height 28 -Text 'Software Testing Final Submission' -FontSize 15 -FontColor $colors.AccentSoft -FontName 'Segoe UI' -Bold $true | Out-Null
    Add-TextShape -Slide $slide -Left 58 -Top 168 -Width 540 -Height 86 -Text "Final testing presentation built from the recorded execution results and supporting artifacts.`n$executedCases / $totalCases cases executed, $passCases pass, $failCases fail, $defectCount confirmed open defects." -FontSize 16 -FontColor $colors.White -FontName 'Segoe UI' | Out-Null
    Add-TextShape -Slide $slide -Left 58 -Top 268 -Width 340 -Height 62 -Text "Team`nHoang Van Thien`nNguyen Thanh Dat`nLe Quang Duy" -FontSize 13 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -FillTransparency 0.08 -Rounded | Out-Null
    Add-TextShape -Slide $slide -Left 58 -Top 486 -Width 360 -Height 20 -Text 'Repository: github.com/Alucard30Dec/Online-Sales-Management-System' -FontSize 10 -FontColor $colors.AccentSoft -FontName 'Segoe UI' | Out-Null
    Add-MetricCard -Slide $slide -Left 582 -Top 126 -Width 132 -Height 88 -Value "$totalCases" -Label 'Total cases' -AccentColor $colors.Accent -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White
    Add-MetricCard -Slide $slide -Left 726 -Top 126 -Width 132 -Height 88 -Value '100%' -Label 'Executed' -AccentColor $colors.Teal -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White
    Add-MetricCard -Slide $slide -Left 582 -Top 226 -Width 132 -Height 88 -Value "$defectCount" -Label 'Live issues' -AccentColor $colors.Danger -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White
    Add-MetricCard -Slide $slide -Left 726 -Top 226 -Width 132 -Height 88 -Value "$failCases" -Label 'Current fail cases' -AccentColor $colors.Warning -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White

    # Slide 2
    $slide = $presentation.Slides.Add(2, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '01  Risk-Based Scope' -Title 'System Surfaces And High-Risk Areas' -Subtitle 'The scope is centered on business-critical flows where defects can affect permissions, inventory, transaction status, or reporting.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    $scopeCards = @(
        @{ X = 48;  Y = 150; W = 250; H = 94; Title = 'Admin UI'; Body = 'Authentication, roles, products, purchases, invoices, reports, and stock operations.'; Accent = $colors.Accent },
        @{ X = 318; Y = 150; W = 250; H = 94; Title = 'Public Catalog UI'; Body = 'Search, sorting, product detail access, and customer-facing catalog behavior.'; Accent = $colors.Teal },
        @{ X = 48;  Y = 258; W = 250; H = 94; Title = 'Health API'; Body = 'Environment readiness smoke used to validate API availability before deeper runs.'; Accent = $colors.Warning },
        @{ X = 318; Y = 258; W = 250; H = 94; Title = 'Catalog API'; Body = 'Filtering, paging, sort, and boundary behavior through Newman collection runs.'; Accent = $colors.Success }
    )
    foreach ($card in $scopeCards) {
        Add-MetricCard -Slide $slide -Left $card.X -Top $card.Y -Width $card.W -Height $card.H -Value $card.Title -Label $card.Body -AccentColor $card.Accent -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    }
    Add-TextShape -Slide $slide -Left 596 -Top 150 -Width 316 -Height 202 -Text "Highest-Risk Modules`n`n- Authentication and permission boundaries`n- Product import, stock, and inventory movement`n- Purchase and invoice transaction flows`n- Reports and public catalog correctness" -FontSize 15 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 16 | Out-Null
    Add-TextShape -Slide $slide -Left 596 -Top 372 -Width 316 -Height 88 -Text "Why this matters`nThese areas can create false stock, invalid revenue records, or unauthorized access if they break." -FontSize 14 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.AccentSoft2 -LineColor $colors.Warning -Rounded -Margin 16 | Out-Null

    # Slide 3
    $slide = $presentation.Slides.Add(3, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '02  Strategy And Evidence Model' -Title 'From Test Design To Recorded Evidence' -Subtitle 'This section shows how scenario design, execution outputs, and defect tracking were kept in one consistent chain.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    Add-TextShape -Slide $slide -Left 48 -Top 150 -Width 288 -Height 186 -Text "Approach`n`n- project-specific audit instead of template-only planning`n- black-box execution with white-box-informed edge cases`n- manual UI validation plus API regression plus targeted UI automation" -FontSize 14 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 16 | Out-Null
    Add-TextShape -Slide $slide -Left 350 -Top 150 -Width 562 -Height 108 -Text "Evidence Chain`nEach key claim in the report can be followed from test design to execution output, defect logging, and the linked GitHub issue when a failure was confirmed." -FontSize 15 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 16 | Out-Null
    $pipelineLabels = @('Scenario', 'Test Case', 'Execution', 'Evidence', 'Defect Log', 'GitHub Issue')
    $pipelineColors = @($colors.Accent, $colors.Teal, $colors.Warning, $colors.Success, $colors.Danger, $colors.Ink)
    for ($i = 0; $i -lt $pipelineLabels.Count; $i++) {
        $left = 350 + ($i * 88)
        Add-TextShape -Slide $slide -Left $left -Top 294 -Width 76 -Height 66 -Text $pipelineLabels[$i] -FontSize 11 -FontColor $colors.White -FontName 'Segoe UI' -Bold $true -Align 2 -FillColor $pipelineColors[$i] -Rounded -Margin 8 | Out-Null
        if ($i -lt ($pipelineLabels.Count - 1)) {
            $line = $slide.Shapes.AddLine($left + 76, 327, $left + 88, 327)
            $line.Line.ForeColor.RGB = $colors.Border
            $line.Line.Weight = 2
        }
    }
    Add-TextShape -Slide $slide -Left 48 -Top 364 -Width 864 -Height 98 -Text "Review point`nThe deck does not rely on summary labels alone. Each important result is backed by a visible runtime artifact and, when failing, by a tracked GitHub issue with severity and priority." -FontSize 14 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.AccentSoft2 -LineColor $colors.Accent -Rounded -Margin 16 | Out-Null

    # Slide 4
    $slide = $presentation.Slides.Add(4, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '03  Coverage Snapshot' -Title 'Coverage Is Complete, Ownership Is Clear' -Subtitle 'The case set is wide enough to cover the required scope and explicit enough to show that the UI workload was split without overlap.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    Add-MetricCard -Slide $slide -Left 48 -Top 148 -Width 198 -Height 86 -Value "$scenarioCount" -Label 'Documented scenarios' -AccentColor $colors.Accent -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-MetricCard -Slide $slide -Left 260 -Top 148 -Width 198 -Height 86 -Value "$totalCases" -Label 'Total test cases' -AccentColor $colors.Teal -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-MetricCard -Slide $slide -Left 472 -Top 148 -Width 198 -Height 86 -Value "44 UI / 19 API" -Label 'Design split' -AccentColor $colors.Warning -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-MetricCard -Slide $slide -Left 684 -Top 148 -Width 228 -Height 86 -Value '100% mapped' -Label 'Scenario-to-testcase coverage' -AccentColor $colors.Success -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-TextShape -Slide $slide -Left 48 -Top 264 -Width 864 -Height 210 -Text 'Owner Allocation' -FontSize 16 -FontColor $colors.Ink -FontName 'Bahnschrift SemiBold' -Bold $true | Out-Null
    $maxOwnerCount = ($ownerCounts | Measure-Object Count -Maximum).Maximum
    for ($i = 0; $i -lt $ownerCounts.Count; $i++) {
        $entry = $ownerCounts[$i]
        $barTop = 304 + ($i * 52)
        Add-TextShape -Slide $slide -Left 48 -Top ($barTop - 2) -Width 180 -Height 20 -Text $entry.Name -FontSize 12 -FontColor $colors.Ink -FontName 'Segoe UI' -Bold $true | Out-Null
        Add-Rectangle -Slide $slide -Left 232 -Top $barTop -Width 560 -Height 18 -FillColor $colors.AccentSoft2 -Rounded | Out-Null
        $barWidth = [math]::Round(560 * ($entry.Count / $maxOwnerCount), 0)
        Add-Rectangle -Slide $slide -Left 232 -Top $barTop -Width $barWidth -Height 18 -FillColor $colors.Accent -Rounded | Out-Null
        Add-TextShape -Slide $slide -Left 806 -Top ($barTop - 4) -Width 92 -Height 20 -Text "$($entry.Count) UI cases" -FontSize 11 -FontColor $colors.Slate -FontName 'Segoe UI' -Align 3 | Out-Null
    }
    Add-TextShape -Slide $slide -Left 48 -Top 464 -Width 864 -Height 34 -Text 'All three members stay above the 10-UI-case threshold required by the brief, and the current ownership split sums cleanly to 44 UI cases.' -FontSize 12 -FontColor $colors.Slate -FontName 'Segoe UI' | Out-Null

    # Slide 5
    $slide = $presentation.Slides.Add(5, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.DarkBg | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '04  Automation Implementation' -Title 'Automation Supports The Manual Baseline Instead Of Replacing It' -Subtitle 'The automation layer adds repeatable UI and API checks while final status still depends on recorded runtime behavior.' -AccentColor $colors.Teal -TitleColor $colors.White -SubtitleColor $colors.AccentSoft -SlideWidth $slideWidth
    Add-TextShape -Slide $slide -Left 48 -Top 150 -Width 262 -Height 184 -Text ".NET UI Suite`n`n- .NET 8, xUnit, Selenium`n- Page Object Model`n- shared settings, waits, and screenshot helper" -FontSize 14 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -LineColor $colors.Accent -Rounded -Margin 16 | Out-Null
    Add-TextShape -Slide $slide -Left 330 -Top 150 -Width 262 -Height 184 -Text "API Suite`n`n- Postman collection + Newman`n- text and XML runner artifacts`n- execution stored in submission package" -FontSize 14 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -LineColor $colors.Teal -Rounded -Margin 16 | Out-Null
    Add-TextShape -Slide $slide -Left 612 -Top 150 -Width 300 -Height 184 -Text "Demonstrated Flows`n`n- TC-UI-AUTH-001 on Edge`n- TC-UI-IMP-002 on Chrome`n- TC-API-HLT-001 in Newman`n- focused fail-case comparison in result view" -FontSize 14 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -LineColor $colors.Warning -Rounded -Margin 16 | Out-Null
    Add-TextShape -Slide $slide -Left 48 -Top 362 -Width 864 -Height 98 -Text "Execution principle`nAutomation is used as bonus evidence and repeatable smoke support. Final status still depends on runtime behavior, visible screenshots, runner outputs, and linked final-result records." -FontSize 14 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -LineColor $colors.Border -Rounded -Margin 16 | Out-Null

    # Slide 6
    $slide = $presentation.Slides.Add(6, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '05  Real Execution Evidence' -Title 'The Deck Uses Real Screenshots And Runner Artifacts' -Subtitle 'This slide proves that pass and fail statuses are backed by visible runtime output rather than summary claims.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    Add-PicturePanel -Slide $slide -ImagePath $imageAuth -Left 48 -Top 158 -Width 288 -Height 154 -Caption 'TC-UI-AUTH-001 pass on focused login smoke' -CaptionColor $colors.Slate -BorderColor $colors.Border -SurfaceColor $colors.White
    Add-PicturePanel -Slide $slide -ImagePath $imagePreview -Left 352 -Top 158 -Width 288 -Height 154 -Caption 'TC-UI-IMP-002 preview shows 6 / 1 / 5 counts' -CaptionColor $colors.Slate -BorderColor $colors.Border -SurfaceColor $colors.White
    Add-PicturePanel -Slide $slide -ImagePath $imageNewman -Left 48 -Top 326 -Width 592 -Height 164 -Caption 'Newman full run proves API regression passed 19 / 19' -CaptionColor $colors.Slate -BorderColor $colors.Border -SurfaceColor $colors.White
    Add-TextShape -Slide $slide -Left 660 -Top 158 -Width 252 -Height 332 -Text "Evidence rules`n`n- 63 / 63 cases executed`n- 0 cases remain Not Run`n- pass status requires runtime proof`n- fail status requires mismatch plus evidence`n`nFail evidence in scope`n- purchase validation banner defect`n- import confirm defect`n- invoice create defect`n- invoice cancel defect" -FontSize 13 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 16 | Out-Null

    # Slide 7
    $slide = $presentation.Slides.Add(7, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '06  Result Comparison' -Title 'Expected Result, Actual Result, Status, Then Evidence' -Subtitle 'This slide shows how each status in the final workbook was justified through runtime behavior rather than summary labels alone.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    Add-ComparisonCard -Slide $slide -Left 48 -Top 162 -Width 410 -Height 166 -CaseId 'TC-UI-AUTH-001' -Title 'Valid admin login reaches dashboard' -Expected 'Admin login must redirect to dashboard without validation error.' -Actual 'Chrome and Edge smoke reruns both reached the dashboard.' -Evidence 'edge-auth-smoke.trx' -Status 'Pass' -StatusColor $colors.Success -SurfaceColor $colors.White -BorderColor $colors.Border -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate
    Add-ComparisonCard -Slide $slide -Left 500 -Top 162 -Width 410 -Height 166 -CaseId 'TC-UI-IMP-002' -Title 'Import preview shows valid and invalid counts' -Expected 'Preview must show row counts and row-level validation info.' -Actual 'Preview showed 6 total, 1 valid, and 5 invalid rows.' -Evidence 'import-preview-rerun.trx' -Status 'Pass' -StatusColor $colors.Success -SurfaceColor $colors.White -BorderColor $colors.Border -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate
    Add-ComparisonCard -Slide $slide -Left 48 -Top 338 -Width 410 -Height 166 -CaseId 'TC-UI-PUR-002' -Title 'Readable supplier validation should be shown' -Expected 'Supplier-missing submission should show a readable error.' -Actual 'Submission was blocked, but the validation banner remained unreadable.' -Evidence 'Issue #2 + rerun TRX' -Status 'Fail' -StatusColor $colors.Danger -SurfaceColor $colors.White -BorderColor $colors.Border -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate
    Add-ComparisonCard -Slide $slide -Left 500 -Top 338 -Width 410 -Height 166 -CaseId 'TC-UI-INV-001' -Title 'Valid walk-in invoice should be created' -Expected 'A valid unpaid invoice should redirect to invoice details.' -Actual 'Create returned to the form with a failure toast and still reproduced.' -Evidence 'Issue #1 + rerun TRX' -Status 'Fail' -StatusColor $colors.Danger -SurfaceColor $colors.White -BorderColor $colors.Border -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate

    # Slide 8
    $slide = $presentation.Slides.Add(8, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '07  Metrics Snapshot' -Title 'Execution Is Complete, Stability Is Not Yet Complete' -Subtitle 'The metrics separate test coverage from product quality so the conclusion stays rigorous and evidence-based.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    Add-PicturePanel -Slide $slide -ImagePath $imageMetrics -Left 48 -Top 144 -Width 474 -Height 286 -Caption 'Metrics workbook snapshot as of 2026-04-11' -CaptionColor $colors.Slate -BorderColor $colors.Border -SurfaceColor $colors.White
    Add-MetricCard -Slide $slide -Left 546 -Top 144 -Width 170 -Height 82 -Value "$executedCases / $totalCases" -Label 'Executed cases' -AccentColor $colors.Accent -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-MetricCard -Slide $slide -Left 732 -Top 144 -Width 180 -Height 82 -Value "$passCases pass" -Label ("Pass rate " + ('{0:N2}%' -f $passRate)) -AccentColor $colors.Success -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-MetricCard -Slide $slide -Left 546 -Top 238 -Width 170 -Height 82 -Value "$failCases fail" -Label ("Fail rate " + ('{0:N2}%' -f $failRate)) -AccentColor $colors.Danger -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-MetricCard -Slide $slide -Left 732 -Top 238 -Width 180 -Height 82 -Value "$defectCount live issues" -Label 'Confirmed defects in GitHub' -AccentColor $colors.Warning -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.Border
    Add-TextShape -Slide $slide -Left 546 -Top 340 -Width 366 -Height 118 -Text "Interface view`n- Admin UI: $adminUiCount executed`n- API: $apiCount executed and fully passed`n- Public UI: $publicUiCount executed`n`nInterpretation`n100% execution proves coverage, not defect-free behavior." -FontSize 12 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 16 | Out-Null

    # Slide 9
    $slide = $presentation.Slides.Add(9, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '08  Defect Management' -Title 'Defects Are Logged, Reproduced, And Linked To Live Issues' -Subtitle 'Each confirmed defect is supported by reproducible evidence and a live GitHub issue with visible severity and priority labels.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    Add-PicturePanel -Slide $slide -ImagePath $imageIssue -Left 48 -Top 144 -Width 424 -Height 332 -Caption 'Representative GitHub issue with visible labels, severity, and priority' -CaptionColor $colors.Slate -BorderColor $colors.Border -SurfaceColor $colors.White
    $defectCards = @(
        @{ Top = 144; Record = ($defectRows | Where-Object 'Record ID' -eq 'BUG-20260406-001') },
        @{ Top = 224; Record = ($defectRows | Where-Object 'Record ID' -eq 'BUG-20260411-002') },
        @{ Top = 304; Record = ($defectRows | Where-Object 'Record ID' -eq 'BUG-20260411-003') },
        @{ Top = 384; Record = ($defectRows | Where-Object 'Record ID' -eq 'BUG-20260411-004') }
    )
    foreach ($item in $defectCards) {
        $record = $item.Record
        $issueNumber = Short-IssueNumber $record.'GitHub Issue'
        Add-TextShape -Slide $slide -Left 496 -Top $item.Top -Width 416 -Height 68 -Text "$($record.'Record ID')  $issueNumber`n$($record.Module)  |  Sev $($record.Severity)  |  Pri $($record.Priority)`n$($record.'Current Status')" -FontSize 12 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 14 | Out-Null
    }
    Add-TextShape -Slide $slide -Left 496 -Top 464 -Width 416 -Height 40 -Text 'All four confirmed defects were rerun on 2026-04-11 and still reproduced. The severity split remains 3 High and 1 Medium.' -FontSize 12 -FontColor $colors.Slate -FontName 'Segoe UI' | Out-Null

    # Slide 10
    $slide = $presentation.Slides.Add(10, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.DarkBg | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '09  Final Assessment' -Title 'What Is Proven Today, And What Still Needs Code Fixes' -Subtitle 'The final assessment is intentionally split between what the evidence proves now and what depends on future product changes.' -AccentColor $colors.Teal -TitleColor $colors.White -SubtitleColor $colors.AccentSoft -SlideWidth $slideWidth
    Add-TextShape -Slide $slide -Left 48 -Top 148 -Width 402 -Height 206 -Text "Proven today`n`n- clear linkage from scenario to issue tracker`n- 63 / 63 cases executed with 0 Not Run`n- automation demonstrates both pass and fail evidence`n- the submission package is complete and reviewable" -FontSize 15 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -LineColor $colors.Success -Rounded -Margin 18 | Out-Null
    Add-TextShape -Slide $slide -Left 500 -Top 148 -Width 412 -Height 206 -Text "Still open`n`n- 4 product defects remain unresolved`n- post-fix retest depends on code fixes`n- cross-browser proof is currently limited to the Edge smoke run`n- current execution shows stability risks in invoice and import flows" -FontSize 15 -FontColor $colors.White -FontName 'Segoe UI' -FillColor $colors.DarkSurface -LineColor $colors.Danger -Rounded -Margin 18 | Out-Null
    Add-MetricCard -Slide $slide -Left 48 -Top 382 -Width 272 -Height 88 -Value '1' -Label 'Fix invoice create and invoice cancel first' -AccentColor $colors.Danger -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White
    Add-MetricCard -Slide $slide -Left 344 -Top 382 -Width 272 -Height 88 -Value '2' -Label 'Fix purchase validation and import confirm' -AccentColor $colors.Warning -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White
    Add-MetricCard -Slide $slide -Left 640 -Top 382 -Width 272 -Height 88 -Value '3' -Label 'Rerun failed cases and attach post-fix evidence' -AccentColor $colors.Teal -SurfaceColor $colors.White -PrimaryTextColor $colors.Ink -SecondaryTextColor $colors.Slate -BorderColor $colors.White

    # Slide 11
    $slide = $presentation.Slides.Add(11, 12)
    Add-Rectangle -Slide $slide -Left 0 -Top 0 -Width $slideWidth -Height $slideHeight -FillColor $colors.Surface | Out-Null
    Add-SectionHeader -Slide $slide -StepLabel '10  Q&A Backup' -Title 'Short Answers For The Most Likely Review Questions' -Subtitle 'This backup slide exists to keep the live defense calm, concrete, and evidence-based.' -AccentColor $colors.Accent -TitleColor $colors.Ink -SubtitleColor $colors.Slate -SlideWidth $slideWidth
    $questions = @(
        'Why do 7 failed cases map to 4 defects?',
        'How do you distinguish automation failure from product defect?',
        'Why does 100% execution not mean a stable product?',
        'Which module is riskiest right now?',
        'What is the first post-fix retest priority?'
    )
    for ($i = 0; $i -lt $questions.Count; $i++) {
        $y = 146 + ($i * 64)
        Add-TextShape -Slide $slide -Left 48 -Top $y -Width 864 -Height 48 -Text ("Q" + ($i + 1) + ". " + $questions[$i]) -FontSize 15 -FontColor $colors.Ink -FontName 'Segoe UI' -FillColor $colors.White -LineColor $colors.Border -Rounded -Margin 16 | Out-Null
    }

    if (Test-Path $outputPath) {
        Remove-Item $outputPath -Force
    }
    $presentation.SaveAs($outputPath)

    Copy-Item $outputPath $workingPptx -Force
    Copy-Item $outputPath $cleanPptx -Force
}
finally {
    if ($presentation) {
        $presentation.Close()
    }
    if ($powerPoint) {
        $powerPoint.Quit()
    }
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}
