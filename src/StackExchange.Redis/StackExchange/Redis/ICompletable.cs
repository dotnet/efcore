using System.Text;

namespace StackExchange.Redis
{
    interface ICompletable
    {
        void AppendStormLog(StringBuilder sb);

        bool TryComplete(bool isAsync);
    }
}
