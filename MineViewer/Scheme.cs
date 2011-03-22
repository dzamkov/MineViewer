using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using KopiLua;
using Cubia;

namespace MineViewer
{

    class Schemes
    {
        public string Name;
        public Dictionary<byte, IMaterial> SchemeMaterials;
        public Schemes(string name)
        {
            Name = name;
            SchemeMaterials = new Dictionary<byte, IMaterial>();
        }
        public void Add(byte id, IMaterial mat)
        {
            SchemeMaterials.Add(id, mat);
        }
    }

    /// <summary>
    /// Defines a mapping of block types to materials.
    /// </summary>
    public class Scheme
    {
    
        public Scheme(Dictionary<byte, IMaterial> Materials)
        {
            this._Materials = Materials;
        }

        /// <summary>
        /// Loads a set of named schemes from a file.
        /// </summary>
        public static Dictionary<string, Scheme> Load(Stream File)
        {
            Lua.lua_State l = Lua.luaL_newstate();
            Lua.luaL_openlibs(l);

            // Add block constants
            foreach(KeyValuePair<string, byte> blockname in Names)
            {
                Lua.lua_pushnumber(l, blockname.Value);
                Lua.lua_setglobal(l, blockname.Key);
            }

            // Derive function
            Lua.lua_pushcfunction(l, delegate(Lua.lua_State ll)
            {
                int source = Lua.lua_gettop(ll);
                Lua.lua_newtable(ll);
                int dest = Lua.lua_gettop(ll);

                // Copy
                Lua.lua_pushnil(ll);
                while (Lua.lua_next(ll, source) != 0)
                {
                    Lua.lua_pushvalue(ll, -2); // Copy key on top
                    Lua.lua_pushvalue(ll, -2); // Copy value on top

                    Lua.lua_settable(ll, dest);

                    Lua.lua_pop(ll, 1); // Remove value
                }


                return 1;
            });
            Lua.lua_setglobal(l, "Derive");

            // Create a "Schemes" variable

            Lua.lua_newtable(l);
            Lua.lua_setglobal(l, "Schemes");

            // Run that shit
            string lua = new StreamReader(File).ReadToEnd();
            int compileerr = Lua.luaL_loadstring(l, lua); 

            if(compileerr != 0)
            {
                string error = Lua.lua_tostring(l, -1).ToString();
                Lua.lua_pop(l, 1); // pop the error
                throw LuaException.Create(error, false);
            }

            int runerr = Lua.lua_pcall(l, 0, 0, 0);
            if (runerr != 0)
            {
                string error = Lua.lua_tostring(l, -1).ToString();
                Lua.lua_pop(l, 1); // pop the error
                throw LuaException.Create(error, true);
            }

            Lua.lua_pop(l, 1); // Pop the file

            // Read off schemes
            Dictionary<string, Scheme> schemes = new Dictionary<string, Scheme>();
            Lua.lua_getglobal(l, "Schemes");
            Lua.lua_pushnil(l);
            while(Lua.lua_next(l, -2) != 0)
            {
                // v, k, test table
                int k = -2;
                string name = Lua.lua_tostring(l, k).ToString();
                Dictionary<byte, IMaterial> data = new Dictionary<byte, IMaterial>();

                int internal_table = Lua.lua_gettop(l);
                // v, k, test table

                Lua.lua_pushnil(l);
                // nil, v, k, test table

                while (Lua.lua_next(l, internal_table) != 0)
                {
                    // v, k, v, k, test table
                    int k2 = -2;

                    byte id = (byte)Lua.lua_tonumber(l, k2);

                    int block_tbl = Lua.lua_gettop(l);

                    Lua.lua_getfield(l, block_tbl, "r"); // 1
                    Lua.lua_getfield(l, block_tbl, "g"); // 2
                    Lua.lua_getfield(l, block_tbl, "b"); // 3
                    Lua.lua_getfield(l, block_tbl, "a"); // 4
                    Lua.lua_getfield(l, block_tbl, "border"); // 5
                    Lua.lua_getfield(l, block_tbl, "b_r"); // 6
                    Lua.lua_getfield(l, block_tbl, "b_g"); // 7
                    Lua.lua_getfield(l, block_tbl, "b_b"); // 8

                    double r = Lua.lua_tonumber(l, block_tbl + 1);
                    double g = Lua.lua_tonumber(l, block_tbl + 2);
                    double b = Lua.lua_tonumber(l, block_tbl + 3);
                    double a = Lua.lua_tonumber(l, block_tbl + 4);
                    double border = Lua.lua_tonumber(l, block_tbl + 5);
                    double b_r = Lua.lua_tonumber(l, block_tbl + 6);
                    double b_g = Lua.lua_tonumber(l, block_tbl + 7);
                    double b_b = Lua.lua_tonumber(l, block_tbl + 8);

                    if (id == 20)
                    {
                        int abc = 101;
                        abc++;
                    }

                    if(border == 0)
                    {
                        if(a>0.0)
                            data[id] = new SolidColorMaterial(Color.RGBA(r, g, b, a));
                        else
                            data[id] = new SolidColorMaterial(Color.RGB(r, g, b));
                    }
                    else
                    {
                        if(a > 0.0)
                            data[id] = new BorderedMaterial(Color.RGB(b_r, b_g, b_b), border, new SolidColorMaterial(Color.RGBA(r, g, b, a)));
                        else
                            data[id] = new BorderedMaterial(Color.RGB(b_r, b_g, b_b), border, new SolidColorMaterial(Color.RGB(r, g, b)));
                    }

                    Lua.lua_pop(l, 8); // pop all the values

                    Lua.lua_pop(l, 1); // pop val - k is poped with next()
                    // k, v, k, test table
                }
                // v, k, test table
                Lua.lua_pop(l, 1);

                schemes.Add(name, new Scheme(data));
            }

            return schemes;
        }

        /// <summary>
        /// Gets the materials defined in this scheme. The dictionary should not be modified.
        /// </summary>
        public Dictionary<byte, IMaterial> Materials
        {
            get
            {
                return this._Materials;
            }
        }

        /// <summary>
        /// Gets the material between two blocks of the specified type, bordered on the specified axis.
        /// </summary>
        public Material MaterialBorder(byte Lower, byte Higher, Axis Axis)
        {
            IMaterial lower = null; this._Materials.TryGetValue(Lower, out lower);
            IMaterial higher = null; this._Materials.TryGetValue(Higher, out higher);

            if ((lower == null) ^ (higher == null)) // Only render if exactly one of the blocks are empty.
            {
                if (higher == null)
                {
                    return new Material() { Description = lower, Direction = Polarity.Positive };
                }
                else
                {
                    return new Material() { Description = higher, Direction = Polarity.Negative };
                }
            }
            return Material.Default;
        }

        /// <summary>
        /// Block names.
        /// </summary>
        public static readonly Dictionary<string, byte> Names;

        static Scheme()
        {
            Names = new Dictionary<string, byte>();
            Names.Add("Air", 0); Names.Add("Empty", 0);
            Names.Add("Stone", 1);
            Names.Add("Grass", 2);
            Names.Add("Dirt", 3);
            Names.Add("Cobblestone", 4);
            Names.Add("Wood", 5);
            Names.Add("Sapling", 6);
            Names.Add("Bedrock", 7); Names.Add("Adminium", 7);
            Names.Add("Water", 8); Names.Add("MovingWater", 8);
            Names.Add("StationaryWater", 9);
            Names.Add("Lava", 10); Names.Add("MovingLava", 10);
            Names.Add("StationaryLava", 11);
            Names.Add("Sand", 12);
            Names.Add("Gravel", 13);
            Names.Add("GoldOre", 14);
            Names.Add("IronOre", 15);
            Names.Add("CoalOre", 16);
            Names.Add("Log", 17);
            Names.Add("Leaves", 18);
            Names.Add("Sponge", 19);
            Names.Add("Glass", 20);
            Names.Add("LapisLazuliOre", 21);
            Names.Add("LapisLazuliBlock", 22);
            Names.Add("Dispenser", 23);
            Names.Add("Sandstone", 24);
            Names.Add("NoteBlock", 25);
            Names.Add("Cloth", 35); Names.Add("Wool", 35);
            Names.Add("YellowFlower", 37);
            Names.Add("RedRose", 38); Names.Add("RedFlower", 38); Names.Add("Flower", 38);
            Names.Add("BrownMushroom", 39);
            Names.Add("RedMushroom", 40); Names.Add("Mushroom", 40);
            Names.Add("GoldBlock", 41); Names.Add("Gold", 41);
            Names.Add("IronBlock", 42); Names.Add("Iron", 42);
            Names.Add("DoubleStep", 43);
            Names.Add("HalfStep", 44); Names.Add("Step", 44);
            Names.Add("Brick", 45);
            Names.Add("TNT", 46); Names.Add("Asplodies", 46); // Easter egg lol
            Names.Add("Bookcase", 47); Names.Add("Bookshelf", 47); Names.Add("Books", 47);
            Names.Add("MossyCobblestone", 48);
            Names.Add("Obsidian", 49);
            Names.Add("Torch", 50);
            Names.Add("Fire", 51);
            Names.Add("MobSpawner", 52); Names.Add("Spawner", 52);
            Names.Add("WoodenStairs", 53); Names.Add("WoodStairs", 53);
            Names.Add("Chest", 54);
            Names.Add("RedstoneWire", 55); Names.Add("Redstone", 55); Names.Add("Wire", 55);
            Names.Add("DiamondOre", 56);
            Names.Add("DiamondBlock", 57); Names.Add("Diamond", 57);
            Names.Add("Workbench", 58); Names.Add("Workstation", 58);
            Names.Add("Crops", 59); Names.Add("Wheat", 59);
            Names.Add("Soil", 60); Names.Add("TiledDirt", 60);
            Names.Add("Furnace", 61);
            Names.Add("BurningFurnace", 62); Names.Add("LitFurnace", 62);
            Names.Add("Sign", 63); Names.Add("SignPost", 63);
            Names.Add("WoodenDoor", 64); Names.Add("WoodDoor", 64); Names.Add("Door", 64);
            Names.Add("Ladder", 65);
            Names.Add("Tracks", 66); Names.Add("MinecartTracks", 66); Names.Add("Rails", 66);
            Names.Add("CobblestoneStairs", 67); Names.Add("StoneStairs", 67);
            Names.Add("WallSign", 68); Names.Add("MountedSign", 68);
            Names.Add("Lever", 69); Names.Add("Switch", 69);
            Names.Add("StonePlate", 70); Names.Add("StonePressurePlate", 70);
            Names.Add("IronDoor", 71);
            Names.Add("WoodenPlate", 72); Names.Add("WoodenPressurePlate", 72); Names.Add("WoodPlate", 72); Names.Add("WoodPressurePlate", 72);
            Names.Add("RedstoneOre", 73);
            Names.Add("GlowingRedstoneOre", 74);
            Names.Add("OnRedstoneTorch", 75); Names.Add("RedstoneTorch", 75);
            Names.Add("OffRedstoneTorch", 76);
            Names.Add("StoneButton", 77); Names.Add("Button", 77);
            Names.Add("Snow", 78);
            Names.Add("Ice", 79);
            Names.Add("SnowBlock", 80);
            Names.Add("Cactus", 81);
            Names.Add("Clay", 82);
            Names.Add("Reed", 83); Names.Add("Bamboo", 83);
            Names.Add("Jukebox", 84);
            Names.Add("Fence", 85);
            Names.Add("Pumpkin", 86); Names.Add("LitPumpkin", 91);
            Names.Add("NetherStone", 87); Names.Add("RedMossyCobbleStone", 87); Names.Add("HellStone", 87);
            Names.Add("Mud", 88);
            Names.Add("GlowStone", 89); Names.Add("Australium", 89); Names.Add("LightStone", 89);
            Names.Add("Portal", 90);
            Names.Add("Cake", 92);
            Names.Add("Repeater", 93); Names.Add("RepeaterOff", 93);
            Names.Add("RepeaterOn", 94);
        }

        private Dictionary<byte, IMaterial> _Materials;
    }

    /// <summary>
    /// An exception indicating there is a lua compile or run error.
    /// </summary>
    public class LuaException : Exception
    {
        /// <summary>
        /// Creates an exception based on a full error message given by lua.
        /// </summary>
        public static LuaException Create(string Description, bool Runtime)
        {
            LuaException le = new LuaException();
            le.Run = Runtime;
            le.LuaDesc = Description;
            return le;
        }

        /// <summary>
        /// Gets if this error occured while running the lua code.
        /// </summary>
        public bool Run;

        /// <summary>
        /// Description of the error given by lua.
        /// </summary>
        public string LuaDesc;
    }

    public static class GCScheme
    {
        public static Scheme Schm;
        public static bool IsTrans(byte type)
        {
            Scheme s = GCScheme.Schm;
            IMaterial imo;
            if (s.Materials.TryGetValue(type, out imo))
            {
                Color c = imo.GetColor();
                if (c.A < 1.0)
                    return true;
            }
            return false;
        }
    }
}
