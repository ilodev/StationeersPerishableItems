using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

// Suggestion: Make damage for poisoned food more a buff/debuff/state instead of an instant damage based on the toxicity, use 
//             toxicity to create a pseudorandom debuff for health.
// Suggestion: Have isolated items (like cereal bars, or tomato soup) being more flexible with it comes to decaying conditions.
// Suggestion: Nutritional value loss is a fixed value that affects differently depending on the food item, to allow food with 
//             little nutritional increasing your hunger value when spoiled. Consider another approach.
// Suggestion: Cramp the damage/toxicity values applied to players.
// TODO      : Disable in creative
// TODO      : Check whether Pause mode affects decay
// TODO      : Move all hardcoded values as defines

namespace Stationeers_PerishableItems
{
    [BepInPlugin("org.ilo.plugins.Stationeers_PerishableItems", "Perishable Items for Stationeers", "1.0.0.0")]
    public class PerishableItemsPlugin : BaseUnityPlugin
    {

        // Allow enabling/disabling the plugin functionality
        private static ConfigEntry<bool>  PluginEnabled;

        // Provides a difficulty multiplier to make your life more miserable
        public static ConfigEntry<float> PluginDecayMultiplier;

        // Allow enabling/disabling the sealed Container Crates
        public static ConfigEntry<bool> PluginAllowSealed;

        // How much decay affects food or plants when they are not in a locked item (suit, locker etc).
        public static ConfigEntry<float> PluginUnprotectedMultiplier;

        // How much food decay affects player heath as a warning of poisoning, and to slow down eating in difficulty settings 
        public static ConfigEntry<float> PluginDamageMultiplier;

        // How much food decay affects player stun as a warning of poisoning, and to slow down eating in difficulty settings 
        public static ConfigEntry<float> PluginStunMultiplier;

        // How much food decay affects player toxicity, requiring medication to recover after ingesting decayed food
        public static ConfigEntry<float> PluginToxicMultiplier;


        // Setup the config for the plugin
        private void ConfigBindings()
        {
            PluginEnabled = Config.Bind(
                "General",                       // The section under which the option is shown
                "Enabled",                       // The key of the configuration option in the configuration file
                true,                            // The default value
                "Enable or disable the plugin"   // Description of the option to show in the config file
            );
            PluginDecayMultiplier = Config.Bind(
                "General",
                "DecayMultiplier",
                1.0f,
                "Multiplier for decay settings per consumable item"
            );
            PluginAllowSealed = Config.Bind(
                "General",
                "AllowSealedContainer",
                true,
                "If true, container Crates will be sealed while closed and maintain food"
            );
            PluginUnprotectedMultiplier = Config.Bind(
                "Rates",
                "UnprotectedDecayMultiplier",
                4.0f,
                "Multiplier for decay settings when the item is not in a closed container"
            );
            PluginDamageMultiplier = Config.Bind(
                "Rates",
                "DamageMultiplier",
                1.0f,
                "Multiplier for base damage applied to the player consuming decayed item"
            );
            PluginStunMultiplier = Config.Bind(
                "Rates",
                "StunMultiplier",
                1.0f,
                "Multiplier for stun damage applied to the player consuming decayed item"
            );
            PluginToxicMultiplier = Config.Bind(
                "Rates",
                "ToxicMultiplier",
                1.0f,
                "Multiplier for toxic damage applied to the player consuming decayed items"
            );

        }


        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            ConfigBindings();

            // Nothing to do if the plugin is disabled
            if (!PluginEnabled.Value)
                return;

            UnityEngine.Debug.Log("Perishable Items Enabled");

            try
            {
#if DEBUG
                // Harmony.DEBUG = true;
#endif
                var harmony = new Harmony("net.ilo.stationeers.PerishableItemsPlugin");
                harmony.PatchAll();

            } catch (Exception e)
            {
                UnityEngine.Debug.Log("Patch Failed " + e.ToString());
            }
        }
    }
}
