using k8s;
using k8s.Models;

public class KnativeStatusCollector
{
    private readonly IKubernetes _client;

    public KnativeStatusCollector(IKubernetes client)
    {
        _client = client;
    }

    public async Task CollectAsync()
    {
        // Knative services are CRDs under group "serving.knative.dev"
        var knativeServices = await _client.CustomObjects.ListClusterCustomObjectAsync(
            group: "serving.knative.dev",
            version: "v1",
            plural: "services");

        Console.WriteLine($"[Knative] Services object: {knativeServices}");
    }
}
