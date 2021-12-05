using System;

namespace PFEditor
{
    /// <summary>
    /// Изключениеето идва от фигури, когато се опитвате да извикате метода Draw с неправилни параметри
    /// </summary>
    [Serializable]
    class ShapeException : Exception
    {
        public ShapeException(string message) : base(message) { }
    }
}
