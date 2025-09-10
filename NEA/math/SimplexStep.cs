namespace NEA.math
{
    public enum SimplexStep : int
    {
        PICK_PIVOT_COLUMN = 0,
        PICK_PIVOT_ROW = 1,
        NORMALISE_ROW = 2,
        APPLY_OTHER = 4,
        CHECK_ARTIFICAL = 5,
        REMOVE_ARTIFICAL = 6,
    }
}
