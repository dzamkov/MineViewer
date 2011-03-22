Many thanks to gschizas (http://www.reddit.com/user/gschizas) for fixing a (major bug!)

If your player is in the nether and you view your map in overworld, the position is translated
to what it would be in the overworld, this is useful to find out where your portals will end up.

Requirements:
	* .Net framework 3.5 or Mono equivalent (untested)
		* Can obtain from http://www.microsoft.com/net/download.aspx

Installing Mono and Monos System.Windows.Forms library for Linux:
	Open a terminal and enter "sudo apt-get install mono libmono-winforms*" and press enter.

Making your own scheme:
Open schemes.lua in a text editor
Add this line to the end if you want to start from scratch: Schemes.MySchemeName = { }
or if you want to derive from another scheme: Schemes.MySchemeName = Derive(Schemes.ToDeriveFrom)

See items below and add items as you wish: (All of these values range from 0.0 to 1.0, anything above or below may cause MineViewer to crash)

-- this is the format { red, green, blue, border size, border red, border green, border blue }
Schemes.MySchemeName[Type] = { r = 0.6, g = 0.6, b = 0.6, border = 0.3, b_r = 1.0, b_g = 1.0, b_b = 1.0 }

Save scheme.lua and reload MineViewer and enjoy!

Lua Item Types: (Ignore if you are not making your own scheme) (If an update comes out, you can grab the Item ID from the wiki and use that instead of the type)

Air			0
Empty			0
Stone			1
Grass			2
Dirt			3
Cobblestone		4
Wood			5
Sapling			6
Bedrock			7
Adminium		7
Water			8
MovingWater		8
StationaryWater		9
Lava			10
MovingLava		10
StationaryLava		11
Sand			12
Gravel			13
GoldOre			14
IronOre			15
CoalOre			16
Log			17
Leaves			18
Sponge			19
Glass			20
LapisLazuliOre		21
LapisLazuliBlock	22
Dispenser		23
Sandstone		24
NoteBlock		25
Cloth			35
Wool			35
YellowFlower		37
RedRose			38
RedFlower		38
Flower			38
BrownMushroom		39
RedMushroom		40
Mushroom		40
GoldBlock		41
Gold			41
IronBlock		42
Iron			42
DoubleStep		43
HalfStep		44
Step			44
Brick			45
TNT			46
Asplodies		46
Bookcase		47
Bookshelf		47
Books			47
MossyCobblestone	48
Obsidian		49
Torch			50
Fire			51
MobSpawner		52
Spawner			52
WoodenStairs		53
WoodStairs		53
Chest			54
RedstoneWire		55
Redstone		55
Wire			55
DiamondOre		56
DiamondBlock		57
Diamond			57
Workbench		58
Workstation		58
Crops			59
Wheat			59
Soil			60
TiledDirt		60
Furnace			61
BurningFurnace		62
LitFurnace		62
Sign			63
SignPost		63
WoodenDoor		64
WoodDoor		64
Door			64
Ladder			65
Tracks			66
MinecartTracks		66
Rails			66
CobblestoneStairs	67
StoneStairs		67
WallSign		68
MountedSign		68
Lever			69
Switch			69
StonePlate		70
StonePressurePlate	70
IronDoor		71
WoodenPlate		72
WoodenPressurePlate	72
WoodPlate		72
WoodPressurePlate	72
RedstoneOre		73
GlowingRedstoneOre	74
OnRedstoneTorch		75
RedstoneTorch		75
OffRedstoneTorch	76
StoneButton		77
Button			77
Snow			78
Ice			79
SnowBlock		80
Cactus			81
Clay			82
Reed			83
Bamboo			83
Jukebox			84
Fence			85
Pumpkin			86
LitPumpkin		91
NetherStone		87
RedMossyCobbleStone	87
HellStone		87
Mud			88
GlowStone		89
Australium		89
LightStone		89
Portal			90
Cake			92