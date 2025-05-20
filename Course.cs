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
    public string Name = "";
    public LessonType Type;
    public LessonOrder Order;
    public string Prompts = "";

    public List<List<Word>> GetWords()
    {
        List<List<Word>> outputLines = [];
        var rawLines = Prompts.Split("  ");
        foreach (var line in rawLines)
        {
            var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => new Word(x)).ToList();
            if (Order == LessonOrder.Random)
            {
                words = words.OrderBy(x => Guid.NewGuid()).ToList();
            }
            outputLines.Add(words);
        }

        return outputLines;
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