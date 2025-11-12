namespace NEA.math
{
    public enum SimplexStep : int
    {
        PICK_PIVOT_COLUMN = 1,
        PICK_PIVOT_ROW = 2,
        NORMALISE_ROW = 3,
        APPLY_OTHER = 4,
        CHECK_ARTIFICAL = 5,
        REMOVE_ARTIFICAL = 6,
    }
}
