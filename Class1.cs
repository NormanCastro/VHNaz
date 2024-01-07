using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;




namespace NewEffects
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInProcess("valheim.exe")]
    public class NewEffects : BaseUnityPlugin

    {
        // Custom status effect
        private AssetBundle TestAssets;
        private AssetBundle SteelIngotBundle;
        private AssetBundle EmbeddedResourceBundle;

        private ConfigEntry<string> configGreeting;
        private ConfigEntry<bool> configDisplayGreeting;
        private const string ModName = "NewEffects";
        private const string ModVersion = "0.0.1";
        private const string ModGUID = "org.bepinex.plugins.TutProject";
        private readonly Harmony harmony = new Harmony(ModGUID);

        private static SE_Stats holySwordEffect = ScriptableObject.CreateInstance<SE_Stats>();
        private static SE_Stats moreHpEffect = ScriptableObject.CreateInstance<SE_Stats>();
        private static SE_Stats lightArmorEffect = ScriptableObject.CreateInstance<SE_Stats>();
        private static StatusEffect testEffect = ScriptableObject.CreateInstance<StatusEffect>();
        private static DateTime coolDownDragon;
        public static StringBuilder m_stringBuilder = new StringBuilder(256);


        public static List<BaseAI> Instances { get; } = new List<BaseAI>();

        private void Awake()
        {
            configGreeting = Config.Bind("General", // The section under which the option is shown
                "GreetingText", // The key of the configuration option in the configuration file
                "TestingMod Cargado 2!", // The default value
                "A greeting text to show when the game is launched"); // Description of the option to show in the config file

            configDisplayGreeting = Config.Bind("General.Toggles",
                "DisplayGreeting",
                true,
                "Whether or not to show the greeting text");
            // Test code
            Logger.LogInfo("Hello, new effects mods 4!");

            AddStatusEffects();
            LoadAssets();
            AddCustomItemConversions();


            // Add custom items cloned from vanilla items
            PrefabManager.OnVanillaPrefabsAvailable += AddClonedItems;

            harmony.PatchAll();

        }


        // Add new status effects
        public void AddStatusEffects()
        {
            holySwordEffect.name = "Holy Sword";
            holySwordEffect.m_name = "Aggro";
            holySwordEffect.m_tooltip = "Una espada santa con propiedades misteriosas";
            holySwordEffect.m_startMessage = "Un poder divino te posee";
            holySwordEffect.m_skillLevel = Skills.SkillType.Swords;
            holySwordEffect.m_skillLevelModifier = 15;
            holySwordEffect.m_icon =
                AssetUtils.LoadSpriteFromFile("C:\\Users\\Norman\\RiderProjects\\NewEffects\\Assets\\reee.png");

            lightArmorEffect.m_ttl = 30;
            lightArmorEffect.m_time = 10;


            moreHpEffect.name = "NAZ_TANK_DragonRage";
            moreHpEffect.m_name = "Dragon Rage";
            moreHpEffect.m_tooltip = "Has atraido la atención de los enemigos";
            moreHpEffect.m_startMessage = "Dragon Rage : Activado";
            moreHpEffect.m_icon =
                AssetUtils.LoadSpriteFromFile("C:\\Users\\Norman\\RiderProjects\\NewEffects\\Assets\\shieldicon.png");
            moreHpEffect.m_startMessageType = MessageHud.MessageType.Center;
            moreHpEffect.m_ttl = 20;
            moreHpEffect.m_time = 5;


            lightArmorEffect.name = "Viento Ligero";
            lightArmorEffect.m_name = "Viento Ligero";
            lightArmorEffect.m_tooltip = "Una pechera más resistente";
            lightArmorEffect.m_startMessage = "Siente un aire en tus pies";
            lightArmorEffect.m_skillLevel = Skills.SkillType.Run;
            lightArmorEffect.m_skillLevelModifier = 5;
            lightArmorEffect.m_icon =
                AssetUtils.LoadSpriteFromFile(
                    "C:\\Users\\Norman\\RiderProjects\\NewEffects\\Assets\\lightArmorIcon.png");

            //Estos 2 de abajo son el cooldown xD ( m_ttl - m_time)
            lightArmorEffect.m_ttl = 30;
            lightArmorEffect.m_time = 10;


            //EvilSwordEffect = new CustomStatusEffect(holySword, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            //ItemManager.Instance.AddStatusEffect(EvilSwordEffect);
        }

        // Implementation of cloned items
        public void AddClonedItems()
        {
            // Create a custom resource based on Wood
            ItemConfig customWoodConfig = new ItemConfig();
            customWoodConfig.Name = "Madera Rara";
            customWoodConfig.Description = "Madera clonada de la tipica madera";
            customWoodConfig.AddRequirement(new RequirementConfig("Wood", 1));
            CustomItem recipeComponent = new CustomItem("CustomWood", "Wood", customWoodConfig);
            ItemManager.Instance.AddItem(recipeComponent);

            // Create and add a custom item based on SwordBlackmetal
            ItemConfig holySwordConfig = new ItemConfig();
            holySwordConfig.Name = "Espada Divina";
            holySwordConfig.Description = "Espada bendecida que atrae la oscuridad";
            holySwordConfig.CraftingStation = "piece_workbench";
            holySwordConfig.AddRequirement(new RequirementConfig("Stone", 1));
            holySwordConfig.AddRequirement(new RequirementConfig("Wood", 1));
            CustomItem holySword = new CustomItem("HolySword", "SwordSilver", holySwordConfig);


            // Create and add a custom item based on SwordBlackmetal
            ItemConfig lightArmorConfig = new ItemConfig();
            lightArmorConfig.Name = "Armadura Ligera";
            lightArmorConfig.Description = "Armadura ligera levemente mejorada";
            lightArmorConfig.CraftingStation = "piece_workbench";
            lightArmorConfig.AddRequirement(new RequirementConfig("DeerHide", 5));
            lightArmorConfig.AddRequirement(new RequirementConfig("LeatherScraps", 5));
            lightArmorConfig.AddRequirement(new RequirementConfig("Resin", 3));
            lightArmorConfig.MinStationLevel = 2;



            CustomItem lightArmor = new CustomItem("ArmaduraLigera", "ArmorLeatherChest", lightArmorConfig);


            ItemManager.Instance.AddItem(holySword);
            ItemManager.Instance.AddItem(lightArmor);
            ;
            // Add our custom status effect to it
            holySword.ItemDrop.m_itemData.m_shared.m_equipStatusEffect = moreHpEffect;
            holySword.ItemDrop.m_itemData.m_shared.m_movementModifier = 1f;
            
            //lightArmor.ItemDrop.m_itemData.m_shared.m_equipStatusEffect = lightArmorEffect;
            //lightArmor.ItemDrop.m_itemData.m_shared.m_movementModifier = 1f;
            //lightArmor.ItemDrop.m_itemData.m_shared.m_equipStatusEffect = moreHpEffect;


            // You want that to run only once, Jotunn has the item cached for the game session
            PrefabManager.OnVanillaPrefabsAvailable -= AddClonedItems;
        }

        // Various forms of asset loading
        private void LoadAssets()
        {
            // path to the folder where the mod dll is located
            string modPath = Path.GetDirectoryName(Info.Location);

            // Load asset bundle from the filesystem
            TestAssets = AssetUtils.LoadAssetBundle(Path.Combine(modPath, "assets"));
            Jotunn.Logger.LogInfo(TestAssets);

            // Print Embedded Resources
            Jotunn.Logger.LogInfo(
                $"Embedded resources: {string.Join(", ", typeof(NewEffects).Assembly.GetManifestResourceNames())}");

            SteelIngotBundle = AssetUtils.LoadAssetBundleFromResources("steel");

        }

        // Add custom item conversions
        private void AddCustomItemConversions()
        {
            // Load and create a custom item to use in another conversion
            var steel_prefab = SteelIngotBundle.LoadAsset<GameObject>("Steel");
            var ingot = new CustomItem(steel_prefab, fixReference: false);
            ItemManager.Instance.AddItem(ingot);

            // Create a conversion for the blastfurnace, the custom item is the new outcome
            var blastConfig = new SmelterConversionConfig();
            blastConfig.Station =
                "blastfurnace"; // Override the default "smelter" station of the SmelterConversionConfig
            blastConfig.FromItem = "Iron";
            blastConfig.ToItem = "Steel"; // This is our custom prefabs name we have loaded just above
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(blastConfig));
        }


        [HarmonyPatch(typeof(Player), "Update")]
        public class CheckButton
        {
            [HarmonyPrefix]
            static void addEffectToPlayer()
            {

                if (Input.GetKey(KeyCode.Keypad9))
                {
                    //Efecto VFX
                    Instantiate(ZNetScene.instance.GetPrefab("vfx_Potion_stamina_medium"), Player.m_localPlayer.transform.position, Quaternion.identity);

                    //Efecto de Sonido
                    Instantiate(ZNetScene.instance.GetPrefab("sfx_dragon_coldball_start"), Player.m_localPlayer.transform.position, Quaternion.identity);

                    //Asigna el estado
                    Player.m_localPlayer.GetSEMan().AddStatusEffect(moreHpEffect);
                    coolDownDragon = DateTime.Now;
                }
                
            }

        }



        [HarmonyPatch(typeof(Humanoid), "DrainEquipedItemDurability")]
        public class ChangeDurability
        {

            [HarmonyPrefix]
            public static void LessDurability(ItemDrop.ItemData item)
            {


                float algo = item.m_durability;

                string name = item.m_shared.m_name;

                float draindmg = item.m_shared.m_durabilityDrain;

                

                if (name == "$item_cape_lox" && EnvMan.instance.GetCurrentEnvironment().m_name == "SnowStorm")

                {
                    item.m_durability -= 0.1f;
                }
                

            }
            
            
            [HarmonyPatch(typeof(SE_Stats), "UpdateStatusEffect")]
            public class CheckStatus
            {
                static void Prefix(ref float ___m_healthPerTick)
                {    
                
                    if (EnvMan.instance.GetCurrentEnvironment().m_name == "SnowStorm" && EnvMan.instance.IsFreezing())
                    {
                    
                        ___m_healthPerTick = -10f;

                    }

                }
            }



            [HarmonyPatch(typeof(StatusEffect), "UpdateStatusEffect")]
            public class checkBuff
            {
                [HarmonyPostfix]
                static void checkCustomStatusEffect()
                {

                    if (Player.m_localPlayer.m_seman.HaveStatusEffect("NAZ_TANK_DragonRage"))
                    {
                        SE_Stats dragonRage = moreHpEffect;


                        //Busca el estado en el jugador
                        StatusEffect effectsd = Player.m_localPlayer.m_seman.GetStatusEffect(dragonRage.NameHash());

                        //Debug.Log(effectsd.GetRemaningTime());

                        //Tiempo ahora
                        DateTime checkTime = DateTime.Now;

                        //Calcula para ver cuantos segundos han pasado
                        int diferencia = (int)(checkTime - coolDownDragon).TotalSeconds;

                        //Si la diferencia de segundos desde la ultima vez a ahora son mas de 3 segundos , entrara , actualizara el cooldown y curara
                        if (diferencia >= 3)
                        {
                            // Obtener la marca de tiempo después de algunas operaciones o esperas
                            coolDownDragon = DateTime.Now;
                            Player.m_localPlayer.Heal(3f, true);
                        }


                        if (effectsd.GetRemaningTime() < 3f)
                        {
                            Debug.Log("Se quitara el estado");
                            Player.m_localPlayer.m_seman.RemoveStatusEffect(dragonRage.NameHash());

                        }
                    }


                    
                }

            }


            [HarmonyPatch(typeof(Player), "AddUniqueKey")]
            public class SetClass
            {
                [HarmonyPrefix]
                static void setClassToPlayer()
                {

                    if (Player.m_localPlayer.m_uniques.Contains("Berserker"))
                    {
                        Debug.Log("Es Berserker.");

                    }

                    if (Player.m_localPlayer.m_uniques.Contains("Paladin"))
                    {
                        Debug.Log("Es Paladin");

                    }

                    PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                    string name = playerProfile.GetName();

                    if (Game.instance.GetPlayerProfile().GetName() == name)
                    {
                        Player.m_localPlayer.m_uniques.Add("Paladin");
                    }

                }
            }


            [HarmonyPatch(typeof(Tameable), "TameAllInArea")]
            public class SetTamed
            {
                [HarmonyPrefix]
                static void setClassToPlayer()
                {

                    foreach (Character allCharacter in Character.GetAllCharacters())
                    {
                        Debug.Log("El valor de allCharacter es: " + allCharacter);
                        if (!allCharacter.IsPlayer())
                        {
                            allCharacter.SetTamed(true);
                            allCharacter.m_tolerateFire = true;

                        }
                    }

                }
            }

        }

    }
}
