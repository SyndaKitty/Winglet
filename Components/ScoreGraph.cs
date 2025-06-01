using Raylib_cs;
using System.Numerics;

public class ScoreGraph
{
    List<LessonResult> scores;
    CourseLesson lesson;
    int wordCount;
    List<(int wpm, float acc)> scorePoints;
    Font font;

    public ScoreGraph(List<LessonResult> scores, CourseLesson lesson)
    {
        this.scores = scores
            .Where(x => x.LessonHash == lesson.GetHashCode())
            .OrderBy(x => x.Time)
            .ToList();

        this.lesson = lesson;
        wordCount = lesson.GetWords().Count();
        scorePoints = this.scores
            .Select(x => (x.WPM, 1f - (float)x.Mistakes / wordCount))
            .ToList();
    }

    public void Load()
    {
        font = Shared.GetFont(Shared.PrimaryFontFile, 20);
    }

    public void SetFont(Font font)
    {
        this.font = font;
    }

    public void Draw(Vector2 pos, Vector2 size)
    {
        if (scorePoints.Count == 0) return;

        var minWPM = scorePoints.Min(s => s.wpm);
        var maxWPM = scorePoints.Max(s => s.wpm);

        var startWPM = (int)MathF.Floor(minWPM / 10f) * 10;
        startWPM = (int)MathF.Min(startWPM, 50);
        var endWPM = (int)MathF.Ceiling(maxWPM / 10f) * 10;

        var minAccuracy = scorePoints.Min(s => s.acc);
        var maxAccuracy = scorePoints.Max(s => s.acc);
        var startAccuracy = MathF.Floor(minAccuracy * 10f) / 10f;
        startAccuracy = MathF.Min(startAccuracy, .8f);

        var endAccuracy = MathF.Ceiling(maxAccuracy * 10f) / 10f;
        if (endAccuracy > 1f)
        {
            endAccuracy = 1f;
        }


        Vector2 topLeft = pos;
        Vector2 bottomRight = pos + size;
        Vector2Int? lastPoint = null;

        for (int i = 0; i < scorePoints.Count; i++)
        {
            var s = scorePoints[i];
            var x = (int)Util.Map(s.wpm, startWPM, endWPM, topLeft.X, bottomRight.X);
            var y = (int)Util.Map(s.acc, startAccuracy, endAccuracy, bottomRight.Y, topLeft.Y);
            var p = new Vector2(x, y);

            Raylib.DrawRectanglePro(new Rectangle(x, y, 10, 10), new(5, 5), 45f, Color.White);
            //Raylib.DrawRectangle(x, y, 3, 3, Color.White);

            if (lastPoint != null)
            {
                var l = lastPoint.Value;
                Raylib.DrawLine(l.X, l.Y, x, y, Color.White);
            }
            lastPoint = new Vector2Int(x, y);
        }

        for (int w = startWPM; w < endWPM; w += 10)
        {
            var x = (int)Util.Map(w, startWPM, endWPM, topLeft.X, bottomRight.X);
            Raylib.DrawLine(x, (int)bottomRight.Y, x, (int)topLeft.Y, Color.White);
            string text = w.ToString();
            Util.DrawText(font, text, new Vector2(x, bottomRight.Y - 20), Color.White);
        }

        for (float a = startAccuracy; a < endAccuracy; a += .1f)
        {
            var y = (int)Util.Map(a, startAccuracy, endAccuracy, bottomRight.Y, topLeft.Y);
            Raylib.DrawLine((int)topLeft.X, y, (int)bottomRight.X, y, Color.White);
        }

        Raylib.DrawRectangleLinesEx(new Rectangle(pos, size), 1f, Color.White);
    }
}