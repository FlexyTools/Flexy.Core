namespace Flexy.Core;

public interface IService		{ void		OrderedInit			( GameContext ctx );	Int32 Order => 0; }
public interface IServiceAsync	{ UniTask	OrderedInitAsync	( GameContext ctx );	Int32 Order => 0; }