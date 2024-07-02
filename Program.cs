using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
    public static void Main()
    {
        Game game = new Game("JackSparrow", "BlackBeard", true);
        game.TurnStarted += OnTurnStarted;
        game.Shot += OnShot;
        game.ShotResult += OnShotResult;
        game.Start();
    }
    static void OnTurnStarted(string playerName)
    {
        Console.WriteLine($"EVENT: {playerName}'s turn started.");
    }

    static void OnShot(string playerName, int x, int y)
    {
        Console.WriteLine($"EVENT: {playerName} fired a shot at ({x}, {y}).");
    }

    static void OnShotResult(string playerName, bool hit)
    {
        Console.WriteLine(hit ? $"EVENT: {playerName} hit the target!" : $"{playerName} missed.");
    }
}

public enum CellState
{
    Empty,
    Ship,
    Hit,
    Miss
}

public class Ship
{
    public int Size { get; private set; }
    public List<(int, int)> Coordinates { get; private set; }

    public Ship(int size)
    {
        Size = size;
        Coordinates = new List<(int, int)>();
    }

    public void AddCoordinates(int x, int y)
    {
        Coordinates.Add((x, y));
    }

    public bool IsSunk(CellState[,] grid)
    {
        return Coordinates.All(coord => grid[coord.Item1, coord.Item2] == CellState.Hit);
    }
}

public class Deck
{
    private const int BoardSize = 10;
    private CellState[,] grid;
    public List<Ship> Ships { get; private set; }

    public Deck()
    {
        grid = new CellState[BoardSize, BoardSize];
        Ships = new List<Ship>();
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                grid[i, j] = CellState.Empty;
            }
        }
    }

    private void MarkAroundSunkShip(Ship ship)
    {
        foreach (var coord in ship.Coordinates)
        {
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    int newX = coord.Item1 + i;
                    int newY = coord.Item2 + j;
                    if (newX >= 0 && newX < BoardSize && newY >= 0 && newY < BoardSize)
                    {
                        if (grid[newX, newY] == CellState.Empty)
                        {
                            grid[newX, newY] = CellState.Miss;
                        }
                    }
                }
            }
        }
    }

    public void Display(bool hideShips = false)
    {
        Console.Write("  ");
        for (int i = 0; i < BoardSize; ++i)
            Console.Write(" " + i + " ");
        Console.WriteLine();

        for (int i = 0; i < BoardSize; ++i)
        {
            Console.Write(i + " ");
            for (int j = 0; j < BoardSize; ++j)
            {
                if (grid[i, j] == CellState.Empty)
                    Console.Write(" . ");
                else if (grid[i, j] == CellState.Ship)
                    Console.Write(hideShips ? " . " : " ■ ");
                else if (grid[i, j] == CellState.Hit)
                    {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" X ");
                    Console.ResetColor();
                    }
                else if (grid[i, j] == CellState.Miss)
                    {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.Write(" * ");
                    Console.ResetColor();
                }
            }
            Console.WriteLine();
        }
    }

    public bool CanPlaceShip(int x, int y, int size, bool horizontal)
    {
        if (horizontal)
        {
            if (y + size > BoardSize) return false;
            for (int i = 0; i < size; i++)
            {
                if (!IsValidPlacement(x, y + i)) return false;
            }
        }
        else
        {
            if (x + size > BoardSize) return false;
            for (int i = 0; i < size; i++)
            {
                if (!IsValidPlacement(x + i, y)) return false;
            }
        }
        return true;
    }

    private bool IsValidPlacement(int x, int y)
    {
        if (grid[x, y] != CellState.Empty) return false;

        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                int newX = x + i;
                int newY = y + j;
                if (newX >= 0 && newX < BoardSize && newY >= 0 && newY < BoardSize)
                {
                    if (grid[newX, newY] == CellState.Ship) return false;
                }
            }
        }
        return true;
    }

    public void PlaceShip(Ship ship, int x, int y, bool horizontal)
    {
        for (int i = 0; i < ship.Size; ++i)
        {
            if (horizontal)
            {
                grid[x, y + i] = CellState.Ship;
                ship.AddCoordinates(x, y + i);
            }
            else
            {
                grid[x + i, y] = CellState.Ship;
                ship.AddCoordinates(x + i, y);
            }
        }
        Ships.Add(ship);
    }

    public bool Shoot(int x, int y)
    {
        if (grid[x, y] == CellState.Ship)
        {
            grid[x, y] = CellState.Hit;
            foreach (var ship in Ships)
            {
                if (ship.Coordinates.Contains((x, y)))
                {
                    if (ship.IsSunk(grid))
                    {
                        MarkAroundSunkShip(ship);
                    }
                    break;
                }
            }
            return true;
        }
        else if (grid[x, y] == CellState.Empty)
        {
            grid[x, y] = CellState.Miss;
            return false;
        }

        return false;
    }

    public bool IsLost()
    {
        return Ships.All(ship => ship.IsSunk(grid));
    }

    public void Reset()
    {
        InitializeBoard();
        Ships.Clear();
    }
}

public class Player
{
    public string Name { get; private set; }
    public Deck Deck { get; private set; }

    public Player(string name)
    {
        Name = name;
        Deck = new Deck();
    }

    public void PlaceShips()
    {
        var shipSizes = new List<int> { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        foreach (var size in shipSizes)
        {
            bool placed = false;
            while (!placed)
            {
                int x = new Random().Next(10);
                int y = new Random().Next(10);
                bool horizontal = new Random().Next(2) == 0;

                if (Deck.CanPlaceShip(x, y, size, horizontal))
                {
                    var ship = new Ship(size);
                    Deck.PlaceShip(ship, x, y, horizontal);
                    placed = true;
                }
            }
        }
    }

    public void ResetShips()
    {
        Deck.Reset();
        PlaceShips();
    }

    public bool Shoot(Player enemy, int x, int y)
    {
        return enemy.Deck.Shoot(x, y);
    }
}

public class Game
{
    /////////////    ///            ///    ///////////     ///          ///     ///////////////
    public delegate void TurnStartedHandler(string playerName);         ///           ///
    public delegate void ShotHandler(string playerName, int x, int y);  ///           ///
    public delegate void ShotResultHandler(string playerName, bool hit);///           ///
    //////////          ////    ////       //////          ///  ///     ///           ///
    public event Action<string> TurnStarted;               ///    ///   ///           ///
    public event Action<string, int, int> Shot;            ///      /// ///           ///
    public event Action<string, bool> ShotResult;          ///        /////           ///
    ////////////////        ///            ///////////     ///          ///           /// 




    private Player player1;
    private Player player2;
    private bool humanVsComputer;



    
    public Game(string name1, string name2, bool hvsc)
    {
        player1 = new Player(name1);
        player2 = new Player(name2);
        humanVsComputer = hvsc;
    }

    private void Delay()
    {
        System.Threading.Thread.Sleep(200);
    }

    private void DisplayDecks()
    {
        Console.WriteLine($"{player1.Name}'s Deck: ");
        player1.Deck.Display();
        Console.WriteLine();

        Console.WriteLine($"{player2.Name}'s Deck: ");
        player2.Deck.Display(true);
        Console.WriteLine();
    }

    public void Start()
    {
        player1.PlaceShips();
        player2.PlaceShips();

        bool player1Turn = true;

        while (!player1.Deck.IsLost() && !player2.Deck.IsLost())
        {
            DisplayDecks();

            Player currentPlayer = player1Turn ? player1 : player2;
            Player enemy = player1Turn ? player2 : player1;

            TurnStarted?.Invoke(currentPlayer.Name); ///////////EVENT

            int x, y;
            bool hit = false;

            do
            {
                if (humanVsComputer && player1Turn)
                {
                    Console.WriteLine($"{currentPlayer.Name}'s turn. Enter coordinates to shoot (x y): ");
                    bool validInput = false;
                    x = y = -1;

                    while (!validInput)
                    {
                        var input = Console.ReadLine().Split();

                        if (input.Length == 2 && int.TryParse(input[0], out x) && int.TryParse(input[1], out y) && x >= 0 && x < 10 && y >= 0 && y < 10)
                        {
                            validInput = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid coordinates. Enter coordinates to shoot (x y): ");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{currentPlayer.Name}'s turn (random shot).");
                    Delay();
                    x = new Random().Next(10);
                    y = new Random().Next(10);

                }

                Shot?.Invoke(currentPlayer.Name, x, y);/////////EVENT


                hit = currentPlayer.Shoot(enemy, x, y);
                DisplayDecks();

                ShotResult?.Invoke(currentPlayer.Name, hit);////////////EVENT
                Console.WriteLine(hit ? "Hit!!!" : "Miss :(((");
            } while (hit);

            player1Turn = !player1Turn;

        }

        if (player1.Deck.IsLost())
        {
            Console.WriteLine($"{player2.Name} wins!!!");
        }
        else
        {
            Console.WriteLine($"{player1.Name} wins!!!");
        }
    }
}
