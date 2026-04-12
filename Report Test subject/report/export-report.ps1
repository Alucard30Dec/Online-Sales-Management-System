param(
    [string]$SourcePath = "E:\Project\Online-Sales-Management-System\Report Test subject\Powered by GPT\MainReport\SourceAssets\Powered by GPT - Software Quality Verification - Final Report Content.md"
)

$ErrorActionPreference = "Stop"

$reportRoot = "E:\Project\Online-Sales-Management-System\Report Test subject"
$reportFolder = Join-Path $reportRoot "report"
$rootDocxPath = Join-Path $reportRoot "Powered by GPT - Software Quality Verification.docx"
$rootPdfPath = Join-Path $reportRoot "Powered by GPT - Software Quality Verification.pdf"
$workingDocxPath = Join-Path $reportRoot "Powered by GPT\MainReport\Powered by GPT - Software Quality Verification.docx"
$workingPdfPath = Join-Path $reportRoot "Powered by GPT\MainReport\Powered by GPT - Software Quality Verification.pdf"
$reportDocxPath = Join-Path $reportFolder "Powered by GPT - Software Quality Verification - Final Report.docx"
$reportPdfPath = Join-Path $reportFolder "Powered by GPT - Software Quality Verification - Final Report.pdf"
$cleanDocxPath = Join-Path $reportRoot "SV00123-ATU-A01\MainReport\Powered by GPT - Software Quality Verification - Final Report.docx"
$cleanPdfPath = Join-Path $reportRoot "SV00123-ATU-A01\MainReport\Powered by GPT - Software Quality Verification - Final Report.pdf"
$generatorScriptPath = Join-Path $reportFolder "generate_report_docx.py"
$renderScriptPath = Join-Path $reportFolder "render-report-html.py"
$edgePath = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
$tempPdfPath = Join-Path $reportFolder "temp-final-report-export.pdf"
$htmlPath = Join-Path $reportFolder "temp-final-report.html"
$reportContentCopyPath = Join-Path $reportFolder "Powered by GPT - Software Quality Verification - Final Report Content.md"

try {
    $resolvedSource = (Resolve-Path $SourcePath).Path
    Copy-Item $resolvedSource $reportContentCopyPath -Force

    & py -3 $generatorScriptPath --source $resolvedSource --output-docx $reportDocxPath
    if ($LASTEXITCODE -ne 0) {
        throw "DOCX generation step failed."
    }

    & py -3 $renderScriptPath --source $resolvedSource --output-html $htmlPath
    if ($LASTEXITCODE -ne 0) {
        throw "HTML render step failed."
    }

    if (Test-Path $tempPdfPath) {
        Remove-Item $tempPdfPath -Force
    }

    & $edgePath --headless --disable-gpu --disable-web-security --allow-file-access-from-files --run-all-compositor-stages-before-draw --virtual-time-budget=10000 "--print-to-pdf=$tempPdfPath" --print-to-pdf-no-header "file:///$($htmlPath -replace '\\','/')"

    for ($i = 0; $i -lt 30; $i++) {
        if (Test-Path $tempPdfPath) {
            break
        }
        Start-Sleep -Seconds 1
    }

    if (-not (Test-Path $tempPdfPath)) {
        throw "PDF export step failed."
    }

    Copy-Item $tempPdfPath $reportPdfPath -Force
}
finally {
    if (Test-Path $tempPdfPath) {
        Remove-Item $tempPdfPath -Force
    }
    if (Test-Path $htmlPath) {
        Remove-Item $htmlPath -Force
    }

    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}

Copy-Item $reportDocxPath $rootDocxPath -Force
Copy-Item $reportDocxPath $workingDocxPath -Force
Copy-Item $reportDocxPath $cleanDocxPath -Force
Copy-Item $reportPdfPath $rootPdfPath -Force
Copy-Item $reportPdfPath $workingPdfPath -Force
Copy-Item $reportPdfPath $cleanPdfPath -Force
