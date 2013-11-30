-- Color schemes are defined with lua. All members of the table "Schemes" will be added
-- as selectable color schemes at runtime. Colors can be assigned by block names or by block id. The Derive
-- function can be used to create a new scheme based off another.
Schemes.Default = {}

-- Common materials
Schemes.Default[Stone] = { r = 0.6, g = 0.6, b = 0.6 }
Schemes.Default[Step] = { r = 0.5, g = 0.5, b = 0.7 }
Schemes.Default[DoubleStep] = { r = 0.5, g = 0.5, b = 0.7 }
Schemes.Default[Grass] = { r = 0.2, g = 0.7, b = 0.2 }
Schemes.Default[Dirt] = { r = 0.6, g = 0.4, b = 0.2 }
Schemes.Default[TiledDirt] = { r = 0.4, g = 0.2, b = 0.0 }
Schemes.Default[Cobblestone] = { r = 0.6, g = 0.6, b = 0.6, border = 0.3, b_r = 1.0, b_g = 1.0, b_b = 1.0 }
Schemes.Default[Wood] = { r = 0.8, g = 0.6, b = 0.3 }
Schemes.Default[Fence] = { r = 0.9, g = 0.7, b = 0.5 }
Schemes.Default[Bedrock] = { r = 0.2, g = 0.2, b = 0.2 }
Schemes.Default[Sand] = { r = 1.0, g = 1.0, b = 0.5 }
Schemes.Default[Sandstone] = { r = 0.9, g = 0.9, b = 0.4 }
Schemes.Default[Gravel] = { r = 0.9, g = 0.6, b = 0.7 }
Schemes.Default[Log] = { r = 0.5, g = 0.2, b = 0.1 }
Schemes.Default[Leaves] = { r = 0.4, g = 1.0, b = 0.4 }
Schemes.Default[Brick] = { r = 0.8, g = 0.3, b = 0.3, border = 0.3, b_r = 1.0, b_g = 1.0, b_b = 1.0 }
Schemes.Default[MossyCobblestone] = { r = 0.6, g = 0.6, b = 0.6, border = 0.3, b_r = 0.0, b_g = 0.8, b_b = 0.0 }
Schemes.Default[Obsidian] = { r = 0.3, g = 0.0, b = 0.2 }
Schemes.Default[Ice] = { r = 0.4, g = 0.4, b = 1.0, a = 0.7 }
Schemes.Default[Clay] = { r = 0.6, g = 0.6, b = 0.8, border = 0.7, b_r = 1.0, b_g = 1.0, b_b = 0.5 }
Schemes.Default[Glass] = { r = 0.5, g = 0.5, b = 1.0, a = 0.3 }
Schemes.Default[NetherStone] = { r = 0.3, g = 0.1, b = 0.1 }
Schemes.Default[Portal] = { r = 0.7, g = 0.5, b = 0.7, a = 0.8 }
Schemes.Default[Cactus] = { r = 0.1, g = 0.1, b = 0.1, border = 0.9, b_r = 0.5, b_g = 1.0, b_b = 0.5 }
Schemes.Default[StainedClay] = { r = 0.9, g = 0.9, b = 0.5 }
Schemes.Default[HardenedClay] = { r = 0.9, g = 0.8, b = 0.5 }

-- Liquids
Schemes.Default[StationaryWater] = { r = 0.0, g = 0.0, b = 1.0, a = 0.6 }
Schemes.Default[MovingWater] = { r = 0.0, g = 0.0, b = 1.0, a = 0.5 }
Schemes.Default[StationaryLava] = { r = 1.0, g = 0.0, b = 0.0, a = 0.7 }
Schemes.Default[MovingLava] = { r = 1.0, g = 0.0, b = 0.0, a = 0.6 }

-- Ores
Schemes.Default[GoldOre] = { r = 1.0, g = 1.0, b = 0.0, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[IronOre] = { r = 1.0, g = 0.5, b = 0.0, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[CoalOre] = { r = 0.1, g = 0.1, b = 0.1, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[DiamondOre] = { r = 0.0, g = 0.5, b = 0.7, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[RedstoneOre] = { r = 1.0, g = 0.0, b = 0.0, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[GlowingRedstoneOre] = { r = 1.0, g = 0.0, b = 0.0, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[GlowStone] = { r = 1.0, g = 1.0, b = 0.0, border = 0.7, b_r = 1.0, b_g = 1.0, b_b = 1.0 }
Schemes.Default[LapisLazuliOre] = { r = 0.0, g = 0.0, b = 1.0, border = 0.7, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[LapisLazuliBlock] = { r = 0.0, g = 0.0, b = 1.0, border = 0.1, b_r = 0.6, b_g = 0.6, b_b = 0.6 }

-- Functional blocks
Schemes.Default[MobSpawner] = { r = 1.0, g = 1.0, b = 1.0, border = 0.3, b_r = 0.0, b_g = 0.0, b_b = 1.0 }
Schemes.Default[Chest] = { r = 0.8, g = 0.5, b = 0.0, border = 0.3, b_r = 0.85, b_g = 0.6, b_b = 0.3 }
Schemes.Default[Workbench] = { r = 0.8, g = 0.7, b = 0.6, border = 0.3, b_r = 0.85, b_g = 0.6, b_b = 0.3 }
Schemes.Default[Jukebox] = { r = 0.6, g = 0.4, b = 0.25, border = 0.3, b_r = 0.85, b_g = 0.6, b_b = 0.3 }
Schemes.Default[NoteBlock] = { r = 0.6, g = 0.7, b = 0.25, border = 0.3, b_r = 0.85, b_g = 0.6, b_b = 0.3 }
Schemes.Default[Furnace] = { r = 0.8, g = 0.8, b = 0.8, border = 0.3, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[Dispenser] = { r = 0.7, g = 0.7, b = 0.7, border = 0.3, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[LitFurnace] = { r = 0.8, g = 0.8, b = 0.8, border = 0.3, b_r = 0.6, b_g = 0.6, b_b = 0.6 }
Schemes.Default[Mud] = { r = 0.2, g = 0.2, b = 0.2 }
Schemes.Default[Pumpkin] = {r = 1.0, g = 0.4, b = 0.0}
Schemes.Default[LitPumpkin] = {r = 1.0, g = 0.6, b = 0.2}
Schemes.Default[Cake] = {r = 1.0, g = 1.0, b = 1.0}





-- Some included schemes.
Schemes.CaveView = { }
Schemes.CaveView[Air] = { r = 0.6, g = 0.6, b = 0.6 }

Schemes.MinersView = Derive(Schemes.CaveView)
Schemes.MinersView[MovingLava] = Schemes.Default[MovingLava]
Schemes.MinersView[StationaryLava] = Schemes.Default[StationaryLava]
Schemes.MinersView[GoldOre] = Schemes.Default[GoldOre]
Schemes.MinersView[DiamondOre] = Schemes.Default[DiamondOre]

Schemes.CivilizationView = { }
Schemes.CivilizationView[Cobblestone] = { r = 0.6, g = 0.6, b = 0.6, border = 0.3, b_r = 1.0, b_g = 1.0, b_b = 1.0 }
Schemes.CivilizationView[Wood] = { r = 0.8, g = 0.6, b = 0.3 }
Schemes.CivilizationView[Chest] = Schemes.Default[Chest]
Schemes.CivilizationView[Workbench] = Schemes.Default[Workbench]
Schemes.CivilizationView[Jukebox] = Schemes.Default[Jukebox]
Schemes.CivilizationView[Furnace] = Schemes.Default[Furnace]
Schemes.CivilizationView[LitFurnace] = Schemes.Default[LitFurnace]
Schemes.CivilizationView[RedstoneWire] = { r = 0.8, g = 0.2, b = 0.2 }
Schemes.CivilizationView[MinecartTracks] = { r = 0.4, g = 0.6, b = 0.3 }
Schemes.CivilizationView[Torch] = { r = 1.0, g = 1.0, b = 0.0 }
Schemes.CivilizationView[Portal] = Schemes.Default[Portal]