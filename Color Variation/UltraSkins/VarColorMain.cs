using System.Collections.Generic;
using System.IO;
using BepInEx;
using UMM;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WeaponColVariations
{
	[UKPlugin("Variation Colors Test", "Wip", "allows you to use the weapon's variation color as a skin color", true, true)]
	public class VarColorMain : UKMod
	{

		Harmony harmony;

		public override void OnModLoaded()
		{
			harmony = new Harmony("WeaponVarationColors");
			harmony.PatchAll(typeof(VarColorMain.WeaponSkinColorVariation));
			harmony.PatchAll();
		}

		public override void OnModUnload()
		{
			harmony.UnpatchSelf();
		}

		private void Start()
		{
			SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		}

		private void SceneManagerOnsceneLoaded(Scene scene, LoadSceneMode mode)
		{
			CreateSkinGUI();
		}

		public void CreateSkinGUI()
		{
			foreach (ShopGearChecker shopGearChecker in Resources.FindObjectsOfTypeAll<ShopGearChecker>())
			{
				GunColorTypeGetter[] weapons = shopGearChecker.GetComponentsInChildren<GunColorTypeGetter>(true);
				foreach (GunColorTypeGetter weapon in weapons)
				{
					GunColorSetter[] Colors = weapon.GetComponentsInChildren<GunColorSetter>(true);
					foreach (GunColorSetter GCS in Colors)
					{
						GameObject buttonBase = GCS.transform.parent.parent.Find("TemplateButton").gameObject;
						GameObject varButton = Instantiate(buttonBase, GCS.transform);
						varButton.name = "VarColButton";
						Destroy(varButton.GetComponent<Button>());
						varButton.transform.localPosition = new Vector3(-35, -70, -15);
						varButton.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
						ShopButton SB = varButton.GetComponent<ShopButton>();
						GameObject Activator = Instantiate(new GameObject(), varButton.transform);
						Activator.SetActive(false);
						SB.toActivate = new GameObject[] { Activator };
						SB.toDeactivate = new GameObject[0];
						ShopVariationButton SVB = varButton.AddComponent<ShopVariationButton>();
						SVB.Activator = Activator;
						SVB.VCM = transform.GetComponent<VarColorMain>();
						SVB.ColorNum = GCS.colorNumber;
						SVB.weaponNum = GCS.transform.parent.parent.parent.GetComponent<GunColorTypeGetter>().weaponNumber;
						SVB.Alt = GCS.transform.parent.parent.parent.GetComponent<GunColorTypeGetter>().altVersion;
						SVB.GunColSetter = GCS;
						varButton.GetComponentInChildren<Text>().text = "USE VAR COLOR";
						varButton.GetComponentInChildren<Text>().transform.localPosition = new Vector3(55, -20, 0);
						varButton.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);
						GameObject CheckBox = Instantiate(buttonBase, varButton.transform);
						Destroy(CheckBox.GetComponent<ShopButton>());
						Destroy(CheckBox.GetComponentInChildren<Text>().transform.gameObject);
						CheckBox.transform.localScale = new Vector3(1, 1, 1);
						CheckBox.transform.localPosition = new Vector3(165, 0, 0);
						CheckBox.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 25);
						SVB.CheckBox = CheckBox;
					}
				}
			}
		}

		[HarmonyPatch(typeof(GunColorGetter), "UpdateColor")]
		public static class WeaponSkinColorVariation
        {
            [HarmonyPostfix]
			public static void PostFix(GunColorGetter __instance)
            {
				WeaponIcon wepico = __instance.GetComponentInParent<WeaponIcon>();
				Material[] materials =  __instance.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
				foreach(Material mat in materials)
                {
					if (!__instance.transform.gameObject.GetComponent<VariationColorHandler>())
					{
						__instance.transform.gameObject.AddComponent<VariationColorHandler>();
						VarColorMain[] guh = Resources.FindObjectsOfTypeAll<VarColorMain>();
						__instance.transform.gameObject.GetComponent<VariationColorHandler>().VCM = guh[0];
					}
					if (wepico && __instance.transform.GetComponent<VariationColorHandler>())
					{
						VariationColorHandler VCH = __instance.transform.gameObject.GetComponent<VariationColorHandler>();
						VCH.weaponnum = __instance.weaponNumber;
						VCH.alt = __instance.altVersion;
						Color varColor = new Color(MonoSingleton<ColorBlindSettings>.Instance.variationColors[wepico.variationColor].r,
							MonoSingleton<ColorBlindSettings>.Instance.variationColors[wepico.variationColor].g,
							MonoSingleton<ColorBlindSettings>.Instance.variationColors[wepico.variationColor].b, 1f);
						if (mat.HasProperty("_CustomColor1"))
						{
							if(VCH.swapColor1)
								mat.SetColor("_CustomColor1", varColor);
							if (VCH.swapColor2)
								mat.SetColor("_CustomColor2", varColor);
							if (VCH.swapColor3)
								mat.SetColor("_CustomColor3", varColor);
						}
					}
				}
            }
        }
	}
}
