using System.Collections;

namespace PacotePenseCre.Generics
{
    /// <summary>
    /// Interface which all manager scripts should implement, so it's easier to find them and call Init without casting
    /// </summary>
    public interface IManager
    {
        IEnumerator Init();
    }
}