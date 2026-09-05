public static class IncrementWithOverflow {
    /// <summary>
    /// Increments the passed currentIndex with overflow, safe for looping indices pointing to lists.
    /// Uses an out parameter.
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="totalCount"></param>
    /// <param name="change"></param>
    /// <param name="result"></param>
    public static void Run(int currentIndex, int totalCount, int change, out int result) {
        result = currentIndex + change;
        result = (result % totalCount + totalCount) % totalCount;
    }
   
    /// <summary>
    /// Increments the passed currentIndex with overflow, safe for looping indices pointing to lists.
    /// Returns the result.
    /// </summary>
    /// <param name="currentIndex"></param>
    /// <param name="totalCount"></param>
    /// <param name="change"></param>
    /// <returns></returns>
    public static int Run(int currentIndex, int totalCount, int change) {
        Run(currentIndex, totalCount, change, out int result);
        return result;
    }
}