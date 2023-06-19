using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Cell 
{
    public int Index {get;}
    public int Type {get;}
    public int InitialResources {get;}
    public IReadOnlyList<int> Neighbours { get; }
 
    public Cell(int index, int type, int initialResources, int neigh0, int neigh1, int neigh2, int neigh3, int neigh4, int neigh5)
    {
        Index = index;
        Type = type;
        InitialResources = initialResources;

        Neighbours= new List<int> () { neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };
    }
    
    public int Resources {get; set;}
    public int MyAnts {get; set;}
    public int OppAnts {get; set;}
    
}

class World : List<Cell>
{
    private int[,] _graph;
    public World(IEnumerable<Cell> cells) : base(cells)
    {
        _graph = GetGraph();
    }

    public List<Base> MyBases { get; } = new List<Base>();
    public List<Base> OppBases { get; } = new List<Base>();

    public int[,] GetGraph()
    {
        int numberOfCells = this.Count;
        int[,] graph = new int[numberOfCells,numberOfCells];
        foreach (var cell in this)
        {
            graph[cell.Index, cell.Index] = 0;

            foreach (var neigh in cell.Neighbours)
            {
                if (neigh != -1)
                {
                    graph[cell.Index, neigh] = 1;
                }
            }
        }
        return graph;
    }

    public (Cell, int) GetClosestEgg()
    {
        var eggs = this.Where(c => c.Type == 1 && c.Resources > 0);

        if (eggs.Any())
        {
            return GetClosestCell(_graph, MyBases.First().Index, eggs.Select(c => c.Index));
        }
        else
        {
            return (null, -1);
        }
    }

    public List<(Cell, int)> GetCrystalsDistance()
    {
        var crystals = this.Where(c => c.Type == 2 && c.Resources > 0);

        var res = new List<(Cell, int)>();
        
        foreach(var c in crystals)
        {
            res.Add((c, GetPath(_graph, MyBases.First().Index, c.Index).Count));
        }
        return res;
        
    }

    public (Cell, int) GetClosestCrystal()
    {
        var crystals = this.Where(c => c.Type == 2 && c.Resources > 0);

        if (crystals.Any())
        {
            return GetClosestCell(_graph, MyBases.First().Index, crystals.Select(c => c.Index));
        }
        else
        {
            return (null, -1);
        }
    }

    
    // public List<(int, int)> GetPaths()
    // {
    //     var crystals = this.Where(c => c.Type == 2 && c.Resources > 0);

    //     if (crystals.Any())
    //     {
    //         return GetClosestCell(_graph, MyBases.First().Index, crystals.Select(c => c.Index));
    //     }
    //     else
    //     {
    //         return (null, -1);
    //     }
    // }

    private List<int> GetPath(int[,] graph, int originIndex, int destinationIndex)
    {
        DijkstraProblem problem = new DijkstraProblem(graph, originIndex, destinationIndex);
        problem.Solve();
        return problem.GetPathToDestination();
    }

    private (Cell, int) GetClosestCell(int[,] graph, int originIndex, IEnumerable<int> destinationIndexes)
    {
        int minDist = -1;
        Cell minDistCell = null;

        foreach (var destinationIndex in destinationIndexes)
        {
            List<int> path = GetPath(graph, originIndex, destinationIndex);
            
            //Console.Error.WriteLine(string.Join(",", path));
            
            if (minDist == -1 || path.Count < minDist)
            {
                minDist = path.Count;
                minDistCell = this[destinationIndex];
            }
            //Console.Error.WriteLine($"Chemin {originIndex}->{destinationIndex} : {path.Count}");
        }

        return (minDistCell, minDist - 1);
    }
}

class Base 
{
    public int Index {get;}
    public Base (int index)
    {
        Index = index;
    }
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static World _world;

    static void Main(string[] args)
    {
        string[] inputs;
        int numberOfCells = int.Parse(Console.ReadLine()); // amount of hexagonal cells in this map
        List<Cell> cells = new List<Cell>();
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int type = int.Parse(inputs[0]); // 0 for empty, 1 for eggs, 2 for crystal
            int initialResources = int.Parse(inputs[1]); // the initial amount of eggs/crystals on this cell
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);
            cells.Add(new Cell(i, type, initialResources, neigh0, neigh1, neigh2, neigh3, neigh4, neigh5));
        }
        _world = new World(cells);
        int numberOfBases = int.Parse(Console.ReadLine());
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int myBaseIndex = int.Parse(inputs[i]);
            _world.MyBases.Add(new Base(myBaseIndex));
        }
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int oppBaseIndex = int.Parse(inputs[i]);
            _world.OppBases.Add(new Base(oppBaseIndex));
        }
     
        int turnNumber = 0;
        // game loop
        while (true)
        {   
            turnNumber++;

            for (int i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int resources = int.Parse(inputs[0]); // the current amount of eggs/crystals on this cell
                int myAnts = int.Parse(inputs[1]); // the amount of your ants on this cell
                int oppAnts = int.Parse(inputs[2]); // the amount of opponent ants on this cell
                _world[i].Resources = resources;
                _world[i].MyAnts = myAnts;
                _world[i].OppAnts = oppAnts;
            }

            List<string> turnActions = new List<string>();

            var crystalsDistance = _world.GetCrystalsDistance();
            var maxCrystalDistance = crystalsDistance.Max(d => d.Item2);
            foreach (Base myBase in _world.MyBases)
            {
                foreach(var crystalDistance in crystalsDistance.OrderByDescending(d => d.Item2))
                {
                    Console.Error.WriteLine((float)((maxCrystalDistance - crystalDistance.Item2) + 1) / maxCrystalDistance * 100);
                    turnActions.Add(LineAction(myBase.Index, crystalDistance.Item1.Index, (int)((float)((maxCrystalDistance - crystalDistance.Item2) + 1) / maxCrystalDistance * 100)));
                }
            }

            var (closetEgg, distEgg) = _world.GetClosestEgg();
            foreach (Base myBase in _world.MyBases)
            {
                if (closetEgg != null)
                {
                    turnActions.Add(LineAction(myBase.Index, closetEgg.Index, (int)((float)((maxCrystalDistance - crystalsDistance.Average(d => d.Item2)) + 1) / maxCrystalDistance * 100)));
                }
            }

           
            Console.Error.WriteLine("Actions :");
            Console.Error.WriteLine(string.Join(";", turnActions));

            if (turnActions.Count == 0)
            {
                Console.WriteLine(WaitAction());
            }
            else
            {
                Console.WriteLine(string.Join(";", turnActions));
            }

        }

        static string WaitAction()
        {
            return "WAIT";
        }
        
        static string LineAction(int sourceIdx, int targetIdx, int strength)
        {
            return $"LINE {sourceIdx} {targetIdx} {strength}";
        }
        
        static string BeaconAction(int cellIdx, int strength)
        {
            return $"BEACON {cellIdx} {strength}";
        }
        
        static string MessageAction(string text)
        {
            return text;
        }
    }
}

public class DijkstraProblem
{
    readonly int[,] _graph;
    readonly int _verticesCount; // Le nombre de noeuds
    readonly int[] _distance;
    readonly int?[] _predecessors;
    readonly bool[] _shortestPathTreeSet;
    int _source;
    bool _isStopingAtSolvedDestination;
    int? _destination;
    bool _isSolvedForDestination;
    bool _isFullySolved;

    public DijkstraProblem(int[,] graph, int source)
    {
        _verticesCount = graph.GetLength(1);
        _graph = graph;
        _source = source;
        _distance = new int[_verticesCount];
        _predecessors = new int?[_verticesCount];
        _shortestPathTreeSet = new bool[_verticesCount];
    }

    public DijkstraProblem(int[,] graph, int source, int destination, bool stopAtDestinationSolved = true)
    {
        _verticesCount = graph.GetLength(1);
        _graph = graph;
        _source = source;
        _destination = destination;
        _isStopingAtSolvedDestination = stopAtDestinationSolved;
        _distance = new int[_verticesCount];
        _predecessors = new int?[_verticesCount];
        _shortestPathTreeSet = new bool[_verticesCount];
    }

    private void Reset()
    {
        // Marque tous les noeuds comme non visités et ayant une distance infinie
        for (int i = 0; i < _verticesCount; ++i)
        {
            _distance[i] = int.MaxValue;
            _predecessors[i] = null;
            _shortestPathTreeSet[i] = false;
        }

        // Marque la source comme n'ayant aucune distance
        _distance[_source] = 0;
        _predecessors[_source] = _source;
        _isFullySolved = false;
    }

    public void Solve()
    {
        Reset();

        // Pour chaque noeud sauf le dernier (calculé automatiquement)
        for (int count = 0; count < _verticesCount - 1; ++count)
        {
            // Obtenir un noeud non visité
            int u = MinimumDistance(_distance, _shortestPathTreeSet);

            // Le noeud est considéré visité par la boucle suivante
            _shortestPathTreeSet[u] = true;

            // Pour chaque noeud
            for (int v = 0; v < _verticesCount; ++v)
            {
                // Si le noeud n'a pas été visité
                // Et le noeud visité est atteignable
                // Et la distance vers le noeud n'est pas infinie
                // Et la distance vers le noeud est plus courte que la valeur actuelle
                if (!_shortestPathTreeSet[v] && Convert.ToBoolean(_graph[u, v]) && _distance[u] != int.MaxValue && _distance[u] + _graph[u, v] < _distance[v])
                {
                    // Actualise la valeur de distance avec la nouvelle distance
                    _distance[v] = _distance[u] + _graph[u, v];

                    // Retient le chemin le plus court par la methode des prédécesseurs
                    _predecessors[v] = u;
                }
            }
            if (_isStopingAtSolvedDestination && u == _destination)
            {
                _isSolvedForDestination = true;
                return;
            }
        }
        _isSolvedForDestination = _isFullySolved = true;
    }

    public List<int> GetPathToDestination()
    {
        if (!_destination.HasValue)
            throw new Exception("La destination n'a pas été définie");

        return GetPathTo(_destination.Value);
    }

    public bool HasPathTo(int destination)
    {
        return _predecessors[destination] != null;
    }

    public List<int> GetPathTo(int destination)
    {
        if (destination == _destination && !_isSolvedForDestination
            || destination != _destination && !_isFullySolved)
            Solve();

        List<int> result = new List<int> { destination };

        int? _pred = _predecessors[destination];
        while (_pred.HasValue && _pred.Value != _source)
        {
            result.Add(_pred.Value);
            _pred = _predecessors[_pred.Value];
        }

        if (!_pred.HasValue)
        {
            throw new Exception("Aucun chemin vers la destination indiqué n'est possible");
        }

        result.Add(_source);
        result.Reverse();

        return result;
    }

    public int GetDistanceTo(int destination)
    {
        return _distance[destination];
    }

    /// <summary>
    /// Retourne l'indice du noeud le plus proche parmi ceux qui n'ont pas été visité
    /// </summary>
    /// <param name="distance">Les distances entre le noeud et ses voisins</param>
    /// <param name="shortestPathTreeSet">Liste des noeuds déjà visités</param>
    /// <returns>L'indice du noeud le plus proche</returns>
    private static int MinimumDistance(int[] distance, bool[] shortestPathTreeSet)
    {
        var verticesCount = distance.Length;
        int min = int.MaxValue;
        int minIndex = 0;

        for (int v = 0; v < verticesCount; ++v)
        {
            if (shortestPathTreeSet[v] == false && distance[v] != int.MaxValue && distance[v] <= min)
            {
                min = distance[v];
                minIndex = v;
            }
        }

        return minIndex;
    }

    /// <summary>
    /// Surpprime les boucles sur les noeuds du graph
    /// </summary>
    public void RemoveLoops()
    {
        for (int i = 0; i < _verticesCount; i++)
        {
            _graph[i, i] = 0;
        }
    }

    public void PrintPredecessors()
    {
        for (int i = 0; i < _predecessors.Length; i++)
        {
            Console.Error.WriteLine($"{i}\t({i % 7},{i / 7})\t{_predecessors[i]}");
        }
    }
}