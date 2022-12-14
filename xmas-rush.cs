using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace XMAS_RUSH
{

    enum TurnType
    {
        PUSH,
        MOVE
    }

    enum Direction
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    enum Orientation
    {
        Horizontal,
        Vertical
    }

    class Game
    {
        const int GameboardSize = 7;
        public string[,] Gameboard { get; private set; } = new string[GameboardSize, GameboardSize];
        public int[,] Graph { get; private set; }
        public Player[] Players { get; private set; } = new Player[2];
        public Player Player => Players[0];
        public Player Opponent => Players[1];
        public List<Item> Items { get; private set; } = new List<Item>();

        public static bool IsPositionOnEdge(int x, int y)
        {
            return x == 0 || x == 6 || y == 0 || y == 6;
        }

        public bool CanPlayerPlayQuestItem()
        {
            var items = Player.GetQuestItems();
            if (items.Any(item => item.X == -1 && item.Y == -1))
                return true;
            return false;
        }

        public bool CanOpponentPlayQuestItem()
        {
            var items = Opponent.GetQuestItems();
            if (items.Any(item => item.X == -2 && item.Y == -2))
                return true;
            return false;
        }

        public void InitGraph()
        {
            Graph = GetGraph(Gameboard);
        }
        
        public static int[,] GetGraph(string[,] gameboard)
        {
            var graph = new int[GameboardSize * GameboardSize, GameboardSize * GameboardSize];
            int linesNumber = GameboardSize;
            int columnsNumber = GameboardSize;
            for (int y = 0; y < linesNumber; y++)
            {
                for (int x = 0; x < columnsNumber; x++)
                {
                    string tile = gameboard[y, x];

                    foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    {
                        bool hasPath = HasPathToDirection(x, y, direction);

                        if (hasPath)
                        {
                            int destinationX = x;
                            int destinationY = y;
                            switch (direction)
                            {
                                case Direction.UP:
                                    destinationY--;
                                    break;
                                case Direction.RIGHT:
                                    destinationX++;
                                    break;
                                case Direction.DOWN:
                                    destinationY++;
                                    break;
                                case Direction.LEFT:
                                    destinationX--;
                                    break;
                                default:
                                    throw new Exception();
                            }

                            int srcPoint = y * linesNumber + x;
                            int dstPoint = destinationY * linesNumber + destinationX;
                            //Console.Error.WriteLine($"Path ({j},{i}) {srcPoint} => ({destinationColumnIndex},{destinationLineIndex}) {dstPoint} ({direction.ToString()})");
                            graph[srcPoint, dstPoint] = graph[dstPoint, srcPoint] = 1;
                        }
                    }
                }
            }
            
            return graph;
        }
        
        public string[,] GetGameboard(int id, Direction direction, string tile)
        {
            var gameboard = new string[GameboardSize, GameboardSize];
            for (int y = 0; y < GameboardSize; y++)
            {
                for (int x = 0; x < GameboardSize; x++)
                {
                    gameboard[y, x] = gameboard[y, x];
                }
            }
            
            if (direction == Direction.LEFT)
            {
                for (int i = 0; i < 6; i++)
                {
                    gameboard[id, i] = gameboard[id, i+1];
                }
                gameboard[id, 6] = tile;
            }
            else if (direction == Direction.UP)
            {
                for (int i = 0; i < 6; i++)
                {
                    gameboard[i, id] = gameboard[i+1, id];
                }
                gameboard[6, id] = tile;
            }
            else if (direction == Direction.RIGHT)
            {
                for (int i = 1; i < 7; i++)
                {
                    gameboard[id, i] = gameboard[id, i-1];
                }
                gameboard[id, 0] = tile;
            }
            else if (direction == Direction.DOWN)
            {
                for (int i = 1; i < 7; i++)
                {
                    gameboard[i, id] = gameboard[i-1, id];
                }
                gameboard[0, id] = tile;
            }
            
            return gameboard;
        }
        
        public int[,] GetGraph(int id, Direction direction, string tile)
        {
            var gameboard = GetGameboard(id, direction, tile);
            return GetGraph(gameboard);
        }

        private bool HasPathToDirection(int x, int y, Direction direction)
        {
            if (y == 0 && direction == Direction.UP
                || y == Gameboard.GetLength(0) - 1 && direction == Direction.DOWN
                || x == 0 && direction == Direction.LEFT
                || x == Gameboard.GetLength(1) - 1 && direction == Direction.RIGHT)
            {
                return false;
            }

            string source = Gameboard[y, x];
            int destinationLineIndex = y;
            int destinationColumnIndex = x;
            string destination;

            bool result = false;
            switch (direction)
            {
                case Direction.UP:
                    destinationLineIndex--;
                    destination = Gameboard[destinationLineIndex, destinationColumnIndex];
                    result = (int)char.GetNumericValue(source[0]) == 1 && (int)char.GetNumericValue(destination[2]) == 1;
                    break;
                case Direction.RIGHT:
                    destinationColumnIndex++;
                    destination = Gameboard[destinationLineIndex, destinationColumnIndex];
                    result = (int)char.GetNumericValue(source[1]) == 1 && (int)char.GetNumericValue(destination[3]) == 1;
                    break;
                case Direction.DOWN:
                    destinationLineIndex++;
                    destination = Gameboard[destinationLineIndex, destinationColumnIndex];
                    result = (int)char.GetNumericValue(source[2]) == 1 && (int)char.GetNumericValue(destination[0]) == 1;
                    break;
                case Direction.LEFT:
                    destinationColumnIndex--;
                    destination = Gameboard[destinationLineIndex, destinationColumnIndex];
                    result = (int)char.GetNumericValue(source[3]) == 1 && (int)char.GetNumericValue(destination[1]) == 1;
                    break;
                default:
                    throw new Exception();
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            //for (int i = 0; i < GameboardSize; i++)
            //{
            //    string line = string.Empty;
            //    for (int j = 0; j < GameboardSize; j++)
            //    {
            //        line += $"[{Gameboard[i, j]}]";
            //    }
            //    sb.AppendLine(line);
            //}
            //sb.AppendLine();

            //for (int src = 0; src < 49; src++)
            //{
            //    string line = string.Empty;
            //    for (int dst = 0; dst < 49; dst++)
            //    {
            //        line += $"[{Graph[src, dst]}]";
            //    }
            //    sb.AppendLine(line);
            //}
            //sb.AppendLine();

            sb.AppendLine(Player.ToString());
            sb.AppendLine(Opponent.ToString());

            return sb.ToString();
        }
    }

    class Quest
    {
        public Quest(Player player, string itemName)
        {
            Player = player;
            ItemName = itemName;
        }

        public Player Player { get; set; }
        public string ItemName { get; set; }

        public override string ToString()
        {
            return ItemName;
        }
    }

    class Item
    {
        public Item(int x, int y, string itemName, Player player)
        {
            X = x;
            Y = y;
            Player = player;
            ItemName = itemName;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string ItemName { get; set; }
        public Player Player { get; set; }

        public static implicit operator Point(Item item) => new Point(item.X, item.Y);
    }

    /**
     * Help the Christmas elves fetch presents in a magical labyrinth!
     **/
    class Player
    {
        public Player(Game game, int x, int y, int questNumber, string currentTile)
        {
            Game = game;
            X = x;
            Y = y;
            QuestNumber = questNumber;
            CurrentTile = currentTile;
        }

        public Game Game { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int QuestNumber { get; private set; }
        public string CurrentTile { get; private set; }
        public List<Quest> Quests { get; private set; } = new List<Quest>();

        public List<Item> GetQuestItems()
        {
            var questItemNames = Quests.Select(q => q.ItemName);
            return Game.Items.Where(item => questItemNames.Contains(item.ItemName) && item.Player == this).ToList();
        }

        public List<Item> GetRecoverableQuestItems()
        {
            var questItemNames = Quests.Select(q => q.ItemName);
            return Game.Items.Where(item => Game.IsPositionOnEdge(item.X, item.Y) && questItemNames.Contains(item.ItemName) && item.Player == this).ToList();
        }

        public bool CanRecoverQuestItem()
        {
            var questItems = GetQuestItems();
            return questItems.Any(item => Game.IsPositionOnEdge(item.X, item.Y));
        }

        public static implicit operator Point(Player item) => new Point(item.X, item.Y);

        public override string ToString()
        {
            return $"({X},{Y})" + "\tCarte jouable : " + CurrentTile;
        }
        
        public static Dictionary<Point, List<Point>> GetReachableItemsCount(Graph graph, Player player, List<Point> destinations)
        {
            var sourceProblem = new DijkstraProblem(graph, player.X + player.Y * 7);
            sourceProblem.Solve();

            Dictionary<Point, List<Point>> srcDico = new Dictionary<Point, List<Point>>();
            foreach (var destination in destinations)
            {
                if (destination.X < 0 || destination.Y < 0)
                    continue;

                List<Point> path;
                if (sourceProblem.HasPathTo(destination.X + destination.Y * 7))
                {
                    path = sourceProblem.GetPointsTo(destination.X, destination.Y, 7, 7);
                    srcDico.Add(destination, path);
                    Console.Error.WriteLine($"Chemin trouvé vers ({destination.X},{destination.Y})");
                }
                else
                {
                    Console.Error.WriteLine($"Pas de chemin trouvé vers ({destination.X},{destination.Y})");
                }
            }
            return srcDico;
        }
        
        public static List<Point> GetBestItemsDirections(Graph graph, Point source, List<Point> destinations)
        {
            var sourceProblem = new DijkstraProblem(graph, source.X + source.Y * 7);
            sourceProblem.Solve();

            Dictionary<Point, List<Point>> srcDico = new Dictionary<Point, List<Point>>();
            Dictionary<Point, Dictionary<Point, List<Point>>> dstDico = new Dictionary<Point, Dictionary<Point, List<Point>>>();
            foreach (var destination in destinations)
            {
                if (destination.X < 0 || destination.Y < 0)
                    continue;

                var destinationProblem = new DijkstraProblem(graph, destination.X + destination.Y * 7);
                destinationProblem.Solve();

                Dictionary<Point,List<Point>> otherDestinationPaths = new Dictionary<Point, List<Point>>();
                foreach (var otherDestination in destinations.Where(dst => dst != destination))
                {
                    if (otherDestination.X < 0 || otherDestination.Y < 0)
                        continue;

                    if (destinationProblem.HasPathTo(otherDestination.X + otherDestination.Y * 7))
                    {
                        otherDestinationPaths.Add(otherDestination, destinationProblem.GetPointsTo(otherDestination.X, otherDestination.Y, 7, 7));
                    }
                }
                dstDico.Add(destination, otherDestinationPaths);

                List<Point> path;
                if (sourceProblem.HasPathTo(destination.X + destination.Y * 7))
                {
                    path = sourceProblem.GetPointsTo(destination.X, destination.Y, 7, 7);
                    srcDico.Add(destination, path);
                    Console.Error.WriteLine($"Chemin trouvé vers ({destination.X},{destination.Y})");
                }
                else
                {
                    Console.Error.WriteLine($"Pas de chemin trouvé vers ({destination.X},{destination.Y})");
                }
            }

            if (srcDico.Count == 0)
                return new List<Direction>();

            List<List<List<Point>>> allPaths = new List<List<List<Point>>>();
            foreach (var item1 in srcDico)
            {
                List<List<Point>> paths1 = new List<List<Point>>();
                paths1.Add(item1.Value);
                allPaths.Add(paths1);

                Console.Error.WriteLine(item1.Key);
                if (dstDico.ContainsKey(item1.Key))
                {
                    foreach (var item2 in dstDico[item1.Key])
                    {
                        List<List<Point>> paths2 = new List<List<Point>>();
                        paths2.Add(item1.Value);
                        paths2.Add(item2.Value);
                        allPaths.Add(paths2);

                        Console.Error.WriteLine(item2.Key);
                        if (dstDico.ContainsKey(item2.Key))
                        {
                            foreach (var item3 in dstDico[item2.Key])
                            {
                                List<List<Point>> paths3 = new List<List<Point>>();
                                paths3.Add(item1.Value);
                                paths3.Add(item2.Value);
                                paths3.Add(item3.Value);
                                allPaths.Add(paths3);
                            }
                        }
                    }
                }
            }

            var recoverableItems = allPaths.Max(path => path.Count);
            var longerPaths = allPaths.Where(lp => lp.Count == recoverableItems);

            Console.Error.WriteLine($"Recovering {recoverableItems}");

            var minPathLength = longerPaths.Min(lp => lp.Sum(path => path.Count));
            var chosenPathList = longerPaths.FirstOrDefault(lp => lp.Sum(path => path.Count) == minPathLength);

            return chosenPathList.SelectMany(path => path).ToList();
        }

        public static string MoveToClosest(Graph graph, Point source, List<Point> destinations)
        {
            var dijkstraProblem = new DijkstraProblem(graph, source.X + source.Y * 7);
            dijkstraProblem.Solve();
            dijkstraProblem.PrintPredecessors();

            List<List<Point>> paths = new List<List<Point>>();
            foreach (var destination in destinations)
            {
                List<Point> path;
                Console.Error.WriteLine($"Recherche de chemin vers ({destination.X},{destination.Y})");
                try
                {
                    path = dijkstraProblem.GetPointsTo(destination.X, destination.Y, 7, 7);
                    paths.Add(path);
                }
                catch
                {
                    Console.Error.WriteLine($"Pas de chemin trouvé vers ({destination.X},{destination.Y})");
                }
            }

            if (paths.Count == 0)
                return Pass();

            var orderedPaths = paths.OrderBy(p => p.Count);

            var directions = PathToDirections(orderedPaths.First()).Take(20).ToList();
            if (directions.Count == 0)
                return Pass();

            return Move(directions);
        }

        public static string MoveToMaximumItems(Graph graph, Point source, List<Point> destinations)
        {
            var directions = GetBestItemsDirections(graph, source, List<Point> destinations);
            if (directions.Count == 0)
                return Pass();

            Console.Error.WriteLine(string.Join(" ", directions));

            return Move(directions);
        }

        private static IEnumerable<Direction> PathToDirections(IList<Point> points)
        {
            if (points.Count < 2)
                yield break;

            var previousPoint = points.First();
            foreach (var point in points.Skip(1))
            {
                if (point.X > previousPoint.X)
                    yield return Direction.RIGHT;
                else if (point.X < previousPoint.X)
                    yield return Direction.LEFT;
                else if (point.Y > previousPoint.Y)
                    yield return Direction.DOWN;
                else if (point.Y < previousPoint.Y)
                    yield return Direction.UP;

                previousPoint = point;
            }
        }

        public static string Move(IEnumerable<Direction> directions)
        {
            return "MOVE " + string.Join(" ", directions);
        }

        public static string PushToClosestEdge(int x, int y)
        {
            Console.Error.WriteLine($"Pushing ({x},{y}) to closest edge");
            string result;

            if (Game.IsPositionOnEdge(x, y))
            {
                result = PushToRecover(x, y);
            }
            else if (x < 6 - x && x < y && x < 6 - y
                || x == y && x < 3)
            {
                result = Push(y, Direction.LEFT);
            }
            else if (6 - x < x && 6 - x < y && 6 - x < 6 - y
                || x == y && x > 3)
            {
                result = Push(y, Direction.RIGHT);
            }
            else if (y < 6 - y && y < x && y < 6 - x)
            {
                result = Push(x, Direction.UP);
            }
            else if (6 - y < y && 6 - y < x && 6 - y < 6 - x)
            {
                result = Push(x, Direction.DOWN);
            }
            else
            {
                result = PushRandom();
            }

            return result;
        }

        public static string PushRandom()
        {
            Console.Error.WriteLine("Pushing random");
            Random rand = new Random();
            return Push(rand.Next(7), (Direction)rand.Next(4));
        }

        public static string PushToRecover(int x, int y)
        {
            Console.Error.WriteLine($"Recovering ({x},{y})");
            string result;

            if (x == 0)
                result = Push(y, Direction.LEFT);
            else if (x == 6)
                result = Push(y, Direction.RIGHT);
            else if (y == 0)
                result = Push(x, Direction.UP);
            else if (y == 6)
                result = Push(x, Direction.DOWN);
            else
                result = PushRandom();

            return result;
        }

        public static string Push(int id, Direction direction)
        {
            return "PUSH " + id + " " + direction;
        }

        public static string Pass()
        {
            return "PASS";
        }
        
        public static PushForBestMove(Game game)
        {
            for (int i = 0; i < 7; i++)
            {
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                {
                    var graph = game.GetGraph(i, direction, game.Player.CurrentTile);
                    
                }
            }
        }

        static void Main(string[] args)
        {
            Game game = new Game();

            string[] inputs;

            // game loop
            while (true)
            {
                TurnType turnType = (TurnType)int.Parse(Console.ReadLine());
                for (int i = 0; i < 7; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    for (int j = 0; j < 7; j++)
                    {
                        game.Gameboard[i, j] = inputs[j];
                    }
                }

                game.InitGraph();

                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int numPlayerCards = int.Parse(inputs[0]); // the total number of quests for a player (hidden and revealed)
                    int playerX = int.Parse(inputs[1]);
                    int playerY = int.Parse(inputs[2]);
                    string playerTile = inputs[3];
                    game.Players[i] = new Player(game, playerX, playerY, numPlayerCards, playerTile);
                }

                int numItems = int.Parse(Console.ReadLine()); // the total number of items available on board and on player tiles
                game.Items.Clear();
                for (int i = 0; i < numItems; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string itemName = inputs[0];
                    int itemX = int.Parse(inputs[1]);
                    int itemY = int.Parse(inputs[2]);
                    int itemPlayerId = int.Parse(inputs[3]);
                    game.Items.Add(new Item(itemX, itemY, itemName, game.Players[itemPlayerId]));
                }
                int numQuests = int.Parse(Console.ReadLine()); // the total number of revealed quests for both players
                for (int i = 0; i < numQuests; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string questItemName = inputs[0];
                    int questPlayerId = int.Parse(inputs[1]);
                    game.Players[questPlayerId].Quests.Add(new Quest(game.Players[questPlayerId], questItemName));
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                Console.Error.WriteLine(game);

                if (turnType == TurnType.MOVE)
                {
                    var questItems = game.Player.GetQuestItems();
                    var moveString = game.Player.MoveToMaximumItems(game.Graph, game.Player, questItems.Cast<Point>().ToList());
                    Console.WriteLine(moveString);
                }
                else if (turnType == TurnType.PUSH)
                {
                    if (game.CanPlayerPlayQuestItem() && Game.IsPositionOnEdge(game.Player.X, game.Player.Y))
                    {
                        Console.WriteLine(PushToClosestEdge(game.Player.X, game.Player.Y));
                    }
                    else if (game.Player.CanRecoverQuestItem())
                    {
                        var questItems = game.Player.GetRecoverableQuestItems();
                        var firstQuestItem = questItems.First();
                        Console.WriteLine(PushToRecover(firstQuestItem.X, firstQuestItem.Y));
                    }
                    else if (!game.CanPlayerPlayQuestItem())
                    {
                        var questItems = game.Player.GetQuestItems();
                        var firstQuestItem = questItems.First();
                        string pushString = PushToClosestEdge(firstQuestItem.X, firstQuestItem.Y);
                        Console.WriteLine(pushString);
                    }
                    else
                    {
                        string pushString = PushRandom();
                        Console.WriteLine(pushString);
                    }
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

            public List<Point> GetPointsTo(int destination, int nbLines, int nbColumns)
            {
                var res = GetPathTo(destination);

                return res.Select(p => new Point(p % nbColumns, p / nbLines)).ToList();
            }

            public List<Point> GetPointsTo(int destinationX, int destinationY, int nbLines, int nbColumns)
            {
                var res = GetPathTo(destinationX + nbLines * destinationY);

                return res.Select(p => new Point(p % nbColumns, p / nbLines)).ToList();
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
                    if (shortestPathTreeSet[v] == false && distance[v] != -1 && distance[v] <= min)
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
    }
}
