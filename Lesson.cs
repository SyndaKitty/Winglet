public class Lesson
{
    public List<Word>? Words;
    const string Tag = "Lesson";
    
    public static Lesson? Load(string fileName)
    {
        Log.Info(Tag, $"Loading lesson from: {fileName}");

        List<Word> words = [];
        try
        {
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                Word? word = LoadWord(line);
                if (word is not null)
                {
                    words.Add(word);
                }
            }

            Log.Info(Tag, $"Loaded {words.Count} words");

            return new Lesson 
            {
                Words = words
            };
        }
        catch
        {
            return null;
        }
    }
    
    static Word? LoadWord(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1 || parts.Length > 2)
        {
            Log.Error(Tag, $"Could not load word from line: {line}");
            return null;
        }

        if (parts.Length == 1)
        {
            return new Word(parts[0], null);
        }

        return new Word(parts[0], parts[1]);
    }
}