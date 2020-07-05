namespace Teikoku
{
    /// <summary>
    /// Generic Class for storing the selected UI element
    /// </summary>
    /// <typeparam name="T">Generic Type of item to track as focus</typeparam>
    public class FocusedItem<T>
    {
        public T Property { get; set; }
    }
}
