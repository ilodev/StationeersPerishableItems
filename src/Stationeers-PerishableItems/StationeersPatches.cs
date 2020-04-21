using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using HarmonyLib;

namespace Stationeers_PerishableItems
{

    // Function helpers for this functionality not relevant enough to have their own file class.
    #region Helper functions
    internal static class PerishableItemsHelpers
    {
        public static float Clamp(float val, float min, float max)
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static float unClamp(float val, float min, float max)
        {
            if (val.CompareTo(min) < 0) return (val * -1);
            else if (val.CompareTo(max) > 0) return (val - max);
            else return 0;
        }

        public static void applyFoodDamage(Food foodItem, float baseDamage)
        {

            // TODO: maybe move these settings to the config file?
            // Clamp the temperature value between 0C and 40C, below 0C there is no spoiling, beyond 40C takes max damage.
            float tempC = PerishableItemsHelpers.Clamp(foodItem.WorldAtmosphere.Temperature - 273.15f, 0.0f, 40.0f);
            float damage = baseDamage * (tempC / 40.0f) * Math.Max(foodItem.WorldAtmosphere.ParticalPressureO2, 0.001f);

            // Add a little toxicity to the food, modified by the nutrition value of the item, the toxicity
            // will be used later to apply stun damage to the player ingesting this food item.
            // foodItem.NutritionValue *= 0.98f; // to make food lose nutrients
            foodItem.DamageState.Brute += 0.005f * damage;
            foodItem.DamageState.Toxic += 0.003f * damage;
            foodItem.NutritionValue    -= 0.001f * damage;
#if DEBUG
                if (damage > 0.0f) { 
                    UnityEngine.Debug.Log(
                        " Food ID: " + foodItem.ReferenceId + 
                        " Name: " + foodItem.DisplayName + 
                        " Temp: " + foodItem.WorldAtmosphere.Temperature + 
                        " Damage: " + damage + 
                        " O2: " + foodItem.WorldAtmosphere.GasMixture.Oxygen.Quantity
                    );
                }
#endif

        }

        public static void applyPlantDamage(Plant plantItem, float baseDamage)
        {

            // Clamp temps outside of the 0-40, below 0 and above 40 there is spoiling
            float tempC = PerishableItemsHelpers.unClamp(plantItem.WorldAtmosphere.Temperature - 273.15f, 0.0f, 40.0f);
            // Clamp again between 0 and 40
            tempC = PerishableItemsHelpers.Clamp(tempC, 0, 40);
            float damage = baseDamage * (tempC / 40.0f);

            plantItem.DamageState.Brute += 0.010f * damage;
            plantItem.DamageState.Toxic += 0.001f * damage;
            plantItem.NutritionValue    -= 0.001f * damage;
#if DEBUG
            if (damage > 0.0f) {
                UnityEngine.Debug.Log(
                    " Plant ID: " + plantItem.ReferenceId + 
                    " Name: " + plantItem.DisplayName + 
                    " Temp: " + plantItem.WorldAtmosphere.Temperature + 
                    " Damage: " + damage + 
                    " CO2: " + plantItem.WorldAtmosphere.GasMixture.CarbonDioxide.Quantity
                );
            }
#endif
        
        }
    }
#endregion

    // To enable consumables spoiling we will be applying decay damage on the Update() event of the monobehaviour
    // class for any given item of the food/plant classes.
    #region Damage on Update

    // Apply damage to the perishable items on the Update() event.
    [HarmonyPatch(typeof(DynamicThing), "Update")]
    class DynamicThingupdate
    {
        static void Prefix(DynamicThing __instance)
        {

            // The code has been thought as an -exit as soon as you can- to prevent unnecessary execution since
            // the Update() function per-item is going to be stressing.
            try
            {
                if (__instance == null)
                    return;

                // Ignore anything inside a closed container (not locker)
                if (PerishableItemsPlugin.PluginAllowSealed.Value == true && __instance.ParentSlot != null && __instance.ParentSlot.Parent.GetType() == typeof(Container) && !__instance.ParentSlot.Parent.IsOpen)
                    return;

                // Despite some having nutritients, Veggies and plants are not foodItems so we need both checks.
                Plant plantItem = __instance as Plant;
                Food foodItem = __instance as Food;

                if (foodItem == null && plantItem == null)
                    return;

                // On the decidated server, the atmosphere wont get the first update until a player logs in.
                if (__instance.WorldAtmosphere == null)
                    return;

                // From here onwards we might be applying damage, this baseDamage will act as a multiplier.
                float baseDamage = PerishableItemsPlugin.PluginDecayMultiplier.Value;

                // Apply x4 decay damage if the item is floating around, in the player hands or in any container that is open
                if (__instance.ParentSlot == null || __instance.ParentSlot.IsHandSlot || __instance.ParentSlot.Parent.IsOpen)
                    baseDamage *= PerishableItemsPlugin.PluginUnprotectedMultiplier.Value;

                // Finally 
                if (foodItem != null)
                    PerishableItemsHelpers.applyFoodDamage(foodItem, baseDamage);

                if (plantItem != null)
                    PerishableItemsHelpers.applyPlantDamage(plantItem, baseDamage);

#if DEBUG
                if (__instance.DamageState.Brute > 0)
                {
                    float NutritionValue = (foodItem != null) ? foodItem.NutritionValue : plantItem.NutritionValue;
                    UnityEngine.Debug.Log(
                        " ID: " + __instance.ReferenceId +
                        " Name: " + __instance.DisplayName +
                        " Max: " + __instance.DamageState.MaxDamage +
                        " Brute: " + __instance.DamageState.Brute +
                        " Toxic: " + __instance.DamageState.Toxic + 
                        " Nutri: " + NutritionValue);
                }
#endif

            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(
                    " Inconsistent item damage for instance ReferenceId: " + __instance.ReferenceId +
                    "\n" + e.Message);
            }

        }
    }
    #endregion

    // Splitting and Merging stacks can make items lose its stats as they are reset when a new DynamicThing is created,
    // therefore requires custom checking of merging/spitting operations. Food does not allow stacking. Plants are the
    // consumable we are going to make sure gets the maximum toxic/brute damage everytime the stack is merged or split.
    // Plants will heal while they are growing (assuming they meet all the requirements of water, pressure, atmos and light,
    // but they will keep previous toxicity which will be shared with the stack once is merged after harvesting.
    #region Splitting/Merging stacks

    // Plants can be stacked and merged, the rule is we apply the max damage to the new stack
    [HarmonyPatch(typeof(Stackable), "OnMergeStack", new Type[] { typeof(Stackable), typeof(float) })]
    class MergingFood
    {
        static void Prefix(Stackable __instance, Stackable oldStack, float delta)
        {
            try
            {
                Plant plantItem = __instance as Plant;
                if (plantItem == null)
                    return;

                __instance.DamageState.Brute = Math.Max(__instance.DamageState.Brute, oldStack.DamageState.Brute);
                __instance.DamageState.Toxic = Math.Max(__instance.DamageState.Toxic, oldStack.DamageState.Toxic);
#if DEBUG
                UnityEngine.Debug.Log(
                    " MERGESTACK OLD ID: " + __instance.ReferenceId +
                    " Name: " + __instance.DisplayName +
                    " Damage: " + __instance.DamageState.Brute
                );
                UnityEngine.Debug.Log(
                    " MERGESTACK NEW ID: " + oldStack.ReferenceId +
                    " Name: " + oldStack.DisplayName +
                    " Damage: " + oldStack.DamageState.Brute
                );
#endif
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(
                    " Inconsistent Plant stack merge found for instance ReferenceId: " + __instance.ReferenceId +
                    " oldStack referenceId: " + oldStack.ReferenceId +
                    "\n" + e.Message);
            };
        }
    }

    [HarmonyPatch(typeof(Stackable), "OnSplitStack", new Type[] { typeof(Stackable) })]
    class SplitingFood
    {
        static void Prefix(Stackable __instance, Stackable newStack)
        {

            try
            {
                Plant plantItem = __instance as Plant;
                if (plantItem == null)
                    return;

                newStack.DamageState.Brute = Math.Max(__instance.DamageState.Brute, newStack.DamageState.Brute);
                newStack.DamageState.Toxic = Math.Max(__instance.DamageState.Toxic, newStack.DamageState.Toxic);
#if DEBUG
                UnityEngine.Debug.Log(
                    " SPLITSTACK OLD ID: " + __instance.ReferenceId +
                    " Name: " + __instance.DisplayName +
                    " Damage: " + __instance.DamageState.Brute
                );
                UnityEngine.Debug.Log(
                    " SPLITSTACK NEW ID: " + newStack.ReferenceId +
                    " Name: " + newStack.DisplayName +
                    " Damage: " + newStack.DamageState.Brute
                );
#endif
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(
                    " Inconsistent Plant stack merge found for instance ReferenceId: " + __instance.ReferenceId +
                    " newStack referenceId: " + newStack.ReferenceId +
                    "\n" + e.Message);
            };
        }
    }

    #endregion

    // We will apply damage to the human consuming spoiled food, in the form of:
    // - short term damage as stun, quickly recoverable 
    // - mid term damage as health, can be recovered with meds, or made a healing buff based on food too.
    // - long term damage as toxicity, requires meds.
    // Plants are not included since they have little to none nutritional value, however they should carry
    // on the toxicity to their fruits when harvesting since this will be a stack exchange and toxicity is not
    // healed up on the plants.
    #region Player Damage for consuming spoiled food
    [HarmonyPatch(typeof(Food), "OnUseItem", new Type[] { typeof(float), typeof(Thing) })]
    class FoodOnUse
    {
        static void Prefix(Food __instance, float quantity, Thing useOnThing)
        {

            try
            {
                Human target = useOnThing as Human;
                if (target == null)
                    return;

                // Toxicity gets accumulated slowly over time
                float toxicity = __instance.DamageState.Toxic * quantity / 100;

                // Compute damages as the mult of the ingested ammount, the item toxicity/bute and the multiplier value.
                float spoil = __instance.DamageState.Brute / __instance.DamageState.MaxDamage;

                // Player damage
                float brute = 0;
                if (spoil > 50)
                    brute = __instance.DamageState.Brute * quantity / 100;

                // Player sutn
                float stun = 0;
                if (spoil > 75)
                    stun = __instance.DamageState.Brute * quantity / 5;

#if DEBUG
                UnityEngine.Debug.Log(
                    " OnUseFood: " + __instance.DisplayName +
                    " Quantity : " + quantity + 
                    " Brute: " + __instance.DamageState.Brute + 
                    " Toxic: "  + __instance.DamageState.Toxic
                );
#endif

                target.DamageState.Toxic += toxicity * PerishableItemsPlugin.PluginToxicMultiplier.Value;
                target.DamageState.Brute += brute * PerishableItemsPlugin.PluginDamageMultiplier.Value;
                target.DamageState.Stun += stun * PerishableItemsPlugin.PluginStunMultiplier.Value;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(
                    " Inconsistent Food onUseItem found for instance ReferenceId: " + __instance.ReferenceId +
                    " Thing referenceId: " + useOnThing.ReferenceId +
                    "\n" + e.Message);
            }

            return;
        }
    }

#endregion

}


