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
    int wordTypedCount = 0;
    float timeElapsed = 0;
    int charsTyped = 0;
    float timeSinceStart = 0;

    public WPM()
    {
        timesSinceWordFinished = [];
    }

    public void WordTyped(string word)
    {
        timesSinceWordFinished.Add(0);
        charsTyped += word.Length + 1; // Add space
    }

    public void Update()
    {
        timeSinceStart += Raylib.GetFrameTime();
        for (int i = 0; i < timesSinceWordFinished.Count; i++)
        {
            timesSinceWordFinished[i] += Raylib.GetFrameTime();
        }
        timeSinceLastCalculation += Raylib.GetFrameTime();
        timeElapsed += Raylib.GetFrameTime();
    }

    public int GetWPM(bool recalculate = false)
    {
        if (timeSinceLastCalculation >= CalculationInterval || recalculate)
        {
            lastWPM = CalculateWPM();
            timeSinceLastCalculation = 0f;
        }

        return lastWPM;
    }

    // Calculates WPM by taking the average of:
    //  average time to type last {NumWordsConsidered} words
    //  number of words typed in the last {ObservationInterval} seconds
    //  total WPM since lesson start
    int CalculateWPM()
    {
        return (int)MathF.Round(charsTyped / 5f * 60f / timeSinceStart);

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

        float totalWPM = wordTypedCount * 60f / timeElapsed;

        int wpm = (int)MathF.Round((wpmWithinConsidered + wpmWithinObservation + totalWPM) / 3f);

        Log.Trace(Tag, $"Calculated WPM={wpm} | {wpmWithinObservation} {wpmWithinConsidered}");
        return wpm;
    }
}