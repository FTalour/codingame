using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Tile {
    // owner : 1 = me, 0 = foe, -1 = neutral
    public int x, y, scrapAmount, owner, units;
    public bool recycler, canBuild, canSpawn, inRangeOfRecycler;

    public Tile(int x, int y, int scrapAmount, int owner, int units, bool recycler, bool canBuild, bool canSpawn,
            bool inRangeOfRecycler) {
        this.x = x;
        this.y = y;
        this.scrapAmount = scrapAmount;
        this.owner = owner;
        this.units = units;
        this.recycler = recycler;
        this.canBuild = canBuild;
        this.canSpawn = canSpawn;
        this.inRangeOfRecycler = inRangeOfRecycler;
    }
}

class World
{
    const int ME = 1;
    const int OPP = 0;
    const int NOONE = -1;

    public World(string[] inputs, int height, int width)
    {
        int myMatter = int.Parse(inputs[0]);
        int oppMatter = int.Parse(inputs[1]);
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                inputs = Console.ReadLine().Split(' ');
                int scrapAmount = int.Parse(inputs[0]);
                int owner = int.Parse(inputs[1]); 
                int units = int.Parse(inputs[2]);
                int recycler = int.Parse(inputs[3]);
                int canBuild = int.Parse(inputs[4]);
                int canSpawn = int.Parse(inputs[5]);
                int inRangeOfRecycler = int.Parse(inputs[6]);

                Tile tile = new Tile(
                        j,
                        i,
                        scrapAmount,
                        owner,
                        units,
                        recycler == 1,
                        canBuild == 1,
                        canSpawn == 1,
                        inRangeOfRecycler == 1);

                tiles.Add(tile);

                if (tile.owner == ME) 
                {
                    myTiles.Add(tile);
                    if (tile.units > 0) 
                    {
                        myUnits.Add(tile);
                    } 
                    else if (tile.recycler) 
                    {
                        myRecyclers.Add(tile);
                    }
                } 
                else if (tile.owner == OPP) 
                {
                    oppTiles.Add(tile);
                    if (tile.units > 0) 
                    {
                        oppUnits.Add(tile);
                    }
                    else if (tile.recycler) 
                    {
                        oppRecyclers.Add(tile);
                    }
                } 
                else 
                {
                    neutralTiles.Add(tile);
                }
            }
        }

        this.myMatter = myMatter;
        this.oppMatter = oppMatter;
        this.tiles = tiles;
        this.myTiles = myTiles;
        this.oppTiles = oppTiles;
        this.neutralTiles = neutralTiles;
        this.myUnits = myUnits;
        this.oppUnits = oppUnits;
        this.myRecyclers = myRecyclers;
        this.oppRecyclers = oppRecyclers;
    }

    public readonly int myMatter;
    public readonly int oppMatter;
    public readonly List<Tile> tiles = new List<Tile>();
    public readonly List<Tile> myTiles = new List<Tile>();
    public readonly List<Tile> oppTiles = new List<Tile>();
    public readonly List<Tile> neutralTiles = new List<Tile>();
    public readonly List<Tile> myUnits = new List<Tile>();
    public readonly List<Tile> oppUnits = new List<Tile>();
    public readonly List<Tile> myRecyclers = new List<Tile>();
    public readonly List<Tile> oppRecyclers = new List<Tile>();

    public Tile GetClosestOpponent(Tile tile)
    {
        if (oppUnits.Count == 0)
            return null;

        double min = oppUnits.Min(o => Math.Sqrt(Math.Pow(o.x - tile.x, 2) + Math.Pow(o.y - tile.y, 2)));
        return oppUnits.FirstOrDefault(o => Math.Sqrt(Math.Pow(o.x - tile.x, 2) + Math.Pow(o.y - tile.y, 2)) == min); 
    }

    public int GetMaxAmountBuilderMECanBuild()
    {
        return myMatter / 10;
    }
}

class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');

            World world = new World(inputs, height, width);

            List<String> actions = new List<String>();
            foreach (Tile tile in world.myTiles) 
            {
                if (tile.canSpawn)
                {
                    int amount = world.GetMaxAmountBuilderMECanBuild();
                    if (amount > 0)
                    {
                        actions.Add(Game.SPAWN(amount, tile));
                    }
                }
                if (tile.canBuild)
                {
                    bool shouldBuild = false;
                    if (shouldBuild)
                    {
                        actions.Add(Game.BUILD(tile));
                    }
                }
            }

            foreach (Tile tile in world.myUnits) 
            {
                // TODO: pick a destination
                Tile target = null;
                target = world.GetClosestOpponent(tile);
                if (target != null)
                {
                    int amount = tile.units; // Move all units from tile
                    actions.Add(Game.MOVE(amount, tile, target));
                }
            }

            if (actions.Count <= 0) 
            {
                Console.WriteLine(Game.WAIT());
            } 
            else 
            {
                Console.WriteLine(string.Join(";", actions.ToArray()));
            }
        }
    }
}

class Game 
{
    public static string WAIT() => "WAIT";
    public static string MOVE(int amount, Tile tile, Tile target) => String.Format("MOVE {0} {1} {2} {3} {4}", amount, tile.x, tile.y, target.x, target.y);
    public static string BUILD(Tile tile) => String.Format("BUILD {0} {1}", tile.x, tile.y);
    public static string SPAWN(int amount, Tile tile) => String.Format("SPAWN {0} {1} {2}", amount, tile.x, tile.y);

}