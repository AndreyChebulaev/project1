using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class FileSystemAnalyzer
{
    // Класс для представления файла или каталога
    public class FileSystemEntry
    {
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; } // Размер файла, для каталогов - 0

        public FileSystemEntry(string path, bool isDirectory, long size = 0)
        {
            Path = path;
            IsDirectory = isDirectory;
            Size = size;
        }

        public override string ToString()
        {
            return $"{(IsDirectory ? "Каталог" : "Файл")}: {Path}{(Size > 0 ? $" ({Size} байт)" : "")}";
        }

        // Переопределяем Equals и GetHashCode для корректного сравнения объектов
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            FileSystemEntry other = (FileSystemEntry)obj;
            return Path == other.Path && IsDirectory == other.IsDirectory && Size == other.Size;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode() ^ IsDirectory.GetHashCode() ^ Size.GetHashCode();
        }
    }

    // Метод для получения списка файлов и каталогов в указанной директории
    public static List<FileSystemEntry> GetFileSystemEntries(string directoryPath)
    {
        List<FileSystemEntry> entries = new List<FileSystemEntry>();

        Console.WriteLine($"    Поиск файлов и каталогов в: {directoryPath}"); // Отладочная печать

        // Получаем список файлов
        try
        {
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);
                    entries.Add(new FileSystemEntry(file, false, fileInfo.Length)); // Size for file
                    Console.WriteLine($"      Найден файл: {file} ({fileInfo.Length} байт)"); // Отладочная печать
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"      Ошибка при получении информации о файле {file}: {ex.Message}");
                    // Обрабатываем ошибки доступа к файлам (например, нет прав)
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Ошибка при получении файлов из {directoryPath}: {ex.Message}");
            // Обрабатываем ошибки доступа к директории
        }

        // Получаем список каталогов
        try
        {
            string[] directories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);
            foreach (string directory in directories)
            {
                entries.Add(new FileSystemEntry(directory, true));  // No size for directories
                Console.WriteLine($"      Найден каталог: {directory}"); // Отладочная печать
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Ошибка при получении каталогов из {directoryPath}: {ex.Message}");
            // Обрабатываем ошибки доступа к директориям
        }

        return entries;
    }

    // Метод для сравнения двух списков файлов и каталогов
    public static void CompareFileSystems(string description, List<FileSystemEntry> before, List<FileSystemEntry> after)
    {
        Console.WriteLine($"\n--- {description} ---");

        // Находим новые и удаленные файлы/каталоги
        var added = after.Except(before).ToList();
        var removed = before.Except(after).ToList();

        if (added.Count > 0)
        {
            Console.WriteLine("\nДобавлено:");
            foreach (var entry in added)
            {
                Console.WriteLine(entry);
            }
        }
        else
        {
            Console.WriteLine("\nНовых файлов или каталогов не добавлено.");
        }

        if (removed.Count > 0)
        {
            Console.WriteLine("\nУдалено:");
            foreach (var entry in removed)
            {
                Console.WriteLine(entry);
            }
        }
        else
        {
            Console.WriteLine("\nФайлы или каталоги не удалены.");
        }

        // Проверяем размеры измененных файлов
        Console.WriteLine("\nИзменены (размер):");
        foreach (var beforeEntry in before)
        {
            if (!beforeEntry.IsDirectory) // Проверяем только файлы
            {
                var afterEntry = after.FirstOrDefault(e => e.Path == beforeEntry.Path);
                if (afterEntry != null && beforeEntry.Size != afterEntry.Size)
                {
                    Console.WriteLine($"Файл: {beforeEntry.Path}");
                    Console.WriteLine($"  Размер до: {beforeEntry.Size} байт");
                    Console.WriteLine($"  Размер после: {afterEntry.Size} байт");
                }
            }
        }

        Console.WriteLine("--- Конец сравнения ---");
    }

    public static void Main(string[] args)
    {
        // Устанавливаем кодировку консоли на UTF-8
        Console.OutputEncoding = Encoding.UTF8;

        // 1.  Запрашиваем пути к директориям у пользователя
        Console.WriteLine("Этап 1: Ввод путей к папкам"); // Отладочная печать
        Console.Write("Введите путь к исходному каталогу: ");
        string beforeDirectory = Console.ReadLine();
        Console.WriteLine($"  Введен путь: {beforeDirectory}"); // Отладочная печать

        Console.Write("Введите путь к обновленному каталогу: ");
        string afterDirectory = Console.ReadLine();
        Console.WriteLine($"  Введен путь: {afterDirectory}"); // Отладочная печать

        // Проверка на пустые пути
        if (string.IsNullOrWhiteSpace(beforeDirectory) || string.IsNullOrWhiteSpace(afterDirectory))
        {
            Console.WriteLine("Ошибка: Пути не могут быть пустыми.");
            Console.ReadKey();
            return;
        }

        // 2.  Получаем списки файлов и каталогов
        Console.WriteLine("Этап 2: Получение списков файлов"); // Отладочная печать
        List<FileSystemEntry> beforeEntries = GetFileSystemEntries(beforeDirectory);
        List<FileSystemEntry> afterEntries = GetFileSystemEntries(afterDirectory);

        // 3.  Сравните файловые системы
        CompareFileSystems("Сравнение файловых систем до и после обновления", beforeEntries, afterEntries);
        Console.ReadKey();
    }
}