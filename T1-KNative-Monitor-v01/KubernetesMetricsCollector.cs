using k8s;
using Prometheus;
using System.Linq;

public class KubernetesMetricsCollector
{
    private readonly IKubernetes _client;

    private readonly Gauge _podCount = Metrics.CreateGauge(
        "k8s_pods_total", "Total number of pods in the cluster");

    private readonly Gauge _nodeCount = Metrics.CreateGauge(
        "k8s_nodes_total", "Total number of nodes in the cluster");

    private readonly Gauge _namespaceCount = Metrics.CreateGauge(
        "k8s_namespaces_total", "Total number of namespaces in the cluster");

    private readonly Gauge _podsPerNamespace = Metrics.CreateGauge(
        "k8s_pods_per_namespace",
        "Number of pods per namespace",
        new GaugeConfiguration
        {
            LabelNames = new[] { "namespace" }
        });

    public KubernetesMetricsCollector(IKubernetes client)
    {
        _client = client;
    }

    public async Task CollectAsync(CancellationToken ct = default)
    {
        var pods = await _client.CoreV1.ListPodForAllNamespacesAsync(cancellationToken: ct);
        var nodes = await _client.CoreV1.ListNodeAsync(cancellationToken: ct);
        var namespaces = await _client.CoreV1.ListNamespaceAsync(cancellationToken: ct);

        // Update Prometheus metrics
        _podCount.Set(pods.Items.Count);
        _nodeCount.Set(nodes.Items.Count);
        _namespaceCount.Set(namespaces.Items.Count);

        var perNs = pods.Items
            .GroupBy(p => p.Metadata?.NamespaceProperty ?? "unknown")
            .Select(g => new { Namespace = g.Key, Count = g.Count() });

        foreach (var entry in perNs)
        {
            _podsPerNamespace.WithLabels(entry.Namespace).Set(entry.Count);
        }

        // Print to CLI
        Console.WriteLine($"[K8s] Pods={pods.Items.Count}, Nodes={nodes.Items.Count}, Namespaces={namespaces.Items.Count}");
        foreach (var entry in perNs)
        {
            Console.WriteLine($"[K8s] namespace={entry.Namespace} pods={entry.Count}");
        }
    }
}
