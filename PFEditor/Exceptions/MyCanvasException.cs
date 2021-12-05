using System;

namespace PFEditor
{
    /// <summary>
    ///Отхвърля се, когато е невъзможно да се създаде, инициализирайте листа/платното
    /// </summary>
    [Serializable]
    class MyCanvasException : Exception
    {
        public MyCanvasException(string message) : base(message) { }
    }
}
