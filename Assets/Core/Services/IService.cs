
using System.Threading.Tasks;

internal interface IService
{
	public abstract Task Setup();
	public abstract Task Start();
	public abstract Task Stop();
}