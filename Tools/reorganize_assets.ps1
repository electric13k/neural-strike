<#
.SYNOPSIS
  Reorganizes Assets/Final Models into:
    Assets/Models      (.obj files, renamed to botname.obj)
    Assets/Textures    (.mtl, .png, .jpg, .jpeg, renamed to botname_type.ext)
    Assets/Animations  (.fbx files, renamed to botname_animname.fbx)

  Then patches each .mtl and .obj to reference the new texture filenames.
  Also patches .obj mtllib lines to point to the renamed .mtl.

  Run from the repo root:
    powershell -ExecutionPolicy Bypass -File Tools/reorganize_assets.ps1
#>

$AssetsRoot  = Join-Path $PSScriptRoot "..\Assets"
$SourceRoot  = Join-Path $AssetsRoot "Final Models"
$ModelsOut   = Join-Path $AssetsRoot "Models"
$TexturesOut = Join-Path $AssetsRoot "Textures"
$AnimOut     = Join-Path $AssetsRoot "Animations"

foreach ($d in @($ModelsOut, $TexturesOut, $AnimOut)) {
    if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d | Out-Null }
}

# ── Bot name map: folder name -> canonical short name ────────────────
$BotNames = @{
    "courier bot"                        = "courierbot"
    "Sniper bot"                         = "sniperbot"
    "Medic bot"                          = "medicbot"
    "spy bot"                            = "spybot"
    "Assault+robot+final+orange+obj"     = "assaultbot"
    "Player"                             = "player"
    "weapons and items"                  = "weapons"
}

# ── Texture suffix map: keyword in original filename -> canonical suffix
$TexSuffixes = @(
    @{ key = "basecolor";  out = "basecolor" }
    @{ key = "metallic";   out = "metallic"  }
    @{ key = "normal";     out = "normal"    }
    @{ key = "roughness";  out = "roughness" }
    @{ key = "albedo";     out = "albedo"    }
    @{ key = "ao";         out = "ao"        }
    @{ key = "emission";   out = "emission"  }
    @{ key = "height";     out = "height"    }
    @{ key = "opacity";    out = "opacity"   }
)

function Get-TexSuffix($filename) {
    $low = $filename.ToLower()
    foreach ($s in $TexSuffixes) {
        if ($low -match $s.key) { return $s.out }
    }
    return "diffuse"
}

# Tracks renames for patching: oldBaseName -> newBaseName (no ext)
$TexRenameMap = @{}
$MtlRenameMap = @{}
$ObjRenameMap = @{}

foreach ($botFolder in (Get-ChildItem $SourceRoot -Directory)) {
    $botKey  = $botFolder.Name
    $botName = if ($BotNames.ContainsKey($botKey)) { $BotNames[$botKey] } else { ($botKey -replace '[^a-zA-Z0-9]','').ToLower() }

    # Recurse into sub-folders too (e.g. courier bot/courier bot/)
    $allFiles = Get-ChildItem $botFolder.FullName -Recurse -File

    foreach ($f in $allFiles) {
        $ext = $f.Extension.ToLower()

        switch ($ext) {
            { $_ -in '.png','.jpg','.jpeg' } {
                $suffix  = Get-TexSuffix $f.BaseName
                $newName = "${botName}_${suffix}$ext"
                $dst     = Join-Path $TexturesOut $newName
                Copy-Item $f.FullName $dst -Force
                $TexRenameMap[$f.Name] = $newName
                Write-Host "[TEX]  $($f.Name) -> $newName"
            }
            '.mtl' {
                $newName = "${botName}.mtl"
                $dst     = Join-Path $TexturesOut $newName
                Copy-Item $f.FullName $dst -Force
                $MtlRenameMap[$f.Name] = $newName
                Write-Host "[MTL]  $($f.Name) -> $newName"
            }
            '.obj' {
                $newName = "${botName}.obj"
                $dst     = Join-Path $ModelsOut $newName
                Copy-Item $f.FullName $dst -Force
                $ObjRenameMap[$f.Name] = $newName
                Write-Host "[OBJ]  $($f.Name) -> $newName"
            }
            '.fbx' {
                # Derive animation name from FBX filename, strip bot name duplication
                $animRaw = $f.BaseName -replace [regex]::Escape($botKey),''
                $animRaw = $animRaw -replace [regex]::Escape($botName),''
                $animRaw = $animRaw.Trim(' -_').ToLower() -replace '[^a-z0-9]','_'
                $animRaw = $animRaw.Trim('_')
                if ($animRaw -eq '') { $animRaw = 'idle' }
                $newName = "${botName}_${animRaw}.fbx"
                $dst     = Join-Path $AnimOut $newName
                Copy-Item $f.FullName $dst -Force
                Write-Host "[FBX]  $($f.Name) -> $newName"
            }
        }
    }
}

# ── Patch .mtl files: update texture references to new names ─────────
Write-Host "`n[PATCH] Patching .mtl texture references..."
foreach ($mtl in (Get-ChildItem $TexturesOut -Filter "*.mtl")) {
    $content = Get-Content $mtl.FullName -Raw
    foreach ($old in $TexRenameMap.Keys) {
        $new = $TexRenameMap[$old]
        $content = $content -replace [regex]::Escape($old), $new
    }
    Set-Content $mtl.FullName $content -NoNewline
    Write-Host "  Patched: $($mtl.Name)"
}

# ── Patch .obj files: update mtllib line to new .mtl name ─────────────
Write-Host "`n[PATCH] Patching .obj mtllib references..."
foreach ($obj in (Get-ChildItem $ModelsOut -Filter "*.obj")) {
    $content = Get-Content $obj.FullName -Raw
    foreach ($old in $MtlRenameMap.Keys) {
        $new = $MtlRenameMap[$old]
        $content = $content -replace [regex]::Escape($old), $new
    }
    Set-Content $obj.FullName $content -NoNewline
    Write-Host "  Patched: $($obj.Name)"
}

Write-Host "`n[DONE] Reorganization complete."
Write-Host "  Models    -> Assets/Models"
Write-Host "  Textures  -> Assets/Textures"
Write-Host "  Animations-> Assets/Animations"
Write-Host "`nNow in Unity: Tools > Neural Strike > 2) Build FFA Scene"
