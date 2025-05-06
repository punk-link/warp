namespace Warp.WebApp.Attributes;


[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class TraceMethodAttribute : Attribute
{
    public TraceMethodAttribute() 
    { }


    public TraceMethodAttribute(List<KeyValuePair<string, string>> tags) 
    {
        Tags = tags;
    }


    public TraceMethodAttribute(string notes)
    {
        Tags.Add(new KeyValuePair<string, string>("trace.notes", notes));
    }


    public List<KeyValuePair<string, string>> Tags { get; } = [];
}