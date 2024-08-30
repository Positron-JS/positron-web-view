namespace NeuroSpeech.Positron;

public interface IJSProxy
{

    IJSValue Get(IJSContext context, IJSValue @this, IList<IJSValue> args);

    IJSValue Set(IJSContext context, IJSValue @this, IList<IJSValue> args);

}