
## Perishable Items mod

"Stationeers Perishable Items" is a Stationers BepInEx plugin adding decay and poisoning mechanics 
to the game food and plant items.

### Download
Either donwload the source code and build it yourself or download the available release from
https://github.com/ilodev/StationeersPerishableItems/releases/

### Installation
PerishableItems Requires BepInEx 5.0.1 or later from 
https://github.com/BepInEx/BepInEx/releases

- Install BepInEx in the Stationeers steam folder.
- Launch the game, reach the main menu, then quit back out.
- In the steam folder of the game, there should now be a folder named BepInEx/Plugins.
- Copy the Stationeers-PerisableItems folder from this mod into BepInEx/Plugins/ folder.
- Launch the game again.

### Multiplayer Support
If this mod is installed on a dedicated server, it will work for all clients. If a client 
has this mod installed and joins the server, the mod will have no effect.

### Configuration
The mod will create a config file in the BepInHex/Config/ called. The following options will
allow for different gameplay and difficulty

```
## Settings file was created by plugin Perishable Items for Stationeers v1.0.4.0
## Plugin GUID: org.ilo.plugins.Stationeers_PerishableItems
## Settings file was created by plugin Perishable Items for Stationeers v1.0.4.0
## Plugin GUID: org.ilo.plugins.Stationeers_PerishableItems

[General]

## Enable or disable the plugin
# Setting type: Boolean
# Default value: true
Enabled = true

## Multiplier for decay settings per consumable item
# Setting type: Single
# Default value: 1
DecayMultiplier = 1

## If true, container Crates will be sealed while closed and maintain food
# Setting type: Boolean
# Default value: true
AllowSealedContainer = true

[Rates]

## Multiplier for decay settings when the item is not in a closed container
# Setting type: Single
# Default value: 4
UnprotectedDecayMultiplier = 4

## Multiplier for food damage applied to canned food like Tomato soup, milk or cereal bars while closed
# Setting type: Single
# Default value: 0.1
CannedMultiplier = 0.1

## Multiplier for food damage applied to bottled food like soy oil
# Setting type: Single
# Default value: 0.001
BottledMultiplier = 0.001

## Multiplier for base damage applied to the player consuming decayed item
# Setting type: Single
# Default value: 1
DamageMultiplier = 1

## Multiplier for stun damage applied to the player consuming decayed item
# Setting type: Single
# Default value: 1
StunMultiplier = 1

## Multiplier for toxic damage applied to the player consuming decayed items
# Setting type: Single
# Default value: 1
ToxicMultiplier = 1
```

### Gameplay
The Mod introduces Decaying/Spoiling for both Plants and Items in a different way:

*Plants* will get damage when they freeze. A plant seed or a veggetable exposed to a temperature
below 0C will start getting hurt and potentially. All the stack will spoil at the same time. Merging
or splitting stacks will still keep the most damaged condition. seeds will recover their health once
they are planted however they would keep any toxicity they have been gaining and this toxicity will be
passed by to the fruits it generates.

*Food* will get no damage when kept under 0C, and will get damage when exposed to heat and oxygen. 
Food can't be stacked in a default game but if using a mod to change that behaviour, merging or splitting
stacks will still keep the most damaged condition. Food will lose nutrients the higher its decay state
and become toxic to the player after certain value.

Damage to both Plants and Foods can be reduced if they are stored in any inventory and that inventory is
closed (e.g. a suit, a locker). Food in your suit will last longer than in your hands or the ground as
long as you don't leave that inventory window open. plants and food can also be kept safe in a closed 
container crate since these are considered insulated items, however this behaviour is configurable.

Canned food (Milk, Tomato Soup or Cereal Bars) will also last longer while the container it is in has 
not been opened, but will decay normally once it has been started to be consumed.

*Humans* will get damage from eating a decayed item in different ways:

- The player might feel indisposed temporarily if the food was highly contaminated.
- the player will get health damage for consuming decayed items which can be treated by medicines or
  will eventually heal when on a cryopod.
- Toxicity will accumulate throught the body and can only be treated by advanced medicine.

Most of the values are tweakable through the config file in the form of multipliers to increase/reduce
the different effects.