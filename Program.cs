using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SopaDeLetras
{
    public static Dictionary<string, string> GetRandomWords(string filePath, int count)
    {
        var lines = System.IO.File.ReadAllLines(filePath);
        var entries = new List<(string, string)>();
        foreach (var line in lines)
        {
            var clean = line.Trim('|', ' ', '\t');
            if (string.IsNullOrWhiteSpace(clean)) continue;
            var parts = clean.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            var word = parts[0].Trim().ToUpper();
            var hint = parts[1].Trim();
            entries.Add((word, hint));
        }
        var rnd = new Random();
        var selected = entries.OrderBy(x => rnd.Next()).Take(count);
        var dict = new Dictionary<string, string>();
        foreach (var pair in selected)
            dict[pair.Item1] = pair.Item2;
        return dict;
    }

    private class WordLocation
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Direction { get; set; }
        public int Length { get; set; }
    }

    private char[,] grid;
    private int gridSize;
    private string[] wordsToFind;
    private string[] wordsToFindNormalized;
    private string[] wordHints;
    private bool[] foundWordsStatus;
    private WordLocation[] wordLocations;
    private readonly Random random = new Random();

    public SopaDeLetras(int size, Dictionary<string, string> wordsAndHints)
    {
        gridSize = size;
        wordsToFind = new string[wordsAndHints.Count];
        wordsToFindNormalized = new string[wordsAndHints.Count];
        wordHints = new string[wordsAndHints.Count];
        wordLocations = new WordLocation[wordsAndHints.Count];

        int i = 0;
        foreach (var pair in wordsAndHints)
        {
            wordsToFind[i] = pair.Key.ToUpper();
            wordsToFindNormalized[i] = Normalize(wordsToFind[i]);
            wordHints[i] = pair.Value;
            i++;
        }

        foundWordsStatus = new bool[wordsAndHints.Count];
        grid = new char[gridSize, gridSize];
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
                grid[i, j] = ' ';

        PlaceWords();
        FillEmptySpaces();
    }

    private void PlaceWords()
    {
        string[] shuffledWords = new string[wordsToFind.Length];
        Array.Copy(wordsToFind, shuffledWords, wordsToFind.Length);
        for (int i = 0; i < shuffledWords.Length; i++)
        {
            int j = random.Next(i, shuffledWords.Length);
            (shuffledWords[i], shuffledWords[j]) = (shuffledWords[j], shuffledWords[i]);
        }

        foreach (var word in shuffledWords)
        {
            int attempts = 0;
            bool placed = false;
            while (attempts < 500 && !placed)
            {
                int direction = random.Next(8);
                int startRow = random.Next(gridSize);
                int startCol = random.Next(gridSize);

                if (CanPlaceWord(word, startRow, startCol, direction))
                {
                    PlaceWord(word, startRow, startCol, direction);
                    int originalIndex = Array.IndexOf(wordsToFind, word);
                    wordLocations[originalIndex] = new WordLocation
                    {
                        Row = startRow,
                        Col = startCol,
                        Direction = direction,
                        Length = word.Length
                    };
                    placed = true;
                }
                attempts++;
            }
        }
    }

    private bool CanPlaceWord(string word, int startRow, int startCol, int direction)
    {
        int row = startRow, col = startCol;
        for (int i = 0; i < word.Length; i++)
        {
            if (row < 0 || row >= gridSize || col < 0 || col >= gridSize)
                return false;
            if (grid[row, col] != ' ' && grid[row, col] != word[i])
                return false;
            (row, col) = GetNextPosition(row, col, direction);
        }
        return true;
    }

    private void PlaceWord(string word, int startRow, int startCol, int direction)
    {
        int row = startRow, col = startCol;
        for (int i = 0; i < word.Length; i++)
        {
            grid[row, col] = word[i];
            (row, col) = GetNextPosition(row, col, direction);
        }
    }

    private (int, int) GetNextPosition(int row, int col, int direction)
    {
        return direction switch
        {
            0 => (row, col + 1),
            1 => (row + 1, col + 1),
            2 => (row + 1, col),
            3 => (row + 1, col - 1),
            4 => (row, col - 1),
            5 => (row - 1, col - 1),
            6 => (row - 1, col),
            7 => (row - 1, col + 1),
            _ => (row, col)
        };
    }

    private void FillEmptySpaces()
    {
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
                if (grid[i, j] == ' ')
                    grid[i, j] = (char)('A' + random.Next(26));
    }

    public void PrintGrid()
    {
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("╔═════════════════════════════════╗"); Program.PadToEndOfLine();
    Console.Write("║   S O P A   D E   L E T R A S   ║"); Program.PadToEndOfLine();
    Console.Write("╚═════════════════════════════════╝"); Program.PadToEndOfLine();
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("   ");
    for (int i = 0; i < gridSize; i++) Console.Write($"{i % 10} ");
    Program.PadToEndOfLine();
    Console.ResetColor();
    Console.Write("  " + new string('─', gridSize * 2 + 1)); Program.PadToEndOfLine();

        for (int i = 0; i < gridSize; i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{i.ToString().PadLeft(2)}║");
            Console.ResetColor();

            for (int j = 0; j < gridSize; j++)
            {
                bool isFound = false;
                for (int k = 0; k < wordsToFind.Length; k++)
                {
                    if (!foundWordsStatus[k]) continue;
                    var loc = wordLocations[k];
                    int r = loc.Row, c = loc.Col;
                    for (int l = 0; l < loc.Length; l++)
                    {
                        if (r == i && c == j) { isFound = true; break; }
                        (r, c) = GetNextPosition(r, c, loc.Direction);
                    }
                    if (isFound) break;
                }

                if (isFound)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.Write($" {grid[i, j]}");
                Console.ResetColor();
            }
            Program.PadToEndOfLine();
        }
    }

    public void PrintWordsToFind()
    {
    Program.PadToEndOfLine();
    int found = foundWordsStatus.Count(b => b);
    Console.Write($"Palabras a encontrar:  (Encontradas {found}/{wordsToFind.Length})");
    Program.PadToEndOfLine();
        int columns = 3;
        int wordsPerColumn = (wordsToFind.Length + columns - 1) / columns;
        for (int i = 0; i < wordsPerColumn; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                int index = i + j * wordsPerColumn;
                if (index < wordsToFind.Length)
                {
                    string word = wordsToFind[index];
                    string status = foundWordsStatus[index] ? "✔️" : " ";
                    if (foundWordsStatus[index])
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"[{status}] {word,-15}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write($"[{status}] {word,-15}");
                    }
                }
            }
            Program.PadToEndOfLine();
        }
    }

    public void GiveHint()
    {
        var notFound = new List<int>();
        for (int i = 0; i < foundWordsStatus.Length; i++)
            if (!foundWordsStatus[i]) notFound.Add(i);

        if (notFound.Count == 0)
        {
            Console.WriteLine("¡Ya has encontrado todas las palabras!");
            return;
        }

        int randomIndex = random.Next(notFound.Count);
        int wordIndex = notFound[randomIndex];
        Console.WriteLine($"\nPista: {wordHints[wordIndex]}");
    }

    public void Play()
    {
        int foundCount = 0;

        while (foundCount < wordsToFind.Length)
        {
            PrintGrid();
            PrintWordsToFind();

            Console.WriteLine("\nIngresa una palabra ('pista' o 'salir'):");
            string? input = Console.ReadLine()?.ToUpper();

            if (input == "SALIR") break;
            if (input == "PISTA")
            {
                GiveHint();
                Console.WriteLine("Presiona cualquier tecla para continuar...");
                Console.ReadKey(true);
                continue;
            }

            if (string.IsNullOrWhiteSpace(input)) continue;

            string normalizedInput = Normalize(input);
            int wordIndex = Array.IndexOf(wordsToFindNormalized, normalizedInput);
            if (wordIndex != -1)
            {
                if (foundWordsStatus[wordIndex])
                    Console.WriteLine("¡Ya habías encontrado esa palabra!");
                else
                {
                    foundWordsStatus[wordIndex] = true;
                    foundCount++;
                    Console.WriteLine($"¡Felicidades! Has encontrado la palabra '{input}'!");
                }
            }
            else
            {
                Console.WriteLine("Esa palabra no está en la lista.");
            }

            Console.WriteLine("Presiona cualquier tecla para continuar...");
            Console.ReadKey(true);
        }

        PrintGrid();
        PrintWordsToFind();
        Console.WriteLine("\nSaliendo del juego. ¡Gracias por jugar!");
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey(true);
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        int choice = 0;
        while (choice < 1 || choice > 3)
        {
            Console.WriteLine("¡Bienvenido a la Sopa de Letras!");
            Console.WriteLine("Selecciona un nivel de dificultad:");
            Console.WriteLine("1. Fácil (20x20)");
            Console.WriteLine("2. Medio (30x30)");
            Console.WriteLine("3. Difícil (50x50)");
            Console.Write("Tu opción: ");

            int.TryParse(Console.ReadLine(), out choice);
        }

    SopaDeLetras? game = null;
        switch (choice)
        {
            case 1:
                game = new SopaDeLetras(20, SopaDeLetras.GetRandomWords("easy.txt", 15));
                break;
            case 2:
                game = new SopaDeLetras(30, SopaDeLetras.GetRandomWords("medium.txt", 20));
                break;
            case 3:
                game = new SopaDeLetras(50, SopaDeLetras.GetRandomWords("hard.txt", 30));
                break;
        }
    game!.Play();
    }

    public static void PadToEndOfLine()
    {
        try
        {
            int width = Math.Max(1, Console.WindowWidth);
            int left = Console.CursorLeft;
            if (left < width)
            {
                Console.Write(new string(' ', width - left));
            }
        }
        catch { }
        finally
        {
            try { Console.WriteLine(); } catch { }
        }
    }


}