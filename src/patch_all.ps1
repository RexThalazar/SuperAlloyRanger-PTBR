$cecil = [System.Reflection.Assembly]::LoadFrom('X:\jogos\epic\SuperAlloyRangerKIpC3\Super.Alloy.Ranger_Data\Managed\Unity.Burst.Cecil.dll')

$managedDir = 'X:\jogos\epic\SuperAlloyRangerKIpC3\Super.Alloy.Ranger_Data\Managed'
$origDll = Join-Path $managedDir 'Assembly-CSharp.dll.orig'
$targetDll = 'C:\Users\Administrador\.gemini\antigravity\scratch\Assembly-CSharp.patched.dll'
$sarDll = 'C:\Users\Administrador\.gemini\antigravity\scratch\SARTranslator.dll'

$resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
$resolver.AddSearchDirectory($managedDir)
$resolver.AddSearchDirectory('C:\Users\Administrador\.gemini\antigravity\scratch')

$readerParams = New-Object Mono.Cecil.ReaderParameters
$readerParams.AssemblyResolver = $resolver
$readerParams.ReadWrite = $false

$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($origDll, $readerParams)

# 1. Import SARTranslator references
$sarAsm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($sarDll, $readerParams)
$modType = $sarAsm.MainModule.GetType('SARTranslator.Mod')

$translateMethod = $null
$translateBtnMethod = $null
$translateLangMethod = $null
foreach ($m in $modType.Methods) {
    if ($m.Name -eq 'Translate' -and $m.Parameters.Count -eq 1) {
        $translateMethod = $asm.MainModule.ImportReference($m)
    }
    if ($m.Name -eq 'TranslateButtonTip' -and $m.Parameters.Count -eq 1) {
        $translateBtnMethod = $asm.MainModule.ImportReference($m)
    }
    if ($m.Name -eq 'TranslateLanguageUI' -and $m.Parameters.Count -eq 1) {
        $translateLangMethod = $asm.MainModule.ImportReference($m)
    }
}

Write-Output "Translate method imported: $($translateMethod -ne $null)"
Write-Output "TranslateBtn method imported: $($translateBtnMethod -ne $null)"
Write-Output "TranslateLang method imported: $($translateLangMethod -ne $null)"

# Find Component::get_transform
$getTransformMethod = $null
foreach ($type in $asm.MainModule.Types) {
    foreach ($m in $type.Methods) {
        if ($m.HasBody) {
            foreach ($inst in $m.Body.Instructions) {
                if ($inst.Operand -and $inst.Operand.ToString() -eq 'UnityEngine.Transform UnityEngine.Component::get_transform()') {
                    $getTransformMethod = $inst.Operand
                    break
                }
            }
        }
        if ($getTransformMethod) { break }
    }
    if ($getTransformMethod) { break }
}
Write-Output "get_transform method found: $($getTransformMethod -ne $null)"

# Helper function to inject TranslateLanguageUI before void method return
function InjectTranslateLanguageUI($method, $ilProcessor) {
    $lastInst = $method.Body.Instructions[$method.Body.Instructions.Count - 1]
    if ($lastInst.OpCode.Name -eq 'ret') {
        $ldarg0 = $ilProcessor.Create([Mono.Cecil.Cil.OpCodes]::Ldarg_0)
        $callGetTrans = $ilProcessor.Create([Mono.Cecil.Cil.OpCodes]::Call, $getTransformMethod)
        $callTransLang = $ilProcessor.Create([Mono.Cecil.Cil.OpCodes]::Call, $translateLangMethod)
        $ilProcessor.InsertBefore($lastInst, $ldarg0)
        $ilProcessor.InsertBefore($lastInst, $callGetTrans)
        $ilProcessor.InsertBefore($lastInst, $callTransLang)
        Write-Output "Injected TranslateLanguageUI into $($method.DeclaringType.FullName)::$($method.Name)"
    }
}

# 2. Patch smx.ResMgrBase::LoadText
$resMgr = $asm.MainModule.GetType('smx.ResMgrBase')
foreach ($m in $resMgr.Methods) {
    if ($m.Name -eq 'LoadText' -and $m.Parameters.Count -eq 2) {
        $il = $m.Body.GetILProcessor()
        $lastInst = $m.Body.Instructions[$m.Body.Instructions.Count - 1]
        if ($lastInst.OpCode.Name -eq 'ret') {
            $callInst = $il.Create([Mono.Cecil.Cil.OpCodes]::Call, $translateMethod)
            $il.InsertBefore($lastInst, $callInst)
            Write-Output "Patched smx.ResMgrBase::LoadText"
        }
    }
}

# 3. Patch InputTips::LoadButton & InitButtions
$it = $asm.MainModule.GetType('InputTips')
foreach ($m in $it.Methods) {
    if ($m.Name -eq 'LoadButton') {
        $il = $m.Body.GetILProcessor()
        $lastInst = $m.Body.Instructions[$m.Body.Instructions.Count - 1]
        if ($lastInst.OpCode.Name -eq 'ret') {
            $dupInst = $il.Create([Mono.Cecil.Cil.OpCodes]::Dup)
            $callBtnInst = $il.Create([Mono.Cecil.Cil.OpCodes]::Call, $translateBtnMethod)
            $il.InsertBefore($lastInst, $dupInst)
            $il.InsertBefore($lastInst, $callBtnInst)
            Write-Output "Patched InputTips::LoadButton"
        }
    }
    if ($m.Name -eq 'InitButtions') {
        $il = $m.Body.GetILProcessor()
        for ($i = 0; $i -lt $m.Body.Instructions.Count; $i++) {
            $inst = $m.Body.Instructions[$i]
            if ($inst.OpCode.Name -eq 'stloc.1') {
                $ldlocInst = $il.Create([Mono.Cecil.Cil.OpCodes]::Ldloc_1)
                $callBtnInst = $il.Create([Mono.Cecil.Cil.OpCodes]::Call, $translateBtnMethod)
                $il.InsertAfter($inst, $callBtnInst)
                $il.InsertAfter($inst, $ldlocInst)
                Write-Output "Patched InputTips::InitButtions"
                break
            }
        }
    }
}

# 4. Patch LanguagePage::OnStartObject
$lp = $asm.MainModule.GetType('LanguagePage')
foreach ($m in $lp.Methods) {
    if ($m.Name -eq 'OnStartObject') {
        $il = $m.Body.GetILProcessor()
        InjectTranslateLanguageUI $m $il
    }
}

# 5. Patch smx.SelectMenu::InitSelectItems
$sm = $asm.MainModule.GetType('smx.SelectMenu')
foreach ($m in $sm.Methods) {
    if ($m.Name -eq 'InitSelectItems') {
        $il = $m.Body.GetILProcessor()
        InjectTranslateLanguageUI $m $il
    }
}

# 6. Patch LangueageBox::Awake & SelectLag(Int32)
$lb = $asm.MainModule.GetType('LangueageBox')
foreach ($m in $lb.Methods) {
    if ($m.Name -eq 'Awake') {
        $il = $m.Body.GetILProcessor()
        InjectTranslateLanguageUI $m $il
    }
    if ($m.Name -eq 'SelectLag' -and $m.Parameters.Count -eq 1 -and $m.Parameters[0].ParameterType.FullName -eq 'System.Int32') {
        $il = $m.Body.GetILProcessor()
        InjectTranslateLanguageUI $m $il
    }
}

# 7. Patch OptionPage::OnStartObject
$op = $asm.MainModule.GetType('OptionPage')
foreach ($m in $op.Methods) {
    if ($m.Name -eq 'OnStartObject') {
        $il = $m.Body.GetILProcessor()
        InjectTranslateLanguageUI $m $il
    }
}

$writerParams = New-Object Mono.Cecil.WriterParameters
$asm.Write($targetDll, $writerParams)
Write-Output "Successfully wrote $targetDll"
