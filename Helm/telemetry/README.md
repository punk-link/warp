# Warp Telemetry Helm Chart

This Helm chart deploys the complete observability stack for Warp, including:

- **OTEL Collector** - OpenTelemetry Collector for receiving and routing telemetry data
- **Loki** - Log aggregation system
- **Tempo** - Distributed tracing backend (pinned to v2.7.1 due to MetricsGenerator bug in v2.10.0)
- **Prometheus** - Metrics storage and querying
- **Grafana** - Unified visualization dashboard with pre-configured datasources

## Prerequisites

- Kubernetes 1.19+
- Helm 3.0+
- **Storage Class**: A StorageClass must be available in your cluster for persistent volumes

### Storage Requirements

The chart requires persistent storage for:
- **Loki**: 10Gi (dev) / 50Gi (prod)
- **Tempo**: 10Gi (dev) / 100Gi (prod)
- **Prometheus**: 20Gi (dev) / 200Gi (prod)
- **Grafana**: 1Gi (dev) / 10Gi (prod)

**Development**: Uses `local-path` StorageClass. Install the provisioner if not available:
```bash
kubectl apply -f https://raw.githubusercontent.com/rancher/local-path-provisioner/v0.0.34/deploy/local-path-storage.yaml
```

**Production**: Configure your cluster's StorageClass in `values_prod.yaml`:
- AWS EKS: `gp3` or `gp2`
- GCP GKE: `standard-rwo` or `premium-rwo`
- Azure AKS: `managed-premium` or `managed`
- On-premise: Your custom StorageClass

## Installation

### Development Environment

Deploy the telemetry stack to the `observability` namespace:

```bash
# Install local-path provisioner (if not already present)
kubectl apply -f https://raw.githubusercontent.com/rancher/local-path-provisioner/v0.0.30/deploy/local-path-storage.yaml

# Install the chart
helm install warp-telemetry ./Helm/telemetry \
  -n observability \
  --create-namespace \
  -f ./Helm/telemetry/values_dev.yaml
```

**Note:** If you get a namespace ownership error, delete the existing namespace first:
```bash
kubectl delete namespace observability
helm install warp-telemetry ./Helm/telemetry -n observability --create-namespace -f ./Helm/telemetry/values_dev.yaml
```

### Production Environment

1. **Configure storage and ingress** in `values_prod.yaml`:
   - Set `storageClassName` for each service (Loki, Tempo, Prometheus, Grafana)
   - Set `grafana.ingress.host` to your production domain
   - Ensure TLS certificate exists or configure cert-manager

2. **Deploy**:
```bash
helm install warp-telemetry ./Helm/telemetry \
  -n observability \
  --create-namespace \
  -f ./Helm/telemetry/values_prod.yaml
```

### Verify Deployment

```bash
kubectl get pods -n observability
kubectl get pvc -n observability
kubectl get svc -n observability
```

## Configuration

All configuration is managed through values files. See [values_dev.yaml](values_dev.yaml) for development defaults.

### Key Configuration Options

| Parameter | Description | Default |
|-----------|-------------|---------|
| `namespace` | Target namespace | `observability` |
| `otelCollector.enabled` | Enable OTEL Collector | `true` |
| `loki.storage.size` | Loki PVC size | `10Gi` |
| `tempo.storage.size` | Tempo PVC size | `10Gi` |
| `prometheus.storage.size` | Prometheus PVC size | `20Gi` |
| `prometheus.retention.time` | Metrics retention | `7d` |
| `grafana.auth.anonymousEnabled` | Enable anonymous access | `true` (dev only) |
| `grafana.ingress.enabled` | Expose Grafana via Ingress | `false` |

### Storage Classes

By default, the chart uses the cluster's default StorageClass. Override per service:

```yaml
loki:
  storage:
    storageClassName: "fast-ssd"
```

## Accessing Grafana

### Port Forward (Development)

```bash
kubectl port-forward -n observability svc/grafana 3000:3000
```

Then access at http://localhost:3000

### Ingress (Production)

Enable ingress in your values file:

```yaml
grafana:
  ingress:
    enabled: true
    host: grafana.example.com
    tls:
      enabled: true
      secretName: grafana-tls
```

## Integration with Warp Application

The Warp application expects the OTEL Collector at:

```
otel-collector.observability.svc.cluster.local:4317
```

This is pre-configured in the existing Warp Helm chart deployment.

## Uninstallation

```bash
helm uninstall warp-telemetry -n observability
```

**Note:** PersistentVolumeClaims and the namespace are not automatically deleted. Remove them manually if needed:

```bash
# Delete PVCs
kubectl delete pvc -n observability --all

# Optionally delete the namespace
kubectl delete namespace observability
```

## Troubleshooting

### Check OTEL Collector logs
```bash
kubectl logs -n observability -l app.kubernetes.io/component=otel-collector
```

### Check Tempo traces ingestion
```bash
kubectl logs -n observability -l app.kubernetes.io/component=tempo
```

### Verify Prometheus targets
Port-forward and check http://localhost:9090/targets:
```bash
kubectl port-forward -n observability svc/prometheus 9090:9090
```

### Check Loki logs ingestion
Port-forward Grafana and query Loki datasource:
```bash
kubectl port-forward -n observability svc/grafana 3000:3000
```
