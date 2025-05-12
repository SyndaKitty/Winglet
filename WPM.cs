using Raylib_cs;

public class WPM 
{
    public const float CalculationInterval = .5f;
    public const float ObservationInterval = 10f;
    public const int NumWordsConsidered = 5;
    const int DefragListCount = 32;
    const string Tag = "WPM";

    List<float> timesSinceWordFinished;
    
    float timeSinceLastCalculation = 0f;
    int lastWPM = 0;

    public WPM()
    {
        timesSinceWordFinished = [];
    }

    public void WordTyped(Word word)
    {
        timesSinceWordFinished.Add(0);
    }

    public void Update()
    {
        for (int i = 0; i < timesSinceWordFinished.Count; i++)
        {
            timesSinceWordFinished[i] += Raylib.GetFrameTime();
        }
        timeSinceLastCalculation += Raylib.GetFrameTime();
    }

    public int GetWPM()
    {
        if (timeSinceLastCalculation >= CalculationInterval)
        {
            lastWPM = CalculateWPM();
            timeSinceLastCalculation = 0f;
        }

        return lastWPM;
    }

    // Calculates WPM by taking the average of:
    //  average time to type last {NumWordsConsidered} words
    //  number of words typed in the last {ObservationInterval} seconds
    int CalculateWPM()
    {
        float count = 0;
        int firstValidIndex = -1;
        for (int i = 0; i < timesSinceWordFinished.Count; i++)
        {
            if (timesSinceWordFinished[i] < ObservationInterval)
            {
                if (firstValidIndex == -1)
                {
                    firstValidIndex = i;
                }
                count += 1;
            }
        }

        float wpmWithinObservation = count * 60f / ObservationInterval;


        float wpmWithinConsidered;
        int wordConsideredIndex = Math.Max(0, timesSinceWordFinished.Count - NumWordsConsidered);
        int wordsConsidered = timesSinceWordFinished.Count - wordConsideredIndex;
        if (wordsConsidered == 0)
        {
            wpmWithinConsidered = 0f;
        }
        else
        {
            wpmWithinConsidered = wordsConsidered / timesSinceWordFinished[wordConsideredIndex] * 60f;
        }

        // If the first {DefragListCount} elements are all stale, remove them
        if (firstValidIndex >= DefragListCount)
        {
            timesSinceWordFinished = timesSinceWordFinished.Skip(firstValidIndex).ToList();
        }

        int wpm = (int)MathF.Round((wpmWithinConsidered + wpmWithinObservation) * 0.5f);

        Log.Trace(Tag, $"Calculated WPM={wpm} | {wpmWithinObservation} {wpmWithinConsidered}");
        return wpm;
    }
}