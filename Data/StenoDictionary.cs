using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

public class StenoDictionary
{
    const string Tag = "StenoDictionary";
    
    Dictionary<string, List<StrokeList>> Translations;
    Dictionary<StrokeList, string> Lookup;

    public StenoDictionary()
    {
        Translations = new();
        Lookup = new();
    }

    public void Clear()
    {
        Translations.Clear();
        Lookup.Clear();
    }

    public List<StrokeList> GetTranslations(string word)
    {
        if (Translations.ContainsKey(word))
        {
            return Translations[word];
        }
        return [];
    }

    public bool TryTranslate(StrokeList strokeList, out string translation)
    {
        if (Lookup.ContainsKey(strokeList))
        {
            translation = Lookup[strokeList];
            return true;
        }
        translation = "";
        return false;
    }

    public bool LoadFromJson(string filepath)
    {
        try
        {
            string rawJson = File.ReadAllText(filepath);
            
            var json = JsonConvert.DeserializeObject(rawJson) as JObject;
            if (json is null)
            {
                Log.Error(Tag, $"Unable to deserialize json file {filepath}");
                return false;
            }

            foreach (var child in json)
            {
                string outputWord = child.Value?.ToString() ?? "";

                var strokeList = new StrokeList(child.Key);

                if (Lookup.ContainsKey(strokeList))
                {
                    Log.Error(Tag, $"Dictionary conflict. Multiple definitions for {child.Key}");
                }
                else
                {
                    Lookup.Add(strokeList, outputWord);
                }

                if (!Translations.ContainsKey(outputWord))
                {
                    Translations.Add(outputWord, new());
                }
                Translations[outputWord].Add(strokeList);
            }
            return true;

        }
        catch (Exception e)
        {
            Log.Error(Tag, $"Failed to read file at {filepath}: {e}");
            return false;
        }
    }

    public bool LoadFromPlover(PloverServer server)
    {
        // TODO: Not currently possible with current Plover server plugin.
        // It does not support getting translations
        throw new NotImplementedException();
    }
}

public struct StrokeList : IEquatable<StrokeList>
{
    string rawStroke;
    List<string> strokes;

    public StrokeList(string strokes)
    {
        rawStroke = strokes;
        this.strokes = [.. strokes.Split('/')];
    }

    public override int GetHashCode()
    {
        return rawStroke.GetHashCode();
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is StrokeList other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(StrokeList other)
    {
        return rawStroke == other.rawStroke;
    }

    public override string ToString()
    {
        return rawStroke;
    }
}