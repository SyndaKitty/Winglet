public class LessonResult
{
    const string Tag = "LessonResult";

    public DateTime Time;
    public string LessonName = "";
    public int WPM;
    public int Mistakes;
    public int LessonHash;

    public string? Serialize()
    {
        if (LessonName.Contains(","))
        {
            Log.Error(Tag, $"Invalid LessonName, contains a comma: \"{LessonName}\"");
            return null;
        }
        return $"{Time:yyyy-MM-dd HH:mm:ss},Lesson: {LessonName},WPM: {WPM},Mistakes: {Mistakes},Hash: {LessonHash}\n";
    }

    public static List<LessonResult> Deserialize(string str)
    {
        List<LessonResult> results = [];

        var lines = str.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var fields = line.Split(",");
            if (fields.Length != 5)
            {
                Log.Error(Tag, $"Invalid format of LessonResult, expected 5 fields: \"{line}\"");
                continue;
            }

            LessonResult r = new();

            for (int i = 0; i < 5; i++)
            {
                var value = fields[i].Split(":").LastOrDefault();
                
                if (value == null)
                {
                    Log.Error(Tag, $"Invalid format of LessonResult, value missing: \"{line}\"");
                    continue;
                }

                value = value.Trim();

                switch (i)
                {
                    case 0:
                        if (!DateTime.TryParse(fields[i], out r.Time))
                        {
                            Log.Error(Tag, $"Invalid format of LessonResult, invalid datetime: \"{line}\"");
                            continue;
                        }
                        break;
                    case 1:
                        r.LessonName = value;
                        break;
                    case 2:
                        if (!int.TryParse(value, out r.WPM))
                        {
                            Log.Error(Tag, $"Invalid format of LessonResult, invalid WPM: \"{line}\"");
                            continue;
                        }
                        break;
                    case 3:
                        if (!int.TryParse(value, out r.Mistakes))
                        {
                            Log.Error(Tag, $"Invlaid format of LessonResult, invalid mistakes: \"{line}\"");
                            continue;
                        }
                        break;
                    case 4:
                        if (!int.TryParse(value, out r.LessonHash))
                        {
                            Log.Error(Tag, $"Invlaid format of LessonResult, invalid lesson Hash: \"{line}\"");
                            continue;
                        }
                        break;
                }
            }

            results.Add(r);
        }
        return results;
    }
}