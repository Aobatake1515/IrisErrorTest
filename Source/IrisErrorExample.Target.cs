// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.Collections.Generic;

public class IrisErrorExampleTarget : TargetRules
{
	public IrisErrorExampleTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;
		bWithPushModel = true;
		bUseIris = true;
		DefaultBuildSettings = BuildSettingsVersion.V5;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_5;
		ExtraModuleNames.Add("IrisErrorExample");
	}
}
