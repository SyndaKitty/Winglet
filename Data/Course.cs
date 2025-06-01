using YamlDotNet.Serialization;

public class Course
{
    public string Name = "";
    public string Description = "";
    public List<CourseLesson> Lessons = [];

    public static Course? Load(string path)
    {
        IDeserializer deserializer = new Deserializer();
        try
        {
            return deserializer.Deserialize<Course>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Log.Error("Course", $"Unable to load file at '{path}\n{e}'");
            return null;
        }
    }
}

public class CourseLesson
{
    public string? Name { get; set; }
    public LessonType Type { get; set; }
    public LessonOrder Order { get; set; }
    public string? Prompts { get; set; }
    public int RepeatCount { get; set; } = 1;
    public LessonSettings Settings { get; set; }

    public List<string> GetWords()
    {
        List<string> finalList = [];
        for(int i = 0; i < RepeatCount; i++)
        {
            var words = Prompts?.Split(" ", StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (Order == LessonOrder.Random)
            {
                Util.Shuffle(words);
                ApplyEasterEggs(words);
            }
            finalList.AddRange(words);
        }

        return finalList;
    }

    public override int GetHashCode()
    {
        // Used when logging lessons, to help ensure lesson content has not changed
        return Util.ConsistentStringHash(Prompts);
    }

    void ApplyEasterEggs(string[] words)
    {
        ApplyEasterEgg(words, .25f, "big", "slut");
        ApplyEasterEgg(words, .40f, "head", "pat");
        ApplyEasterEgg(words, .3f, "live", "laugh", "love");
        ApplyEasterEgg(words, .7f, "good", "girl");
        ApplyEasterEgg(words, .7f, "good", "boy");
        ApplyEasterEgg(words, .5f, "fem", "boy");
        ApplyEasterEgg(words, 1f, "trans", "rights");
        ApplyEasterEgg(words, .5f, "egg", "crack");
    }

    void ApplyEasterEgg(string[] words, float chance, params string[] list)
    {
        if (list.Length == 0) return;

        if (Util.RandomChance(chance) && list.All(w => words.Contains(w)))
        {
            var w = words.ToList();
            int firstIndex = w.IndexOf(list[0]);

            if (firstIndex >= words.Length - (list.Length - 1)) return;
            for (int i = 1; i < list.Length; i++)
            {
                var idx = w.IndexOf(list[i]);
                (words[firstIndex + i], words[idx]) = (words[idx], words[firstIndex + i]);
            }
        }
    }
}

public enum LessonType
{
    Raw,
    Words,
}

public enum LessonOrder
{
    Ordered,
    Random,
}

public struct LessonSettings
{
    public bool StrictSpaces { get; set; }
    public bool StrictSymbols { get; set; }
    public bool StrictCase { get; set; }
    public bool OnlyAdvanceWhenNextWordCorrect { get; set; }

    public static LessonSettings Default => new LessonSettings 
    {
        StrictSpaces = false,
        StrictSymbols = false,
        StrictCase = false,
        OnlyAdvanceWhenNextWordCorrect = true,
    };
}