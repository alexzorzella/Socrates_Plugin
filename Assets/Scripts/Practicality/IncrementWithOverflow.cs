public static class IncrementWithOverflow {
    public static void Run(int currentInd, int totalCount, int change, out int result) {
        result = currentInd + change;

        if (result >= totalCount)
            result = 0;
        else if (result < 0) result = totalCount - 1;
    }
    
    public static int Run(int currentInd, int totalCount, int change) {
        int result = currentInd + change;

        if (result >= totalCount) {
            result = 0;
        } else if (result < 0) {
            result = totalCount - 1;
        }

        return result;
    }
}