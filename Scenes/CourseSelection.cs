using Raylib_cs;
using System.Numerics;

public class CourseSelection : Scene
{
    const string CoursePath = "Resources/Courses";
    
    public const string Tag = "CourseSelection";

    Font primaryFont;
    List<Course> courses = [];
    int selectedIndex = 2;

    Color standardColor = Shared.TextColor;
    Color selectedColor = Shared.AltTextColor;

    public void Load()
    {
        primaryFont = Shared.GetFont(Shared.PrimaryFontFile, 60);

        var courseFiles = Directory.GetFiles(CoursePath, "*.yaml", SearchOption.TopDirectoryOnly)
            .OrderBy(x => x);
        
        foreach (var courseFile in courseFiles)
        {
            Course? c = Course.Load(courseFile);
            if (c != null)
            {
                courses.Add(c);
            }
        }
    }

    public void Unload() { }

    public void Update()
    {
        KeyInput();
    }
    
    public void Draw()
    {
        Raylib.ClearBackground(Shared.BackgroundColor);

        const int verticalPadding = 10;

        Vector2 cursor = new Vector2(0, 0);
        cursor.X = Raylib.GetScreenWidth() * .25f;
        cursor.Y = Raylib.GetScreenHeight() / 2 - primaryFont.BaseSize / 2;
        cursor.Y -= selectedIndex * (primaryFont.BaseSize + verticalPadding);
        
        for (int i = 0; i < courses.Count; i++)
        {
            var course = courses[i];
            Color color = (i == selectedIndex) ? selectedColor : standardColor;
            Util.DrawText(primaryFont, course.Name, cursor, color);
            cursor.Y += primaryFont.BaseSize + verticalPadding;
        }

    }

    void KeyInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            Window.SetScene(new PracticeScene(courses[selectedIndex].Lessons[0]));
        }
    }
}