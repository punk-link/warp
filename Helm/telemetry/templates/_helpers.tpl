{{/*
Expand the name of the chart.
*/}}
{{- define "warp-telemetry.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "warp-telemetry.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "warp-telemetry.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "warp-telemetry.labels" -}}
helm.sh/chart: {{ include "warp-telemetry.chart" . }}
{{ include "warp-telemetry.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "warp-telemetry.selectorLabels" -}}
app.kubernetes.io/name: {{ include "warp-telemetry.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
OTEL Collector labels
*/}}
{{- define "otel-collector.labels" -}}
{{ include "warp-telemetry.labels" . }}
app.kubernetes.io/component: otel-collector
{{- end }}

{{/*
OTEL Collector selector labels
*/}}
{{- define "otel-collector.selectorLabels" -}}
{{ include "warp-telemetry.selectorLabels" . }}
app.kubernetes.io/component: otel-collector
{{- end }}

{{/*
Loki labels
*/}}
{{- define "loki.labels" -}}
{{ include "warp-telemetry.labels" . }}
app.kubernetes.io/component: loki
{{- end }}

{{/*
Loki selector labels
*/}}
{{- define "loki.selectorLabels" -}}
{{ include "warp-telemetry.selectorLabels" . }}
app.kubernetes.io/component: loki
{{- end }}

{{/*
Tempo labels
*/}}
{{- define "tempo.labels" -}}
{{ include "warp-telemetry.labels" . }}
app.kubernetes.io/component: tempo
{{- end }}

{{/*
Tempo selector labels
*/}}
{{- define "tempo.selectorLabels" -}}
{{ include "warp-telemetry.selectorLabels" . }}
app.kubernetes.io/component: tempo
{{- end }}

{{/*
Prometheus labels
*/}}
{{- define "prometheus.labels" -}}
{{ include "warp-telemetry.labels" . }}
app.kubernetes.io/component: prometheus
{{- end }}

{{/*
Prometheus selector labels
*/}}
{{- define "prometheus.selectorLabels" -}}
{{ include "warp-telemetry.selectorLabels" . }}
app.kubernetes.io/component: prometheus
{{- end }}

{{/*
Grafana labels
*/}}
{{- define "grafana.labels" -}}
{{ include "warp-telemetry.labels" . }}
app.kubernetes.io/component: grafana
{{- end }}

{{/*
Grafana selector labels
*/}}
{{- define "grafana.selectorLabels" -}}
{{ include "warp-telemetry.selectorLabels" . }}
app.kubernetes.io/component: grafana
{{- end }}
