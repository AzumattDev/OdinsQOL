﻿using HarmonyLib;
using System.Diagnostics;

namespace VMP_Mod.Patches
{
   

		//[HarmonyPatch(typeof(WearNTear), "HaveRoof")]
		//public static class RemoveWearNTear
		//{
		//	private static void Postfix(ref bool __result)
		//	{
		//		if (Configuration.Current.Building.noWeatherDamage)
		//		{
		//			__result = true;
		//		}
		//	}
		//}

		///// <summary>
		///// Disable weather damage under water
		///// </summary>
		//[HarmonyPatch(typeof(WearNTear), "IsUnderWater")]
		//public static class RemoveWearNTearFromUnderWater
		//{
		//	private static void Postfix(ref bool __result)
		//	{
		//		if (Configuration.Current.Building.noWeatherDamage)
		//		{
		//			__result = false;
		//		}
		//	}
		//}

		///// <summary>
		///// Removes the integrity check for having a connected piece to the ground.
		///// </summary>
		//[HarmonyPatch(typeof(WearNTear), "HaveSupport")]
		//public static class WearNTear_HaveSupport_Patch
		//{
		//	private static void Postfix(ref bool __result)
		//	{
		//		if (Configuration.Current.StructuralIntegrity.disableStructuralIntegrity)
		//		{
		//			__result = true;
		//		}
		//	}
		//}

		///// <summary>
		///// Disable damage to player structures
		///// </summary>
		//[HarmonyPatch(typeof(WearNTear), "ApplyDamage")]
		//public static class WearNTear_ApplyDamage_Patch
		//{
		//	private static bool Prefix(ref WearNTear __instance, ref float damage)
		//	{
		//		// Gets the name of the method calling the ApplyDamage method
		//		StackTrace stackTrace = new StackTrace();
		//		string callingMethod = stackTrace.GetFrame(2).GetMethod().Name;

		//		if ( __instance.m_piece && __instance.m_piece.IsPlacedByPlayer() && callingMethod != "UpdateWear"))
		//			return true;

		//		if (__instance.m_piece.m_name.StartsWith("$ship"))
		//		{
		//			if (Configuration.Current.StructuralIntegrity.disableDamageToPlayerBoats ||
		//				(Configuration.Current.StructuralIntegrity.disableWaterDamageToPlayerBoats &&
		//				 stackTrace.GetFrame(15).GetMethod().Name == "UpdateWaterForce")) return false;

		//			return true;
		//		}

		//		return !Configuration.Current.StructuralIntegrity.disableDamageToPlayerStructures;
		//	}
		//}

		///// <summary>
		///// Disable structural integrity
		///// </summary>
		//[HarmonyPatch(typeof(WearNTear), "GetMaterialProperties")]
		//public static class RemoveStructualIntegrity
		//{
		//	private static bool Prefix(ref WearNTear __instance, out float maxSupport, out float minSupport, out float horizontalLoss, out float verticalLoss)
		//	{
		//		if (Configuration.Current.StructuralIntegrity.disableStructuralIntegrity)
		//		{
		//			maxSupport = 1500f;
		//			minSupport = 20f;
		//			verticalLoss = 0f;
		//			horizontalLoss = 0f;
		//			return false;
		//		}
		//			// This handling is choosen because we subtract from a value thats reduced by distance from ground contact.
		//			Configuration.Current.StructuralIntegrity.wood = (Configuration.Current.StructuralIntegrity.wood >= 100 ? 100 : Configuration.Current.StructuralIntegrity.wood);
		//			Configuration.Current.StructuralIntegrity.stone = (Configuration.Current.StructuralIntegrity.wood >= 100 ? 100 : Configuration.Current.StructuralIntegrity.stone);
		//			Configuration.Current.StructuralIntegrity.iron = (Configuration.Current.StructuralIntegrity.wood >= 100 ? 100 : Configuration.Current.StructuralIntegrity.iron);
		//			Configuration.Current.StructuralIntegrity.hardWood = (Configuration.Current.StructuralIntegrity.wood >= 100 ? 100 : Configuration.Current.StructuralIntegrity.hardWood);

		//			switch (__instance.m_materialType)
		//			{
		//				case WearNTear.MaterialType.Wood:
		//					maxSupport = 100f;
		//					minSupport = 10f;
		//					verticalLoss = 0.125f - ((0.125f / 100) * Configuration.Current.StructuralIntegrity.wood);
		//					horizontalLoss = 0.2f - ((0.125f / 100) * Configuration.Current.StructuralIntegrity.wood);
		//					return false;
		//				case WearNTear.MaterialType.Stone:
		//					maxSupport = 1000f;
		//					minSupport = 100f;
		//					verticalLoss = 0.125f - ((0.125f / 100) * Configuration.Current.StructuralIntegrity.stone);
		//					horizontalLoss = 1f - ((1f / 100) * Configuration.Current.StructuralIntegrity.stone);
		//					return false;
		//				case WearNTear.MaterialType.Iron:
		//					maxSupport = 1500f;
		//					minSupport = 20f;
		//					verticalLoss = 0.07692308f - ((0.07692308f / 100) * Configuration.Current.StructuralIntegrity.iron);
		//					horizontalLoss = 0.07692308f - ((0.07692308f / 100) * Configuration.Current.StructuralIntegrity.iron);
		//					return false;
		//				case WearNTear.MaterialType.HardWood:
		//					maxSupport = 140f;
		//					minSupport = 10f;
		//					verticalLoss = 0.1f - ((0.1f / 100) * Configuration.Current.StructuralIntegrity.hardWood);
		//					horizontalLoss = 0.16666667f - ((0.16666667f / 100) * Configuration.Current.StructuralIntegrity.hardWood);
		//					return false;
		//				default:
		//					maxSupport = 0f;
		//					minSupport = 0f;
		//					verticalLoss = 0f;
		//					horizontalLoss = 0f;
		//					return false;
		//			}

		//	}
		//}

	
}
