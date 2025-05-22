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