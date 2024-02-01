namespace RPG
{
    using System.Collections.Generic;

    using Data;
    using Enums;
    using Heroes;
    using Players;
    using Data.Models;

    public class Game
    {
        private Screen currentScreen;
        private string[,] matrix = new string[10, 10];
        private BasePlayer player;
        private Monster monster;
        private Dictionary<int, List<int>> monstersToMove;
        private Dictionary<Dictionary<int, int>, Monster> monstersPosition;
        private Dictionary<int, Dictionary<int, int>> monstersInRange;

        public Game()
        {
            currentScreen = Screen.MainMenu;
            monster = new Monster();
            monstersToMove = new Dictionary<int, List<int>>();
            monstersPosition = new Dictionary<Dictionary<int, int>, Monster>();
            monstersInRange = new Dictionary<int, Dictionary<int, int>>();
        }

        public void Start(DataContext context)
        {
            if (currentScreen == Screen.MainMenu)
            {
                DisplayMainMenu();
            }
            if (currentScreen == Screen.CharacterSelect)
            {
                SelectCharacter();

                Player newPlayer = new Player()
                {
                    Agility = player.Agility,
                    Damage = player.Damage,
                    Health = player.Health,
                    Intelligence = player.Intelligence,
                    Mana = player.Mana,
                    Name = player.GetType().Name,
                };

                context.Players.Add(newPlayer);
                context.SaveChanges();
            }
            if (currentScreen == Screen.InGame)
            {
                PlayGame();
            }
            if (currentScreen == Screen.Exit)
            {
                ExitGame();
            }
        }

        private void DisplayMainMenu()
        {
            Console.WriteLine("Welcome!");
            Console.WriteLine("Press any key to play.");
            Console.ReadLine();

            currentScreen = Screen.CharacterSelect;
        }

        private void SelectCharacter()
        {
            while (true)
            {
                Console.WriteLine("Choose character type:");
                Console.WriteLine("Options:");
                Console.WriteLine($"1) Warrior");
                Console.WriteLine($"2) Archer");
                Console.WriteLine($"3) Mage");
                Console.Write("Your pick: ");

                bool result = int.TryParse(Console.ReadLine(), out int heroNumber);

                if (result)
                {
                    if (heroNumber == 1)
                    {
                        player = new Warrior();
                        break;
                    }
                    else if (heroNumber == 2)
                    {
                        player = new Archer();
                        break;
                    }
                    else if (heroNumber == 3)
                    {
                        player = new Mage();
                        break;
                    }
                }
            }

            while (true)
            {
                Console.WriteLine("Would you like to buff up your stats before starting ?        (Limit: 3 points total)");
                Console.Write(@"Response(Y\N): ");
                string response = Console.ReadLine();

                if (response.ToUpper() == "Y")
                {
                    int totalPoints = 3;
                    while (true)
                    {
                        Console.WriteLine($"Remaining Points: {totalPoints}");
                        Console.Write("Add to Strength: ");

                        bool result =
                            int.TryParse(Console.ReadLine(), out int strengthPoints);

                        if (!result)
                        {
                            continue;
                        }

                        if (strengthPoints >= 0 && strengthPoints <= totalPoints)
                        {
                            totalPoints -= strengthPoints;
                            player.Strength += strengthPoints;
                            break;
                        }
                    }

                    if (totalPoints == 0)
                    {
                        break;
                    }

                    while (true)
                    {
                        Console.WriteLine($"Remaining Points: {totalPoints}");
                        Console.Write("Add to Agility: ");

                        bool result =
                            int.TryParse(Console.ReadLine(), out int agilityPoints);

                        if (!result)
                        {
                            continue;
                        }

                        if (agilityPoints >= 0 && agilityPoints <= totalPoints)
                        {
                            totalPoints -= agilityPoints;
                            player.Agility += agilityPoints;
                            break;
                        }
                    }

                    if (totalPoints == 0)
                    {
                        break;
                    }

                    while (true)
                    {
                        Console.WriteLine($"Remaining Points: {totalPoints}");
                        Console.Write("Add to Intelligence: ");

                        bool result = 
                            int.TryParse(Console.ReadLine(), out int intelligencePoints);

                        if (!result)
                        {
                            continue;
                        }

                        if (intelligencePoints >= 0 && intelligencePoints <= totalPoints)
                        {
                            player.Intelligence += intelligencePoints;
                            break;
                        }
                    }

                    break;
                }
                else if (response.ToUpper() == "N")
                {
                    break;
                }
            }

            player.Setup();

            currentScreen = Screen.InGame;
        }

        private void PlayGame()
        {
            FillMatrix();
            matrix[0, 0] = player.ToString();

            int playerRow = 0;
            int playerCol = 0;
            string shadedBlock = "\u2592";

            while (player.Health > 0)
            {
                Console.Write($"Health: {player.Health}       ");
                Console.WriteLine($"Mana: {player.Mana}");
                Console.WriteLine();


                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        Console.Write(matrix[j, i]);
                    }
                    Console.WriteLine();
                }

                monster = new Monster();
                monster.Setup();
                var spawnMonsterCoordinates = SpawnMonster(shadedBlock);

                while (true)
                {
                    Console.WriteLine("Choose action");
                    Console.WriteLine("1) Attack");
                    Console.WriteLine("2) Move");

                    bool result = int.TryParse(Console.ReadLine(), out int command);

                    if (!result)
                    {
                        continue;
                    }

                    if (command == 1)
                    {
                        CheckRangeForMonsters(playerRow, playerCol, shadedBlock);
                        MoveMonster(playerRow, playerCol,
                            shadedBlock, spawnMonsterCoordinates);
                        break;
                    }
                    else if (command == 2)
                    {
                        bool canMove = SwitchPlayerPosition(ref playerRow,
                           ref playerCol,
                           shadedBlock);
                        if (!canMove)
                        {
                            Console.WriteLine("You cannot move there!");
                        }
                        MoveMonster(playerRow, playerCol,
                            shadedBlock, spawnMonsterCoordinates);
                        break;
                    }
                }
            }

            currentScreen = Screen.Exit;
        }

        private void FillMatrix()
        {
            string shadedBlock = "\u2592";

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[j, i] = shadedBlock;
                }
            }
        }

        private bool IsIndexValid(int playerRow, int playerCol)
            => playerRow >= 0 && playerRow < matrix.GetLength(0)
            && playerCol >= 0 && playerCol < matrix.GetLength(1);

        private bool SwitchPlayerPosition(
            ref int playerRow,
            ref int playerCol,
            string shadedBlock)
        {
            bool result =
                char.TryParse(Console.ReadLine().ToUpper(), out char newPosition);

            int playerRange = player.Range;
            int counterFreeCoordinates = 0;
            bool canMove = false;

            switch (newPosition)
            {
                case 'W':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow, playerCol - i)
                            && matrix[playerRow, playerCol - i] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[playerRow, --playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'S':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow, playerCol + i)
                            && matrix[playerRow, playerCol + i] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[playerRow, ++playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'D':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow + i, playerCol)
                            && matrix[playerRow + i, playerCol] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[++playerRow, playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'A':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow - i, playerCol)
                            && matrix[playerRow - i, playerCol] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[--playerRow, playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'E':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow + i, playerCol - i)
                            && matrix[playerRow + i, playerCol - i] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[++playerRow, --playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'X':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow + i, playerCol + i)
                            && matrix[playerRow + i, playerCol + i] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[++playerRow, ++playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'Q':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow - i, playerCol - i)
                            && matrix[playerRow - i, playerCol - i] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;
                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[--playerRow, --playerCol]
                                = player.ToString();
                        }
                    }
                    break;
                case 'Z':
                    for (int i = 1; i <= playerRange; i++)
                    {
                        if (IsIndexValid(playerRow - i, playerCol + i)
                            && matrix[playerRow - i, playerCol + i] == shadedBlock)
                        {
                            counterFreeCoordinates++;
                        }
                    }
                    if (counterFreeCoordinates == playerRange)
                    {
                        canMove = true;

                        for (int i = 0; i < playerRange; i++)
                        {
                            matrix[playerRow, playerCol] = shadedBlock;
                            matrix[--playerRow, ++playerCol]
                                = player.ToString();
                        }
                    }
                    break;
            }

            return canMove;
        }

        private Dictionary<int, int> SpawnMonster(string shadedBlock)
        {
            while (true)
            {
                int spawnMonsterRow = new Random().Next(1, 10);
                int spawnMonsterCol = new Random().Next(1, 10);

                if (matrix[spawnMonsterRow, spawnMonsterCol] == shadedBlock)
                {
                    matrix[spawnMonsterRow, spawnMonsterCol] = monster.ToString();
                    return new Dictionary<int, int>()
                    {
                        {
                            spawnMonsterRow,
                            spawnMonsterCol
                        }
                    };
                }
            }
        }

        private void MoveMonster(
            int playerRow,
            int playerCol,
            string shadedBlock,
            Dictionary<int, int> spawnMonsterCoordinates)
        {
            monstersToMove = new Dictionary<int, List<int>>();

            var coordinates = new Dictionary<int, int>()
            {
                {
                    spawnMonsterCoordinates.Keys.First(),
                    spawnMonsterCoordinates.Values.First()
                }
           };
            monstersPosition.Add(coordinates, monster);

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == monster.ToString())
                    {
                        var oldMonsterCoordinates =
                                    new Dictionary<int, int>
                                    {
                                        { i, j }
                                    };

                        if (playerRow == i || playerCol == j)
                        {
                            if (playerRow + 1 == i || playerRow - 1 == i
                             || playerCol + 1 == j || playerCol - 1 == j)
                            {

                                foreach (var kvp in monstersPosition)
                                {
                                    if (kvp.Key.First().Key == oldMonsterCoordinates.Keys.First()
                                        && kvp.Key.First().Value == oldMonsterCoordinates.Values.First())
                                    {
                                        player.Health -= monstersPosition[kvp.Key].Damage;
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                        var key = new Dictionary<int, int>();
                        var oldMonster = new Monster();
                        bool isExist = false;

                        foreach (var kvp in monstersPosition)
                        {
                            foreach (var monsterKey in kvp.Key)
                            {
                                if (monsterKey.Key == oldMonsterCoordinates.Keys.First()
                                && monsterKey.Value == oldMonsterCoordinates.Values.First())
                                {
                                    isExist = true;
                                }
                            }
                        }

                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == oldMonsterCoordinates.Keys.First()
                                && kvp.Key.First().Value == oldMonsterCoordinates.Values.First())
                            {
                                oldMonster = monstersPosition[kvp.Key];
                                monstersPosition.Remove(kvp.Key);
                            }
                        }

                        matrix[i, j] = shadedBlock;

                        if (playerCol == j)
                        {
                            if (playerRow >= i)
                            {
                                if (!monstersToMove.ContainsKey(i + 1))
                                {
                                    monstersToMove[i + 1] = new List<int>();
                                }
                                monstersToMove[i + 1].Add(j);

                                key.Add(i + 1, j);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }

                                monstersPosition[key] = oldMonster;
                            }
                            else
                            {
                                if (!monstersToMove.ContainsKey(i - 1))
                                {
                                    monstersToMove[i - 1] = new List<int>();
                                }
                                monstersToMove[i - 1].Add(j);

                                key.Add(i - 1, j);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }
                                monstersPosition[key] = oldMonster;
                            }
                        }
                        else if (playerRow == i)
                        {
                            if (playerCol >= j)
                            {
                                if (!monstersToMove.ContainsKey(i))
                                {
                                    monstersToMove[i] = new List<int>();
                                }
                                monstersToMove[i].Add(j + 1);

                                key.Add(i, j + 1);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }
                                monstersPosition[key] = oldMonster;
                            }
                            else
                            {
                                if (!monstersToMove.ContainsKey(i))
                                {
                                    monstersToMove[i] = new List<int>();
                                }
                                monstersToMove[i].Add(j - 1);

                                key.Add(i, j - 1);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }
                                monstersPosition[key] = oldMonster;
                            }
                        }
                        else
                        {
                            if (playerRow >= i)
                            {
                                if (!monstersToMove.ContainsKey(i + 1))
                                {
                                    monstersToMove[i + 1] = new List<int>();
                                }
                                monstersToMove[i + 1].Add(j);


                                key.Add(i + 1, j);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }
                                monstersPosition[key] = oldMonster;
                            }
                            else if (playerRow < i)
                            {
                                if (!monstersToMove.ContainsKey(i - 1))
                                {
                                    monstersToMove[i - 1] = new List<int>();
                                }
                                monstersToMove[i - 1].Add(j);

                                key.Add(i - 1, j);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }

                                monstersPosition[key] = oldMonster;
                            }
                            else if (playerCol >= i)
                            {
                                if (!monstersToMove.ContainsKey(i))
                                {
                                    monstersToMove[i] = new List<int>();
                                }
                                monstersToMove[i].Add(j + 1);

                                key.Add(i, j + 1);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }
                                monstersPosition[key] = oldMonster;
                            }
                            else
                            {
                                if (!monstersToMove.ContainsKey(i))
                                {
                                    monstersToMove[i] = new List<int>();
                                }
                                monstersToMove[i].Add(j - 1);

                                key.Add(i, j - 1);

                                if (!isExist)
                                {
                                    monstersPosition[key] = monster;
                                    continue;
                                }
                                monstersPosition[key] = oldMonster;
                            }
                        }
                    }
                }
            }

            foreach (var monsterCoordinates in monstersToMove)
            {
                foreach (var values in monsterCoordinates.Value)
                {
                    matrix[monsterCoordinates.Key, values] = monster.ToString();
                }
            }
        }

        private void CheckRangeForMonsters(int playerRow, int playerCol,
            string shadedBlock)
        {
            int playerRange = player.Range;
            int counter = 0;

            for (int i = playerRow - playerRange; i <= playerRow + playerRange; i++)
            {
                if (IsIndexValid(i, playerCol))
                {
                    if (matrix[i, playerCol] == monster.ToString())
                    {
                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == i
                                && kvp.Key.First().Value == playerCol)
                            {
                                var monster = monstersPosition[kvp.Key];
                                var monsterPosition = new Dictionary<int, int>()
                                {
                                    {i, playerCol}
                                };
                                monstersInRange[counter] = (monsterPosition);
                                Console.WriteLine($"{counter++}) target with remaining blood {monster.Health}");
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = playerCol - playerRange; i <= playerCol + playerRange; i++)
            {
                if (IsIndexValid(playerRow, i))
                {
                    if (matrix[playerRow, i] == monster.ToString())
                    {
                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == playerRow
                                && kvp.Key.First().Value == i)
                            {
                                var monster = monstersPosition[kvp.Key];
                                var monsterPosition = new Dictionary<int, int>()
                                {
                                    {playerRow,i}
                                };
                                monstersInRange[counter] = (monsterPosition);
                                Console.WriteLine($"{counter++}) target with remaining blood {monster.Health}");
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 1; i <= playerRange; i++)
            {
                if (IsIndexValid(playerCol + i, playerRow - i))
                {
                    if (matrix[playerRow - i, playerCol + i] == monster.ToString())
                    {
                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == playerRow - i
                                && kvp.Key.First().Value == playerCol + i)
                            {
                                var monster = monstersPosition[kvp.Key];
                                var monsterPosition = new Dictionary<int, int>()
                                {
                                    {playerCol + i, playerRow - i}
                                };
                                monstersInRange[counter] = (monsterPosition);
                                Console.WriteLine($"{counter++}) target with remaining blood {monster.Health}");
                                break;
                            }
                        }
                    }
                }

                if (IsIndexValid(playerCol - i, playerRow + i))
                {
                    if (matrix[playerRow + i, playerCol - i] == monster.ToString())
                    {
                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == playerRow + i
                                && kvp.Key.First().Value == playerCol - i)
                            {
                                var monster = monstersPosition[kvp.Key];
                                var monsterPosition = new Dictionary<int, int>()
                                {
                                    {playerCol - i, playerRow + i}
                                };
                                monstersInRange[counter] = (monsterPosition);
                                Console.WriteLine($"{counter++}) target with remaining blood {monster.Health}");
                                break;
                            }
                        }
                    }
                }

                if (IsIndexValid(playerRow - i, playerCol - i))
                {
                    if (matrix[playerRow - i, playerCol - i] == monster.ToString())
                    {
                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == playerRow - i
                                && kvp.Key.First().Value == playerCol - i)
                            {
                                var monster = monstersPosition[kvp.Key];
                                var monsterPosition = new Dictionary<int, int>()
                                {
                                     {playerRow - i, playerCol - i}
                                };
                                monstersInRange[counter] = (monsterPosition);
                                Console.WriteLine($"{counter++}) target with remaining blood {monster.Health}");
                                break;
                            }
                        }
                    }
                }

                if (IsIndexValid(playerRow + i, playerCol + i))
                {
                    if (matrix[playerRow + i, playerCol + i] == monster.ToString())
                    {
                        foreach (var kvp in monstersPosition)
                        {
                            if (kvp.Key.First().Key == playerRow + i
                                && kvp.Key.First().Value == playerCol + i)
                            {
                                var monster = monstersPosition[kvp.Key];
                                var monsterPosition = new Dictionary<int, int>()
                                {
                                     {playerRow + i, playerCol + i}
                                };
                                monstersInRange[counter] = (monsterPosition);
                                Console.WriteLine($"{counter++}) target with remaining blood {monster.Health}");
                                break;
                            }
                        }
                    }
                }
            }

            if (counter == 0)
            {
                Console.WriteLine("No available targets in your range.");
            }
            else
            {
                Console.WriteLine("Which one to attack:");
                HitMonster(shadedBlock);
            }
        }

        private void HitMonster(string shadedBlock)
        {
            while (true)
            {
                bool result = int.TryParse(Console.ReadLine(), out int monsterToHit);
                bool isMonsterFound = false;

                if (!result)
                {
                    continue;
                }

                foreach (var kvp in monstersInRange)
                {
                    if (kvp.Key == monsterToHit)
                    {
                        foreach (var monsterCoordinates in monstersPosition)
                        {
                            if (monsterCoordinates.Key.First().Key == kvp.Value.First().Key
                          && monsterCoordinates.Key.First().Value == kvp.Value.First().Value)
                            {
                                var monster = monstersPosition[monsterCoordinates.Key];
                                monster.Health -= player.Damage;

                                if (monster.Health <= 0)
                                {
                                    matrix[kvp.Value.First().Key,
                                        kvp.Value.First().Value] = shadedBlock;

                                    monstersPosition.Remove(monsterCoordinates.Key);
                                }
                            }
                        }
                        isMonsterFound = true;

                        break;
                    }
                }

                if (isMonsterFound)
                {
                    break;
                }
            }
        }

        private void ExitGame()
        {
            Console.WriteLine("You died");
            Console.WriteLine("The game is over");
        }
    }
}
