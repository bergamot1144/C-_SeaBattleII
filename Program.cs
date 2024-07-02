


Game game = new Game();
game.StartGame();





public class Board
{
    private const int BoardSize = 10;
    private char[,] grid;
    private static readonly Random rand = new Random();

    public Board()
    {
        grid = new char[BoardSize, BoardSize];
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                grid[i, j] = '·'; // символ воды
            }
        }
    }

    public bool CanPlaceShip(int x, int y, int length, bool isHorizontal)
    {
        // Проверка выхода за границы доски
        if (isHorizontal)
        {
            if (y + length > BoardSize)
                return false;
        }
        else // Вертикальное размещение
        {
            if (x + length > BoardSize)
                return false;
        }

        // Проверка наличия других кораблей в местоположении и вокруг будущего положения корабля
        for (int i = 0; i < length; i++)
        {
            int currX = isHorizontal ? x : x + i;
            int currY = isHorizontal ? y + i : y;

            if (grid[currX, currY] == '■')
                return false;

            // Проверка соседних ячеек
            if (HasAdjacentShip(currX, currY))
                return false;
        }

        return true;
    }

    private bool HasAdjacentShip(int x, int y)
    {
        // Проверка слева
        if (x > 0 && grid[x - 1, y] == '■')
            return true;

        // Проверка справа
        if (x < BoardSize - 1 && grid[x + 1, y] == '■')
            return true;

        // Проверка сверху
        if (y > 0 && grid[x, y - 1] == '■')
            return true;

        // Проверка снизу
        if (y < BoardSize - 1 && grid[x, y + 1] == '■')
            return true;

        // Проверка диагонали вверх-слева
        if (x > 0 && y > 0 && grid[x - 1, y - 1] == '■')
            return true;

        // Проверка диагонали вверх-справа
        if (x > 0 && y < BoardSize - 1 && grid[x - 1, y + 1] == '■')
            return true;

        // Проверка диагонали вниз-слева
        if (x < BoardSize - 1 && y > 0 && grid[x + 1, y - 1] == '■')
            return true;

        // Проверка диагонали вниз-справа
        if (x < BoardSize - 1 && y < BoardSize - 1 && grid[x + 1, y + 1] == '■')
            return true;

        return false;
    }


    public void PlaceShip(int x, int y, int length, bool isHorizontal)
    {
        for (int i = 0; i < length; i++)
        {
            if (isHorizontal)
            {
                grid[x, y + i] = '■'; // символ корабля
            }
            else
            {
                grid[x + i, y] = '■';
            }
        }
    }

    public void AutoPlaceAllShips()
    {
        int[] shipLengths = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        foreach (int length in shipLengths)
        {
            bool placed = false;
            while (!placed)
            {
                int x = rand.Next(BoardSize);
                int y = rand.Next(BoardSize);
                bool isHorizontal = rand.Next(2) == 0;

                if (CanPlaceShip(x, y, length, isHorizontal))
                {
                    PlaceShip(x, y, length, isHorizontal);
                    placed = true;
                }
            }
        }
    }

    public bool TakeShot(int x, int y)
    {
        if (grid[x, y] == '■')
        {
            grid[x, y] = 'X'; // символ попадания
            return true;
        }
        else if (grid[x, y] == '·')
        {
            grid[x, y] = '*'; // символ промаха
            return false;
        }

        return false;
    }

    public void DisplayBoard(bool isEnemyBoard, bool revealShips = false)
    {
        // Вывод букв по горизонтали
        Console.Write("  ");
        for (int i = 0; i < BoardSize; i++)
        {
            Console.Write(' ');
            Console.Write((char)('A' + i) + "");
        }
        Console.WriteLine();

        // Вывод доски с номерами строк по вертикали
        for (int i = 0; i < BoardSize; i++)
        {
            Console.Write((i + 1).ToString().PadLeft(2) + " "); // Вывод номера строки
            for (int j = 0; j < BoardSize; j++)
            {


                if (isEnemyBoard && grid[i, j] == 'R' && !revealShips)
                {
                    Console.Write("· ");
                }
                else
                {
                    Console.Write(grid[i, j] + " ");
                }
            }
            Console.WriteLine();
        }
    }
}



public class Game
{
    private Board playerBoard;
    private Board enemyBoard;

    public Game()
    {
        playerBoard = new Board();
        enemyBoard = new Board();
    }

    public void StartGame()
    {
        // Размещение кораблей
        //playerBoard.PlaceShip(1, 1, 4, true); // пример размещения корабля игрока
        //enemyBoard.PlaceShip(2, 2, 4, false); // пример размещения корабля врага

        enemyBoard.AutoPlaceAllShips();
        playerBoard.AutoPlaceAllShips();

        bool isPlayerTurn = true;
        bool gameWon = false;

        while (!gameWon)
        {
            Console.Clear();
            Console.WriteLine("Player's Board:");
            playerBoard.DisplayBoard(false,true);

            Console.WriteLine("\nEnemy's Board:");
            enemyBoard.DisplayBoard(true, false);

            if (isPlayerTurn)
            {
                Console.WriteLine("\nYour Turn!");
                TakePlayerTurn();
            }
            else
            {
                Console.WriteLine("\nEnemy's Turn!");
                TakeEnemyTurn();
            }

            gameWon = CheckForWin();
            isPlayerTurn = !isPlayerTurn;
        }

        Console.WriteLine(gameWon ? "You won!" : "Enemy won!");
    }

    private void TakePlayerTurn()
    {
        Console.Write("Enter X coordinate for your shot: ");
        int x = int.Parse(Console.ReadLine());
        Console.Write("Enter Y coordinate for your shot: ");
        int y = int.Parse(Console.ReadLine());

        bool hit = enemyBoard.TakeShot(x, y);
        Console.WriteLine(hit ? "Hit!" : "Miss!");
    }

    private void TakeEnemyTurn()
    {
        Random rand = new Random();
        int x = rand.Next(1, 10);
        int y = rand.Next(1, 10);

        bool hit = playerBoard.TakeShot(x, y);
        Console.WriteLine(hit ? "Enemy hit your ship!" : "Enemy missed!");
    }

    private bool CheckForWin()
    {
        // Проверка на победу (например, все ли корабли потоплены)
        // Для простоты здесь возвращается false
        return false;
    }
}
