# simpl
Simcraft Impl

Simraft Impl is an Implementation of the Simcraft Action Priority List Expression Language for Honorbuddy assisted 
play with DPS Specialisations.

Simcraft Impl does not support:

Shadow Priest
Feral Druid
Subtlety Rogue

for now.

Action Priority List:

Simcraft provides a plethora of Action Lists that model the perfect rotation for a class, you can find them here:

https://code.google.com/p/simulationcraft/source/browse/#git%2Fprofiles%2FTier17H

You can also simply import your character into Simcraft itself and copy that into a .simc file, this has the advantage 
that the list will already contain special actions to for example use the trinkets your character has.

Want to learn how the Action Priority List works and how you can write your own ? 
Luckily Simcraft provides an in depth Documentation of almost everything possible, you can find it here:

https://code.google.com/p/simulationcraft/wiki/ActionLists

To allow for more customization Simcraft Impl adds a few extra Expressions:

Hotkeys: 

aoe_enabled / cooldowns_enables

These two conditions allow you to control the flow of the rotation, you can set them in the configuration Interface
However, on their own these do not do anything, you need to check for the explicitly in your action list conditions.

Custom Hotkeys:

The two hotkeys mentioned above already allow you most of the customization you will need, however should you need more
you can define new ones like this

hotkeys+=/potion_enabled,alt,E

And then simply use that hotkeys name inside your expression conditions like this:

actions+=/vanish,if=potion_enabled

The Variables start at false and toggle with each press of the hotkey

its name, modifier key:

modifier can be: alt ctrl shift none
key can be can of the ones liste here: https://msdn.microsoft.com/en-us/library/system.windows.forms.keys%28v=vs.110%29.aspx