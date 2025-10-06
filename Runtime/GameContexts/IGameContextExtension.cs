namespace Flexy.Core.GameContexts;

public interface IGameContextExtension
{
	void SetParent(GameContext parent);
	void RegisterAdditionalServices( Dictionary<Type, Object> registeredServicesDict );
	T GetService<T>() where T : class;
}